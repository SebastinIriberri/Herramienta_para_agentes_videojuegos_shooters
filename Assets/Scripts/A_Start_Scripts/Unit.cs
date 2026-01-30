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

    IEnumerator FollowPath() {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy) yield break;

        bool followingPath = true;
        int pathIndex = 0;
        float speedPercent = 1f;

        Vector3 firstDir = path.lookPoints[0] - transform.position;
        firstDir.y = 0f;
        if (firstDir.sqrMagnitude > 0.0001f) { 
            Quaternion r = Quaternion.LookRotation(firstDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, r, Time.deltaTime * turnSpeed);
        }

        _lastPos = transform.position;
        _stuckTimer = 0f;

        while (isActiveAndEnabled && gameObject.activeInHierarchy) {
            if (_movementSuspended)
            {
                CurrentSpeed = 0f;
                yield return null;
                continue;
            }
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);

            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) {
                if (pathIndex == path.finishLineIndex) { followingPath = false; break; }
                pathIndex++;
            }

            if (followingPath) {
                HasReachedDestination = false;

                if (pathIndex >= path.slowDownIndex && stoppingDst > 0f) {
                    float distToFinish = path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D);
                    speedPercent = Mathf.Clamp01(distToFinish / stoppingDst);

                    if (speedPercent < 0.1f ||
                        Vector3.Distance(transform.position, path.lookPoints[path.finishLineIndex]) <= stoppingDst) {
                        HasReachedDestination = true;
                        CurrentSpeed = 0f;
                        OnDestinationReached?.Invoke();
                        yield break;
                    }
                }

                Vector3 direction = path.lookPoints[pathIndex] - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.01f) {
                    Quaternion targetRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
                    float yRot = Mathf.LerpAngle(transform.eulerAngles.y, targetRot.eulerAngles.y, Time.deltaTime * turnSpeed);
                    transform.rotation = Quaternion.Euler(0f, yRot, 0f);
                }

                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
                CurrentSpeed = speed * speedPercent;

                float moved = Vector3.Distance(transform.position, _lastPos);
                if (moved < 0.02f) _stuckTimer += Time.deltaTime;
                else _stuckTimer = 0f;

                if (_stuckTimer > stuckRepathSeconds) {
                    TryRepath();
                    _stuckTimer = 0f;
                }

                _lastPos = transform.position;
            }
            else {
                HasReachedDestination = true;
                CurrentSpeed = 0f;
                yield break;
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
