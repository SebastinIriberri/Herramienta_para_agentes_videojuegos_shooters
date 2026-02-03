using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour {
    const float minPathUpdateTime = .2f;

    [Header("Repath (anti-spam)")]
    [Tooltip("Tiempo mínimo entre recalcular rutas para el MISMO target.")]
    public float repathCooldown = 0.25f;

    [Tooltip("Si el target no se movió más que esto, no se recalcula.")]
    public float targetMoveThreshold = 0.5f;

    [Tooltip("Si nosotros no avanzamos nada en este tiempo, forzar repath.")]
    public float stuckRepathSeconds = 1.5f;

    public Transform target;
    public float speed = 3.5f;
    public float turnSpeed = 6f;
    public float turnDst = 5f;
    public float stoppingDst = 1.25f;

    Path path;
    public System.Action OnDestinationReached;
    public float CurrentSpeed { get; private set; }
    public bool HasReachedDestination { get; private set; }

    bool _movementSuspended = false;
    Transform _resumeTarget = null;

    Transform _followTarget;
    Vector3 _lastTargetPos;
    float _lastRepathTime;

    Vector3 _lastPos;
    float _stuckTimer;

    Coroutine _followRoutine;
    Coroutine _updatePathRoutine;
    Rigidbody _rb;

    void Awake(){
        _rb = GetComponent<Rigidbody>();
    }
    public void ConfigureMovement(float spd, float turnSpd, float stopDst, float turnDistance) {
        speed = spd;
        turnSpeed = turnSpd;
        stoppingDst = stopDst;
        turnDst = turnDistance;
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful) {
        if (!pathSuccessful || waypoints == null || waypoints.Length == 0) return;
        path = new Path(waypoints, transform.position, turnDst, stoppingDst);

        if (!isActiveAndEnabled || !gameObject.activeInHierarchy) return;

        if (_followRoutine != null) StopCoroutine(_followRoutine);
        _followRoutine = StartCoroutine(FollowPath());
    }

    IEnumerator UpdatePath() {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy) yield break;

        if (Time.timeSinceLevelLoad < .3f) yield return new WaitForSeconds(.3f);

        RequestRepath();

        float sqrMoveThreshold = targetMoveThreshold * targetMoveThreshold;
        Vector3 targetPosOld = _followTarget ? _followTarget.position : Vector3.positiveInfinity;

        while (isActiveAndEnabled && gameObject.activeInHierarchy) {
            if (_movementSuspended)
            {
                yield return null;
                continue;
            }


            yield return new WaitForSeconds(minPathUpdateTime);

            if (!_followTarget) continue;

            if ((_followTarget.position - targetPosOld).sqrMagnitude > sqrMoveThreshold) {
                TryRepath();
                targetPosOld = _followTarget.position;
            }
        }
    }

    void RequestRepath() {
        if (!_followTarget) return;
        PathRequestManager.RequestPath(new PathRequest(transform.position, _followTarget.position, OnPathFound));
        _lastRepathTime = Time.time;
        _lastTargetPos = _followTarget.position;
    }

    void TryRepath() {
        if (Time.time - _lastRepathTime < repathCooldown) return;
        if ((_followTarget.position - _lastTargetPos).sqrMagnitude < targetMoveThreshold * targetMoveThreshold) return;

        RequestRepath();
    }

    IEnumerator FollowPath()
    {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy) yield break;

        int pathIndex = 0;

        while (isActiveAndEnabled && gameObject.activeInHierarchy)
        {
            if (_movementSuspended)
            {
                CurrentSpeed = 0f;
                yield return null;
                continue;
            }

            if (path == null || path.lookPoints == null || path.lookPoints.Length == 0)
            {
                CurrentSpeed = 0f;
                yield return null;
                continue;
            }

            // Si ya no hay puntos, terminamos
            if (pathIndex >= path.lookPoints.Length)
            {
                HasReachedDestination = true;
                CurrentSpeed = 0f;
                OnDestinationReached?.Invoke();
                yield break;
            }

            Vector3 targetPoint = path.lookPoints[pathIndex];
            targetPoint.y = transform.position.y; // mantener altura

            // Rotación hacia el waypoint
            Vector3 dir = (targetPoint - transform.position);
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
                float y = Mathf.LerpAngle(transform.eulerAngles.y, look.eulerAngles.y, Time.deltaTime * turnSpeed);
                transform.rotation = Quaternion.Euler(0f, y, 0f);
            }

            // Movimiento hacia el waypoint (NO forward)
            float stepDist = speed * Time.deltaTime;
            Vector3 nextPos = Vector3.MoveTowards(transform.position, targetPoint, stepDist);

            if (_rb != null)
            {
                _rb.MovePosition(nextPos);
            }
            else
            {
                transform.position = nextPos;
            }

            CurrentSpeed = speed;
            HasReachedDestination = false;

            // Si llegamos cerca del waypoint, avanzamos al siguiente
            float arriveDist = 0.2f; // ajusta: 0.1 - 0.4 según tu escala
            if (Vector3.Distance(transform.position, targetPoint) <= arriveDist)
            {
                pathIndex++;

                // si era el último waypoint, ya llegamos
                if (pathIndex >= path.lookPoints.Length)
                {
                    HasReachedDestination = true;
                    CurrentSpeed = 0f;
                    OnDestinationReached?.Invoke();
                    yield break;
                }
            }

            yield return null;
        }
    }


    public void StartFollowing(Transform newTarget) {
        if (!newTarget) return;
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy) return;

        var h = GetComponent<Health>();
        if (h != null && h.IsDead) return;

        // si es el mismo target, NO reinicies corutinas
        if (_followTarget == newTarget) return;

        _followTarget = newTarget;
        HasReachedDestination = false;

        if (_updatePathRoutine != null) StopCoroutine(_updatePathRoutine);
        _updatePathRoutine = StartCoroutine(UpdatePath());
    }

    public void StopFollowing() {
        if (_updatePathRoutine != null) { StopCoroutine(_updatePathRoutine); _updatePathRoutine = null; }
        if (_followRoutine != null) { StopCoroutine(_followRoutine); _followRoutine = null; }

        CurrentSpeed = 0f;
        HasReachedDestination = true;
        _followTarget = null;
    }

    public void SuspendMovement(bool suspend)
    {
        _movementSuspended = suspend;

        if (suspend)
        {
            // Guardamos target actual para reanudar luego
            _resumeTarget = _followTarget;

            // Detenemos corutinas de movimiento pero no "olvidamos" el target
            if (_updatePathRoutine != null) { StopCoroutine(_updatePathRoutine); _updatePathRoutine = null; }
            if (_followRoutine != null) { StopCoroutine(_followRoutine); _followRoutine = null; }
            _followTarget = null;
            CurrentSpeed = 0f;
            HasReachedDestination = true;
        }
        else
        {
            // Reanudar si había target
            if (_resumeTarget != null)
            {
                StartFollowing(_resumeTarget);
                _resumeTarget = null;
            }
        }
    }

    public void OnDrawGizmos() {
        if (path != null) {
            var em = GetComponent<EnemyManager>();
            if (em != null) {
                path.DrawWithGizmos(
                    em.debugDrawPath,
                    em.debugDrawTurnLines,
                    em.debugDrawLookPoints,
                    em.debugColorPath,
                    em.debugColorTurnLines,
                    em.debugColorLookPoints
                );
            }
        }
    }
}
