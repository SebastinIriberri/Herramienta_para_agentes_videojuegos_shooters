using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour {
    // === Parámetros de actualización de path ===
    const float minPathUpdateTime = .2f;

    [Header("Repath (anti-spam)")]
    [Tooltip("Tiempo mínimo entre recalcular rutas para el MISMO target.")]
    public float repathCooldown = 0.25f;

    [Tooltip("Si el target no se movió más que esto, no se recalcula.")]
    public float targetMoveThreshold = 0.5f;

    [Tooltip("Si nosotros no avanzamos nada en este tiempo, forzar repath.")]
    public float stuckRepathSeconds = 1.5f;

    // === Movimiento ===
    public Transform target;             // se asigna desde fuera (EnemyManager/Squad)
    public float speed = 3.5f;
    public float turnSpeed = 6f;
    public float turnDst = 5f;
    public float stoppingDst = 1.25f;

    Path path;
    public System.Action OnDestinationReached;
    public float CurrentSpeed { get; private set; }
    public bool HasReachedDestination { get; private set; }

    // Estado interno anti-spam
    Transform _followTarget;
    Vector3 _lastTargetPos;
    float _lastRepathTime;

    // Para detectar atascos locales
    Vector3 _lastPos;
    float _stuckTimer;

    public void ConfigureMovement(float spd, float turnSpd, float stopDst, float turnDistance) {
        speed = spd; turnSpeed = turnSpd; stoppingDst = stopDst; turnDst = turnDistance;
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful) {
        if (!pathSuccessful || waypoints == null || waypoints.Length == 0) return;
        path = new Path(waypoints, transform.position, turnDst, stoppingDst);
        StopCoroutine(nameof(FollowPath));
        StartCoroutine(nameof(FollowPath));
    }

    IEnumerator UpdatePath() {
        // pequeño delay al cargar escena
        if (Time.timeSinceLevelLoad < .3f) yield return new WaitForSeconds(.3f);

        // primer cálculo
        RequestRepath();

        float sqrMoveThreshold = targetMoveThreshold * targetMoveThreshold;
        Vector3 targetPosOld = _followTarget ? _followTarget.position : Vector3.positiveInfinity;

        while (true) {
            yield return new WaitForSeconds(minPathUpdateTime);

            if (!_followTarget) continue;

            // Si el target se movió lo suficiente, pide nueva ruta (respetando cooldown)
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
        // Enfriamiento
        if (Time.time - _lastRepathTime < repathCooldown) return;
        // Si el target apenas varió, omite
        if ((_followTarget.position - _lastTargetPos).sqrMagnitude < targetMoveThreshold * targetMoveThreshold) return;

        RequestRepath();
    }

    IEnumerator FollowPath() {
        bool followingPath = true;
        int pathIndex = 0;
        float speedPercent = 1f;

        // orientación inicial (solo Y)
        Vector3 firstDir = path.lookPoints[0] - transform.position;
        firstDir.y = 0f;
        if (firstDir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(firstDir.normalized, Vector3.up);

        _lastPos = transform.position;
        _stuckTimer = 0f;

        while (true) {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);

            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) {
                if (pathIndex == path.finishLineIndex) { followingPath = false; break; }
                pathIndex++;
            }

            if (followingPath) {
                HasReachedDestination = false;

                // slowdown suave
                if (pathIndex >= path.slowDownIndex && stoppingDst > 0f) {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                    if (speedPercent < 0.1f ||
                        Vector3.Distance(transform.position, path.lookPoints[path.finishLineIndex]) <= stoppingDst) {
                        followingPath = false;
                        HasReachedDestination = true;
                    }
                }

                // rotación suave (solo Y)
                Vector3 direction = path.lookPoints[pathIndex] - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.01f) {
                    Quaternion targetRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
                    float yRot = Mathf.LerpAngle(transform.eulerAngles.y, targetRot.eulerAngles.y, Time.deltaTime * turnSpeed);
                    transform.rotation = Quaternion.Euler(0f, yRot, 0f);
                }

                // avance
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
                CurrentSpeed = speed * speedPercent;

                // anti-atasco → si no avanzamos, fuerza repath (respetando cooldown)
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
            }

            yield return null;
        }
    }

    // === API pública, con anti-spam ===
    public void StartFollowing(Transform newTarget) {
        if (!newTarget) return;

        // Si es el mismo target y estamos dentro del cooldown, ignora
        if (_followTarget == newTarget && Time.time - _lastRepathTime < repathCooldown) return;

        _followTarget = newTarget;
        HasReachedDestination = false;

        StopAllCoroutines();            // resetea cualquier seguimiento anterior
        StartCoroutine(UpdatePath());   // arranca el ciclo de repath controlado
    }

    public void StopFollowing() {
        StopAllCoroutines();
        CurrentSpeed = 0f;
        HasReachedDestination = true;
        _followTarget = null;
    }

    public void OnDrawGizmos() {
        if (path != null) path.DrawWithGizmos();
    }
}
