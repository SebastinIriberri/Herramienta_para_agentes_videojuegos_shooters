using UnityEngine;

public class SquadMember : MonoBehaviour
{
    [Header("Escuadrón")]
    [Tooltip("Escuadrón al que pertenece este enemigo.")]
    public Squad squad;

    [Tooltip("Rol del enemigo en el escuadrón: Elite (líder) o Grunt (seguidor).")]
    public EnemyRole Role = EnemyRole.Grunt;

    [Header("Cohesión (solo Grunts)")]
    [Tooltip("Distancia máxima al líder antes de intentar acercarse.")]
    public float maxCohesionDistance = 8f;

    [Tooltip("Radio objetivo para formar alrededor del líder.")]
    public float desiredRadiusAroundLeader = 3f;

    [Tooltip("Si es true, cuando el Grunt está lejos del líder deja de patrullar y lo sigue.")]
    public bool lockPatrolWhenFar = true;

    [Header("Separación local")]
    [Tooltip("Capa(s) a considerar como compañeros para la separación (por rendimiento y control).")]
    public LayerMask separationLayers;

    [Tooltip("Radio de búsqueda para detectar vecinos y obstáculos próximos.")]
    public float separationScanRadius = 2.0f;

    [Tooltip("Distancia objetivo mínima a mantener respecto a cada vecino.")]
    public float separationDesired = 1.0f;

    [Tooltip("Fuerza/escala de la separación (0 = desactivado).")]
    public float separationStrength = 1.0f;

    [Tooltip("Ajuste vertical: 0 para plano, >0 si quieres levantar el empuje un poco.")]
    public float separationHeightOffset = 0.0f;

    [Tooltip("Suavizado del movimiento del anchor para evitar jitter.")]
    [Range(0f, 1f)] public float anchorSmooth = 0.2f;

    [Header("Promoción de líder")]
    [Tooltip("Si el líder muere: ¿promover al primer Grunt a líder?")]
    public bool promoteOnLeaderDeath = true;

    EnemyManager _manager;
    Unit _unit;
    Health _myHealth;

    EnemyManager _leaderManager;
    Health _leaderHealth;

    Transform _anchor;
    Vector3 _anchorVel; // para SmoothDamp

    public bool IsDead => _myHealth != null && _myHealth.IsDead;

    [Header("Throttle de ancla (evita spam)")]
    [Tooltip("Tiempo mínimo entre órdenes StartFollowing(anchor).")]
    public float anchorFollowCooldown = 0.35f;

    [Tooltip("Si el anchor se movió menos que esto, no reordenar.")]
    public float anchorMoveThreshold = 0.25f;

    float _anchorOrderTimer;
    Vector3 _lastAnchorPos;
    void Awake() {
        _manager = GetComponent<EnemyManager>();
        _unit = GetComponent<Unit>();
        _myHealth = GetComponent<Health>();

        if (!squad) {
            Debug.LogWarning($"{name}: SquadMember sin 'squad' asignado.");
        }
        else {
            squad.Register(this);
        }

        if (!_anchor) {
            var go = new GameObject($"{name}_SquadAnchor");
            go.transform.SetParent(transform.parent);
            _anchor = go.transform;
        }
    }

    void OnEnable() {
        HookLeaderEvents(true);
    }

    void OnDisable() {
        HookLeaderEvents(false);
        if (squad) squad.Unregister(this);
    }

    void Update() {
        if (Role == EnemyRole.Elite) return;
        if (!EnsureLeaderCache()) return;

        bool engaged = _manager.currentTarget != null;
        if (engaged) return;

        float distToLeader = Vector3.Distance(transform.position, _leaderManager.transform.position);

        if (distToLeader > maxCohesionDistance) {
            if (lockPatrolWhenFar && _unit != null) {
                // solo ordena si cambió target o pasó cooldown
                if (Time.time >= _anchorOrderTimer + anchorFollowCooldown) {
                    _unit.StartFollowing(_leaderManager.transform);
                    _anchorOrderTimer = Time.time;
                }
            }
            return;
        }

        if (_unit == null) return;

        // Calcula posición deseada (círculo + separación local si activaste)
        Vector3 targetPos = _leaderManager.transform.position + GetCircleOffsetAroundLeader();
        if (separationStrength > 0.001f && separationScanRadius > 0.05f) {
            Vector3 sep = ComputeSeparationOffset(targetPos);
            targetPos += sep * separationStrength;
        }
        if (separationHeightOffset != 0f) targetPos.y += separationHeightOffset;

        // Suaviza anchor
        _anchor.position = Vector3.SmoothDamp(_anchor.position, targetPos, ref _anchorVel, anchorSmooth);

        // Solo ordenar follow si el anchor cambió lo suficiente y pasó el cooldown
        if (Time.time >= _anchorOrderTimer + anchorFollowCooldown) {
            if ((_anchor.position - _lastAnchorPos).sqrMagnitude > (anchorMoveThreshold * anchorMoveThreshold)) {
                _unit.StartFollowing(_anchor);
                _lastAnchorPos = _anchor.position;
                _anchorOrderTimer = Time.time;
            }
        }
    }

    Vector3 GetCircleOffsetAroundLeader() {
        int seed = Mathf.Abs(name.GetHashCode());
        Random.InitState(seed);
        float angle = Random.Range(0f, 360f);
        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        return dir * Mathf.Max(0.1f, desiredRadiusAroundLeader);
    }

    /// <summary>
    /// Calcula un vector de separación a partir de vecinos cercanos (por capas) y obstáculos
    /// para empujar el anchor lejos de aglomeraciones.
    /// </summary>
    Vector3 ComputeSeparationOffset(Vector3 sampleAround) {
        Collider[] cols = Physics.OverlapSphere(sampleAround, separationScanRadius, separationLayers, QueryTriggerInteraction.Ignore);
        if (cols == null || cols.Length == 0) return Vector3.zero;

        Vector3 accum = Vector3.zero;
        int count = 0;

        for (int i = 0; i < cols.Length; i++) {
            var c = cols[i];
            if (!c || c.attachedRigidbody == null) continue;

            // Evita contarte a ti mismo
            if (c.transform == transform || c.transform.IsChildOf(transform)) continue;

            Vector3 otherPos = c.bounds.center;
            Vector3 toMe = sampleAround - otherPos;
            toMe.y = 0f;

            float d = toMe.magnitude;
            if (d < 0.0001f) continue;

            // Solo aplica si está dentro de la distancia deseada
            if (d < separationDesired) {
                float t = Mathf.InverseLerp(separationDesired, 0f, d); // más cerca ? mayor empuje
                accum += toMe.normalized * t;
                count++;
            }
        }

        if (count > 0) {
            accum /= count;
        }
        return accum;
    }

    bool EnsureLeaderCache() {
        if (!squad) return false;
        if (squad.Leader == null) return false;

        var sm = squad.Leader;
        var m = sm ? sm.GetComponent<EnemyManager>() : null;
        if (m != _leaderManager) {
            _leaderManager = m;
            _leaderHealth = sm ? sm.GetComponent<Health>() : null;
            HookLeaderEvents(false);
            HookLeaderEvents(true);
        }
        return _leaderManager != null;
    }

    void HookLeaderEvents(bool hook) {
        if (!squad || squad.Leader == null) return;
        var h = squad.Leader.GetComponent<Health>();
        if (!h) return;

        if (hook) h.onDied.AddListener(OnLeaderDied);
        else h.onDied.RemoveListener(OnLeaderDied);
    }

    void OnLeaderDied() {
        if (!squad) return;

        if (promoteOnLeaderDeath) {
            var newLeader = squad.PromoteFirstGruntAsLeader();
            if (newLeader != null) {
                Debug.Log($"[{squad.squadName}] Nuevo líder: {newLeader.name}");
            }
            else {
                Debug.Log($"[{squad.squadName}] Líder caído; escuadrón sin líder.");
            }
        }
        else {
            Debug.Log($"[{squad.squadName}] Líder caído. Sin promoción automática.");
        }

        _leaderManager = null;
        _leaderHealth = null;
    }

    public void PromoteToLeader() {
        Role = EnemyRole.Elite;
    }

    void OnDrawGizmosSelected() {
        // Gizmos de ayuda: radio de escaneo y separación deseada
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.25f);
        Gizmos.DrawWireSphere((_leaderManager ? _leaderManager.transform.position : transform.position), Mathf.Max(maxCohesionDistance, desiredRadiusAroundLeader));

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
        Vector3 center = (_anchor ? _anchor.position : transform.position);
        Gizmos.DrawWireSphere(center, separationScanRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, separationDesired);
    }
}
