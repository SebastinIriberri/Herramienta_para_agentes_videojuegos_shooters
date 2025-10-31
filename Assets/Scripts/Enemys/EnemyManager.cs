using UnityEngine;
/// <summary>
/// Orquesta el ciclo de vida del enemigo con una FSM (patrulla / persecución / ataque / seguir líder / vagar).
/// - Expone parámetros legibles en el Inspector (con tooltips).
/// - Inyecta arquetipo (opcional) para setear valores por tipo (Grunt/Elite).
/// - Selecciona estado inicial según rol y disponibilidad de patrulla/squad.
/// </summary>
public enum EnemyRole { Grunt, Elite }
[RequireComponent(typeof(SphereCollider))]
public class EnemyManager : MonoBehaviour {
    // ===========================
    //           Rol / Squad
    // ===========================
    [Header("Rol / Escuadra")]
    [Tooltip("Grunt = seguidor (si tiene squad), Elite = líder (suele patrullar o vagar).")]
    public EnemyRole role = EnemyRole.Grunt;

    [Tooltip("Grupo/escuadra al que pertenece (para slotting y seguir líder).")]
    public SquadGroup squadGroup;

    // ===========================
    //       Arquetipo (SO)
    // ===========================
    [Header("Arquetipo (SO Opcional)")]
    [Tooltip("ScriptableObject con parámetros por tipo de enemigo.")]
    public EnemyArchetype archetype;

    [Tooltip("Aplica el arquetipo automáticamente en Awake.")]
    public bool applyOnAwake = true;

    [Tooltip("Aplica el arquetipo en OnValidate para ver cambios en editor.")]
    public bool applyInEditor = true;

    // ===========================
    //       Visión y Rangos
    // ===========================
    [Header("Visión y Rangos")]
    [Tooltip("Radio (m) para detectar al jugador y pasar a persecución.")]
    public float detectionRange = 12f;

    [Tooltip("Radio (m) para considerar ataque/disparo.")]
    public float attackRange = 6f;

    [Tooltip("Ángulo de visión (grados).")]
    [Range(0, 360)] public float viewAngle = 120f;

    // ===========================
    //          Movimiento
    // ===========================
    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento (m/s).")]
    public float moveSpeed = 3.5f;

    [Tooltip("Velocidad de giro (interpolación).")]
    public float turnSpeed = 6f;

    [Tooltip("Distancia de llegada suave al destino.")]
    public float stoppingDistance = 1.25f;

    [Tooltip("Adelantamiento para suavizar giros del path (lo usa Unit).")]
    public float turnDst = 5f;

    // ===========================
    //        Memoria Visual
    // ===========================
    [Header("Memoria visual")]
    [Tooltip("Segundos recordando la última posición vista del jugador.")]
    public float targetMemorySeconds = 3f;

    [HideInInspector] public Vector3 lastSeenPos;
    [HideInInspector] public float lastSeenTime;

    // ===========================
    //            Chase
    // ===========================
    [Header("Persecución (Chase)")]
    [Tooltip("Segundos sin ver al jugador para abandonar Chase.")]
    public float chaseMaxLostSightTime = 4f;

    [Tooltip("Margen extra sobre detectionRange para abandonar Chase si se aleja demasiado.")]
    public float chaseExitDistanceExtra = 2f;

    [Tooltip("Intervalo de re-cálculo de ruta en Chase (anti-spam).")]
    public float chaseRepathInterval = 0.25f;

    [Tooltip("Si es true, además del FOV exige línea de visión para considerarlo visible.")]
    public bool chaseRequireLineOfSight = false;

    // ===========================
    //            Attack
    // ===========================
    [Header("Ataque (Attack)")]
    [Tooltip("Segundos sin ver al objetivo para salir de Attack.")]
    public float maxLostSightTime = 3f;

    [Tooltip("Margen adicional de distancia para abandonar Attack.")]
    public float exitAttackExtra = 0.5f;
    [Header("Combate: colisiones y strafe")]
    [Tooltip("Capas consideradas como obstáculos para el strafe/correcciones.")]
    public LayerMask combatObstacleMask = ~0;

    [Tooltip("Margen para cápsula al probar colisión (skin).")]
    public float combatSkin = 0.05f;

    [Tooltip("Cuánto avanza lateralmente el strafe (m/s) relativo a moveSpeed.")]
    public float strafeSpeedFactor = 0.6f;

    [Tooltip("Si se bloquea N frames seguidos, invierte dirección de strafe.")]
    public int strafeBlockedFramesToFlip = 6;

    // ===========================
    //           Follow (Grunt)
    // ===========================
    [Header("Follow (solo Grunt)")]
    [Tooltip("Intervalo para reordenar el follow hacia el slot (anti-spam).")]
    public float followRepathInterval = 0.35f;

    [Tooltip("Umbral (m) de movimiento del anchor para volver a ordenar.")]
    public float followAnchorMoveThreshold = 0.25f;

    [Tooltip("Fuerza de separación local entre compañeros (0 = off).")]
    public float followSeparationStrength = 0.6f;

    [Tooltip("Radio (m) para calcular separación local.")]
    public float followSeparationRadius = 1.2f;

    // ===========================
    //             Wander
    // ===========================
    [Header("Vagar (Wander)")]
    [Tooltip("Si no hay puntos de patrulla, puede deambular por la zona.")]
    public bool enableWander = true;

    [Tooltip("Centro de wander (si está vacío, usa la posición inicial).")]
    public Transform wanderCenter;

    [Tooltip("Radio en el que deambula alrededor del centro.")]
    public float wanderRadius = 10f;

    [Tooltip("Tiempo mínimo esperando al llegar a un punto.")]
    public float wanderWaitMin = 0.5f;

    [Tooltip("Tiempo máximo esperando al llegar a un punto.")]
    public float wanderWaitMax = 1.5f;

    [Tooltip("Intervalo mínimo entre órdenes de movimiento (anti-spam).")]
    public float wanderRepathInterval = 0.75f;
    // NUEVO: tolerancia de llegada y retarget programado
    [Tooltip("Distancia a la meta para considerarse 'llegado' (evita jitter).")]
    public float wanderArriveTolerance = 0.35f;

    [Tooltip("Cada cuántos segundos, forzar elegir otro punto aunque no haya llegado (min..max).")]
    public Vector2 wanderRetargetEvery = new Vector2(4f, 7f);

    // ===========================
    //             Debug
    // ===========================
    [Header("Debug")]
    [SerializeField] private string currentStateName = "(none)";
    [Tooltip("Blanco actual detectado (el Player).")]
    public Transform currentTarget;

    // ===========================
    //          Dependencias
    // ===========================
    [Header("Dependencias (auto)")]
    public SphereCollider visionCollider;
    public CapsuleCollider bodyCollider;
    public Rigidbody rb;
    public Unit unit;
    public EnemyShooter shooter;
    public EnemyAnimator enemyAnimator;

    // Anchor utilitario para seguir posiciones (memoria/slots/wander)
    [HideInInspector] public Transform runtimeAnchor;

    // ===========================
    //              FSM
    // ===========================
    private IEnemyState currentState;
    private readonly PatrolState patrolState = new PatrolState();
    private readonly ChaseState chaseState = new ChaseState();
    private readonly AttackState attackState = new AttackState();
    private readonly FollowLeaderState followLeaderState = new FollowLeaderState();
    private readonly WanderState wanderState = new WanderState();

    // Posición inicial (fallback para wander)
    private Vector3 _spawnPos;

    // ===========================
    //       Ciclo de vida
    // ===========================
    void OnValidate() {
        // Auto-asignaciones básicas
        if (!visionCollider) visionCollider = GetComponent<SphereCollider>();
        if (!visionCollider) visionCollider = gameObject.AddComponent<SphereCollider>();
        visionCollider.isTrigger = true;
        visionCollider.radius = detectionRange;

        if (!bodyCollider) bodyCollider = GetComponent<CapsuleCollider>();

        if (!rb) rb = GetComponent<Rigidbody>();
        if (!rb) {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = true; // movimiento por código
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ |
                             RigidbodyConstraints.FreezePositionY;
        }

        if (!unit) unit = GetComponent<Unit>();
        if (!shooter) shooter = GetComponent<EnemyShooter>();
        if (!enemyAnimator) enemyAnimator = GetComponent<EnemyAnimator>();

        if (applyInEditor && archetype) ApplyArchetype();
    }

    void Awake() {
        _spawnPos = transform.position;

        if (applyOnAwake && archetype) ApplyArchetype();
        unit?.ConfigureMovement(moveSpeed, turnSpeed, stoppingDistance, turnDst);
    }

    void Start() {
        // Estado inicial claro y predecible:
        // 1) Grunt con squad ? FollowLeader
        // 2) Si tiene patrulla ? Patrol
        // 3) Si se permite wander ? Wander
        // 4) Fallback ? Patrol (sin puntos solo se queda "quieto" hasta detectar)
        if (role == EnemyRole.Grunt && squadGroup != null) {
            TransitionTo(followLeaderState);
        }
        else if (HasPatrol()) {
            TransitionTo(patrolState);
        }
        else if (enableWander) {
            TransitionTo(wanderState);
        }
        else {
            TransitionTo(patrolState);
        }
    }

    void Update() {
        currentState?.Update(this);
    }

    // ===========================
    //          Transiciones
    // ===========================
    public void TransitionTo(IEnemyState s) {
        if (currentState == s) return;
        currentState?.Exit(this);
        currentState = s;
        currentStateName = currentState?.GetType().Name ?? "(none)";
        currentState?.Enter(this);
    }

    public void GoToPatrol() {
        if (role == EnemyRole.Grunt && squadGroup != null) TransitionTo(followLeaderState);
        else if (HasPatrol()) TransitionTo(patrolState);
        else if (enableWander) TransitionTo(wanderState);
        else TransitionTo(patrolState);
    }

    public void GoToChase() => TransitionTo(chaseState);
    public void GoToAttack() => TransitionTo(attackState);
    public void GoToWander() => TransitionTo(wanderState);

    // ===========================
    //            Visión
    // ===========================
    void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        if (IsInFOV(other.transform)) currentTarget = other.transform;
    }

    void OnTriggerStay(Collider other) {
        if (!other.CompareTag("Player")) return;
        if (IsInFOV(other.transform)) currentTarget = other.transform;
        else if (currentTarget == other.transform) currentTarget = null;
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && currentTarget == other.transform)
            currentTarget = null;
    }

    public bool IsInFOV(Transform target) {
        if (!target) return false;
        Vector3 dir = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        return angle < viewAngle * 0.5f;
    }

    public bool HasLineOfSight(Transform target, float maxDistance) {
        if (!target) return false;
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 dir = (target.position + Vector3.up * 1.5f - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance))
            return hit.collider.CompareTag("Player");

        return false;
    }

    // ===========================
    //           Helpers
    // ===========================
    public bool HasPatrol() => patrolPoints != null && patrolPoints.Length > 0;

    /// <summary>Devuelve un punto centro para Wander (transform asignado o spawn inicial).</summary>
    public Vector3 GetWanderCenter() {
        if (wanderCenter) return wanderCenter.position;
        return _spawnPos;
    }

    /// <summary>Permite a estados mover al enemigo a un punto Vector3 reutilizando un anchor.</summary>
    public Transform FollowPoint(Vector3 worldPos) {
        if (!runtimeAnchor) {
            var go = new GameObject($"{name}_Anchor");
            runtimeAnchor = go.transform;
        }
        runtimeAnchor.position = worldPos;
        runtimeAnchor.rotation = Quaternion.identity;
        unit?.StartFollowing(runtimeAnchor);
        return runtimeAnchor;
    }

    /// <summary>Aplica un arquetipo (SO) a los campos del manager y del shooter.</summary>
    public void ApplyArchetype(EnemyArchetype src = null) {
        var a = src ? src : archetype;
        if (!a) return;

        role = a.role;
        detectionRange = a.detectionRange;
        attackRange = a.attackRange;
        viewAngle = a.viewAngle;

        moveSpeed = a.moveSpeed;
        turnSpeed = a.turnSpeed;
        stoppingDistance = a.stoppingDistance;
        turnDst = a.turnDst;

        targetMemorySeconds = a.targetMemorySeconds;

        chaseMaxLostSightTime = a.chaseMaxLostSightTime;
        chaseExitDistanceExtra = a.chaseExitDistanceExtra;
        chaseRepathInterval = a.chaseRepathInterval;
        chaseRequireLineOfSight = a.chaseRequireLineOfSight;

        maxLostSightTime = a.maxLostSightTime;
        exitAttackExtra = a.exitAttackExtra;

        followRepathInterval = a.followRepathInterval;
        followAnchorMoveThreshold = a.followAnchorMoveThreshold;
        followSeparationStrength = a.followSeparationStrength;
        followSeparationRadius = a.followSeparationRadius;

        // Disparo (ShooterBase/EnemyShooter)
        if (shooter) {
            shooter.fireRange = a.fireRange;
            shooter.cooldownSeconds = a.cooldownSeconds; // asegúrate de tener este campo público
            shooter.spawnOffset = a.spawnOffset;
        }

        if (visionCollider) visionCollider.radius = detectionRange;
        if (unit) unit.ConfigureMovement(moveSpeed, turnSpeed, stoppingDistance, turnDst);
    }

    // ===========================
    //           Patrulla
    // ===========================
    [Header("Patrullaje (solo Elite o fallback)")]
    public Transform[] patrolPoints;
    public float waitAtPointSeconds = 1.5f;
    [HideInInspector] public int currentPatrolIndex = 0;

    // ===========================
    //            Gizmos
    // ===========================
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue; Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Vector3 fwd = transform.forward;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * fwd;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * fwd;
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Gizmos.DrawLine(origin, origin + left * detectionRange);
        Gizmos.DrawLine(origin, origin + right * detectionRange);

        if (runtimeAnchor) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(runtimeAnchor.position, 0.1f);
        }

        // Wander debug
        if (enableWander) {
            Gizmos.color = new Color(0f, 1f, 0.3f, 0.25f);
            Gizmos.DrawWireSphere(GetWanderCenter(), Mathf.Max(0.1f, wanderRadius));
        }
    }

}