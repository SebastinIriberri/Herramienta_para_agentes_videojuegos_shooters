using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour {
    // ======= Constantes internas =======
    const float kMinPathUpdateTime = 0.2f;       // cadencia para revisar si el target se movió
    const float kPathUpdateMoveThreshold = 0.5f; // si el target se movió más de esto, recalcular
    const float kFrontCheckHeight = 0.5f;        // altura del raycast frontal
    const float kFrontCheckDist = 0.8f;          // distancia del raycast frontal
    const float kOverlapRadius = 0.3f;           // radio para detectar solapes con Obstacles
    const string kObstacleTag = "Obstacle";      // Tag para obstáculos (como acordamos)

    // ======= Referencias externas =======
    Path _path;
    PathfindingGrid _grid; // referencia al grid para saber si estamos sobre walkable

    // ======= Parámetros configurados por EnemyManager =======
    Transform _target;           // asignado por StartFollowing
    float _speed;                // asignado por ConfigureMovement
    float _turnSpeed;            // asignado por ConfigureMovement
    float _turnDst;              // asignado por ConfigureMovement
    float _stoppingDst;          // asignado por ConfigureMovement

    // ======= API pública (útil para FSM y Animator) =======
    public System.Action OnDestinationReached;   // evento opcional, al llegar destino
    public float CurrentSpeed { get; private set; }
    public bool HasReachedDestination { get; private set; }

    // ======= Inicialización =======
    private void Awake() {
        // Busca un grid en la escena si no se asigna explícitamente
        _grid = FindAnyObjectByType<PathfindingGrid>();
    }

    /// <summary>
    /// Permite inyectar el grid manualmente (si manejas múltiples grids).
    /// </summary>
    public void SetGrid(PathfindingGrid grid) => _grid = grid;

    /// <summary>
    /// Recibe la configuración desde EnemyManager (para no exponer campos públicos aquí).
    /// </summary>
    public void ConfigureMovement(float speed, float turnSpeed, float turnDst, float stoppingDst) {
        _speed = speed;
        _turnSpeed = turnSpeed;
        _turnDst = turnDst;
        _stoppingDst = stoppingDst;
    }

    // ======= Ciclo de vida del path =======
    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful) {
        if (!pathSuccessful || waypoints == null || waypoints.Length == 0) {
            // Si falla, intenta un reintento suave
            StartCoroutine(RepathSoon());
            return;
        }

        _path = new Path(waypoints, transform.position, _turnDst, _stoppingDst);
        StopCoroutine(nameof(FollowPath));
        StartCoroutine(nameof(FollowPath));
    }

    IEnumerator RepathSoon() {
        yield return new WaitForSeconds(0.3f);
        SafeRequestPath();
    }

    IEnumerator UpdatePath() {
        // Pequeña espera inicial por si la escena acaba de cargar
        if (Time.timeSinceLevelLoad < 0.3f)
            yield return new WaitForSeconds(0.3f);

        SafeRequestPath();

        float sqrMoveThreshold = kPathUpdateMoveThreshold * kPathUpdateMoveThreshold;
        Vector3 lastTargetPos = _target ? _target.position : transform.position;

        while (true) {
            yield return new WaitForSeconds(kMinPathUpdateTime);

            if (_target == null) continue;

            Vector3 delta = _target.position - lastTargetPos;
            if (delta.sqrMagnitude > sqrMoveThreshold) {
                SafeRequestPath();
                lastTargetPos = _target.position;
            }
        }
    }

    void SafeRequestPath() {
        if (_target == null) return;
        PathRequestManager.RequestPath(new PathRequest(transform.position, _target.position, OnPathFound));
    }

    // ======= Movimiento sobre el path =======
    IEnumerator FollowPath() {
        if (_path == null || _path.lookPoints == null || _path.lookPoints.Length == 0)
            yield break;

        bool followingPath = true;
        int pathIndex = 0;
        float speedPercent = 1f;

        // Rotación inicial (solo Y)
        Vector3 firstDir = _path.lookPoints[0] - transform.position;
        firstDir.y = 0f;
        if (firstDir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(firstDir.normalized, Vector3.up);

        // Variables anti-atasco
        Vector3 lastPos = transform.position;
        float stuckTimer = 0f;

        while (true) {
            // 0) Autocorrección si caí en no-walkable
            if (_grid != null) {
                Node currentNode = _grid.NodeFromWorldPoint(transform.position);
                if (!currentNode.walkable) {
                    // Empuja un poco hacia atrás y re-calcula
                    transform.position += -transform.forward * 0.5f;
                    SafeRequestPath();
                    yield return new WaitForSeconds(0.4f);
                    continue;
                }
            }

            // 1) Avance entre límites de giro
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (_path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) {
                if (pathIndex == _path.finishLineIndex) {
                    followingPath = false;
                    break;
                }
                else {
                    pathIndex++;
                }
            }

            if (followingPath) {
                HasReachedDestination = false;

                // 2) Frenado suave al final del camino
                if (pathIndex >= _path.slowDownIndex && _stoppingDst > 0f) {
                    speedPercent = Mathf.Clamp01(
                        _path.turnBoundaries[_path.finishLineIndex].DistanceFromPoint(pos2D) / _stoppingDst
                    );

                    if (speedPercent < 0.1f ||
                        Vector3.Distance(transform.position, _path.lookPoints[_path.finishLineIndex]) <= _stoppingDst) {
                        followingPath = false;
                        HasReachedDestination = true;
                    }
                }

                // 3) Rotación suave (solo Y)
                Vector3 direction = _path.lookPoints[pathIndex] - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.01f) {
                    Quaternion targetRot = Quaternion.LookRotation(direction.normalized, Vector3.up);
                    float y = Mathf.LerpAngle(transform.eulerAngles.y, targetRot.eulerAngles.y, Time.deltaTime * _turnSpeed);
                    transform.rotation = Quaternion.Euler(0f, y, 0f);
                }

                // 4) Detección simple de obstáculo frontal (raycast)
                if (Physics.Raycast(transform.position + Vector3.up * kFrontCheckHeight,
                                    transform.forward,
                                    out RaycastHit hit,
                                    kFrontCheckDist)) {
                    if (!hit.collider.isTrigger && hit.collider.CompareTag(kObstacleTag)) {
                        // Recalcular ruta si hay obstáculo delante
                        SafeRequestPath();
                        yield return new WaitForSeconds(0.3f);
                        continue;
                    }
                }

                // 5) Movimiento hacia adelante
                transform.Translate(Vector3.forward * (_speed * speedPercent * Time.deltaTime), Space.Self);
                CurrentSpeed = _speed * speedPercent;

                // 6) Anti-atasco progresivo:
                float moved = Vector3.Distance(transform.position, lastPos);
                if (moved < 0.03f) {
                    stuckTimer += Time.deltaTime;

                    // 6.1) Recalcular pronto si empieza a atascarse
                    if (stuckTimer > 1.0f && stuckTimer < 3.0f) {
                        SafeRequestPath();
                    }

                    // 6.2) Si sigue atascado mucho tiempo, forzar salida a patrulla (FSM)
                    if (stuckTimer > 3.0f) {
                        Debug.Log($"{name}: atascado, forzando regreso a patrulla.");
                        CurrentSpeed = 0f;
                        HasReachedDestination = true;

                        var manager = GetComponent<EnemyManager>();
                        if (manager != null) manager.GoToPatrol();

                        yield break; // salir del FollowPath
                    }
                }
                else {
                    stuckTimer = 0f;
                }

                // 7) Empuje suave si está solapado con obstáculo (por seguridad)
                int obstacleLayer = LayerMask.GetMask("Obstacle");
                if (obstacleLayer != 0 && Physics.CheckSphere(transform.position, kOverlapRadius, obstacleLayer)) {
                    transform.position += -transform.forward * Time.deltaTime * 0.5f;
                }

                lastPos = transform.position;
            }
            else {
                // Fin del path
                HasReachedDestination = true;
                CurrentSpeed = 0f;

                // Dispatcher opcional
                OnDestinationReached?.Invoke();
            }

            yield return null;
        }
    }

    // ======= API de control =======
    /// <summary>
    /// Comienza a seguir al target (solicita path y se mantiene actualizando si se mueve).
    /// </summary>
    public void StartFollowing(Transform newTarget) {
        _target = newTarget;
        HasReachedDestination = false;
        StopAllCoroutines();
        StartCoroutine(UpdatePath());
    }

    /// <summary>
    /// Detiene cualquier movimiento y marca destino como alcanzado.
    /// </summary>
    public void StopFollowing() {
        StopAllCoroutines();
        CurrentSpeed = 0f;
        HasReachedDestination = true;
    }
}
