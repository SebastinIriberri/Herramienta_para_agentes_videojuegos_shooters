using UnityEngine;

public enum EnemyRole { Grunt, Elite }
public enum EnemyAILOD { High, Medium, Low }

[RequireComponent(typeof(SphereCollider))]
public class EnemyManager : MonoBehaviour {
    [Header("Rol / Escuadra")]
    public EnemyRole role = EnemyRole.Grunt;
    public SquadGroup squadGroup;

    [Header("Arquetipo (SO Opcional)")]
    public EnemyArchetype archetype;
    public bool applyOnAwake = true;
    public bool applyInEditor = true;

    [Header("Visión y Rangos")]
    public float detectionRange = 12f;
    public float attackRange = 6f;
    [Range(0, 360)] public float viewAngle = 120f;

    [Header("Movimiento")]
    public float moveSpeed = 3.5f;
    public float turnSpeed = 6f;
    public float stoppingDistance = 1.25f;
    public float turnDst = 5f;

    [Header("Memoria visual")]
    public float targetMemorySeconds = 3f;
    [HideInInspector] public Vector3 lastSeenPos;
    [HideInInspector] public float lastSeenTime;

    [Header("Persecución (Chase)")]
    public float chaseMaxLostSightTime = 4f;
    public float chaseExitDistanceExtra = 2f;
    public float chaseRepathInterval = 0.25f;
    public bool chaseRequireLineOfSight = false;

    [Header("Ataque (Attack)")]
    public float maxLostSightTime = 3f;
    public float exitAttackExtra = 0.5f;

    [Header("Cobertura")]
    public bool canUseCover = true;
    [Range(0f, 1f)] public float coverLowHealthThreshold = 0.35f;
    public float coverUnderFireWindow = 2.5f;
    public float coverMaxSearchRadius = 12f;
    [Range(0f, 1f)] public float coverChanceOnHit = 0.6f;
    public float coverRetryCooldown = 2.5f;
    public float coverDuration = 2f;

    [HideInInspector] public CoverPoint currentCover;
    [HideInInspector] public float lastUnderFireTime;
    [HideInInspector] public float lastCoverDecisionTime;
    [HideInInspector] public Transform lastThreat;

    [Header("Combate: colisiones y strafe")]
    public bool canStrafe = true;
    public LayerMask combatObstacleMask = ~0;
    public float combatSkin = 0.05f;
    public float strafeSpeedFactor = 0.6f;
    public int strafeBlockedFramesToFlip = 6;

    [Header("Follow (solo Grunt)")]
    public float followRepathInterval = 0.35f;
    public float followAnchorMoveThreshold = 0.25f;
    public float followSeparationStrength = 0.6f;
    public float followSeparationRadius = 1.2f;

    [Header("Vagar (Wander)")]
    public bool enableWander = true;
    public Transform wanderCenter;
    public float wanderRadius = 10f;
    public float wanderWaitMin = 0.5f;
    public float wanderWaitMax = 1.5f;
    public float wanderRepathInterval = 0.75f;
    public float wanderArriveTolerance = 0.35f;
    public Vector2 wanderRetargetEvery = new Vector2(4f, 7f);

    [Header("Oído (Hearing)")]
    public bool enableHearing = true;
    public float hearingRange = 18f;
    public float hearingCooldownSeconds = 3f;
    public float investigateWaitSeconds = 2f;
    [HideInInspector] public Vector3 lastHeardNoisePos;
    [HideInInspector] public float lastHeardNoiseTime;

    [Header("LOD de IA")]
    public EnemyAILOD currentLOD = EnemyAILOD.High;
    public float aiTickIntervalHigh = 0f;
    public float aiTickIntervalMedium = 0.25f;
    public float aiTickIntervalLow = 1.0f;

    [Header("Debug")]
    [SerializeField] private string currentStateName = "(none)";
    public Transform currentTarget;

    public bool debugDrawDetectionRange = true;
    public bool debugDrawAttackRange = true;
    public bool debugDrawViewCone = true;
    public bool debugDrawRuntimeAnchor = true;
    public bool debugDrawWanderArea = true;
    public bool debugDrawPath = true;
    public bool debugDrawTurnLines = true;
    public bool debugDrawLookPoints = false;
    public bool debugDrawHearingRange = true;

    public Color debugColorDetection = Color.yellow;
    public Color debugColorAttack = Color.cyan;
    public Color debugColorViewCone = Color.red;
    public Color debugColorRuntimeAnchor = Color.magenta;
    public Color debugColorWander = new Color(0f, 1f, 0.3f, 0.25f);
    public Color debugColorPath = Color.cyan;
    public Color debugColorTurnLines = Color.magenta;
    public Color debugColorLookPoints = Color.green;
    public Color debugColorHearing = new Color(1f, 0.5f, 0f, 0.5f);

    [Header("Dependencias (auto)")]
    public SphereCollider visionCollider;
    public CapsuleCollider bodyCollider;
    public Rigidbody rb;
    public Unit unit;
    public EnemyShooter shooter;
    public EnemyAnimator enemyAnimator;

    [HideInInspector] public Transform runtimeAnchor;

    Health _health;

    IEnemyState currentState;
    readonly PatrolState patrolState = new PatrolState();
    readonly ChaseState chaseState = new ChaseState();
    readonly AttackState attackState = new AttackState();
    readonly FollowLeaderState followLeaderState = new FollowLeaderState();
    readonly WanderState wanderState = new WanderState();
    readonly InvestigateNoiseState investigateNoiseState = new InvestigateNoiseState();
    readonly CoverState coverState = new CoverState();
    readonly ReloadState reloadState = new ReloadState();

    Vector3 _spawnPos;
    float _aiTickTimer;

    void OnValidate() {
        if (!visionCollider) visionCollider = GetComponent<SphereCollider>();
        if (!visionCollider) visionCollider = gameObject.AddComponent<SphereCollider>();
        visionCollider.isTrigger = true;
        visionCollider.radius = detectionRange;

        if (!bodyCollider) bodyCollider = GetComponent<CapsuleCollider>();

        if (!rb) rb = GetComponent<Rigidbody>();
        if (!rb) {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = true;
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
        _health = GetComponent<Health>();
    }

    void OnEnable() {
        if (_health) _health.onDied.AddListener(OnDiedHandler);
        NoiseSystem.OnNoiseEmitted += HandleNoise;
        if (EnemyLODManager.Instance != null) EnemyLODManager.Instance.Register(this);
    }

    void OnDisable() {
        if (_health) _health.onDied.RemoveListener(OnDiedHandler);
        NoiseSystem.OnNoiseEmitted -= HandleNoise;
        if (EnemyLODManager.Instance != null) EnemyLODManager.Instance.Unregister(this);
    }

    void Start() {
        InitState();
    }

    void InitState() {
        currentTarget = null;
        lastSeenTime = -999f;
        lastHeardNoiseTime = -999f;

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
        float interval = GetCurrentAITickInterval();
        if (interval > 0f) {
            _aiTickTimer -= Time.deltaTime;
            if (_aiTickTimer > 0f) return;
            _aiTickTimer = interval;
        }
        currentState?.Update(this);
    }

    public void TransitionTo(IEnemyState s) {
        if (currentState == s) return;
        currentState?.Exit(this);
        currentState = s;
        currentStateName = currentState != null ? currentState.GetType().Name : "(none)";
        currentState?.Enter(this);
    }

    void OnDiedHandler() {
        ReleaseCover();

        currentState?.Exit(this);
        currentState = null;

        unit?.StopFollowing();

        if (shooter) shooter.enabled = false;
        if (enemyAnimator) enemyAnimator.enabled = false;
    }

    public void ResetForRespawn(Vector3 position, Quaternion rotation) {
        ReleaseCover();

        transform.SetPositionAndRotation(position, rotation);
        _spawnPos = position;

        if (_health != null) _health.ResetForRespawn();

        currentTarget = null;
        lastSeenTime = -999f;
        lastHeardNoiseTime = -999f;
        lastSeenPos = position;
        runtimeAnchor = null;

        _aiTickTimer = 0f;

        if (shooter) shooter.enabled = true;
        if (enemyAnimator) enemyAnimator.enabled = true;

        InitState();
    }

    public void GoToPatrol() {
        if (role == EnemyRole.Grunt && squadGroup != null) TransitionTo(followLeaderState);
        else if (HasPatrol()) TransitionTo(patrolState);
        else if (enableWander) TransitionTo(wanderState);
        else TransitionTo(patrolState);
    }

    public void GoToCover() {
        if (!canUseCover) return;
        if (currentCover == null) return;
        TransitionTo(coverState);
    }

    public void ReleaseCover() {
        if (currentCover != null) {
            currentCover.Release(this);
            currentCover = null;
        }
    }

    public void GoToChase() => TransitionTo(chaseState);
    public void GoToAttack() => TransitionTo(attackState);
    public void GoToWander() => TransitionTo(wanderState);
    public void GoToReload() => TransitionTo(reloadState);

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

    public bool HasPatrol() => patrolPoints != null && patrolPoints.Length > 0;

    public Vector3 GetWanderCenter() {
        if (wanderCenter) return wanderCenter.position;
        return _spawnPos;
    }

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

        if (shooter) {
            shooter.fireRange = a.fireRange;
            shooter.cooldownSeconds = a.cooldownSeconds;
            shooter.spawnOffset = a.spawnOffset;
        }

        if (visionCollider) visionCollider.radius = detectionRange;
        if (unit) unit.ConfigureMovement(moveSpeed, turnSpeed, stoppingDistance, turnDst);
    }

    public Transform[] patrolPoints;
    public float waitAtPointSeconds = 1.5f;
    [HideInInspector] public int currentPatrolIndex = 0;

    void HandleNoise(NoiseInfo info) {
        if (!enableHearing) return;
        if (!enabled || !gameObject.activeInHierarchy) return;
        if (_health != null && _health.IsDead) return;

        float maxRange = Mathf.Min(hearingRange, info.radius > 0f ? info.radius : hearingRange);
        float sqrRange = maxRange * maxRange;
        float sqrDist = (info.position - transform.position).sqrMagnitude;
        if (sqrDist > sqrRange) return;

        if (Time.time - lastHeardNoiseTime < hearingCooldownSeconds) return;

        if (currentTarget != null && (currentState == chaseState || currentState == attackState))
            return;

        lastHeardNoisePos = info.position;
        lastHeardNoiseTime = Time.time;

        TransitionTo(investigateNoiseState);
    }

    public void OnHit(Transform attacker, float currentHealthNormalized) {
        lastThreat = attacker;
        lastUnderFireTime = Time.time;

        // DEBUG opcional para ver que realmente entra aquí
        Debug.Log($"{name} OnHit: health01={currentHealthNormalized:F2}, attacker={attacker}");

        if (!canUseCover) return;
        if (attacker == null) return;
        if (currentLOD == EnemyAILOD.Low) return;

        // Evita spamear decisiones de cover
        if (Time.time - lastCoverDecisionTime < coverRetryCooldown) return;

        bool lowHealth = currentHealthNormalized <= coverLowHealthThreshold;
        bool underFire = (Time.time - lastUnderFireTime) <= coverUnderFireWindow;

        // DEBUG opcional
        Debug.Log($"{name} Cover check -> lowHealth={lowHealth}, underFire={underFire}");

        // Si no está con poca vida NI bajo fuego reciente, no pide cover
        if (!lowHealth && !underFire) return;

        // Probabilidad de decidir ir a cover
        if (Random.value > coverChanceOnHit) {
            lastCoverDecisionTime = Time.time;
            Debug.Log($"{name} decidió NO ir a cover (falló probabilidad).");
            return;
        }

        if (CoverManager.Instance == null) {
            Debug.LogWarning($"{name}: No hay CoverManager en la escena.");
            return;
        }

        CoverPoint best;
        if (!CoverManager.Instance.TryFindBestCover(
            transform.position,
            attacker.position,
            coverMaxSearchRadius,
            out best)) {
            lastCoverDecisionTime = Time.time;
            Debug.Log($"{name}: no encontró cover dentro de radio {coverMaxSearchRadius}.");
            return;
        }

        if (currentCover != null && currentCover != best) {
            currentCover.Release(this);
        }

        currentCover = best;
        currentCover.Reserve(this);

        lastCoverDecisionTime = Time.time;

        Debug.Log($"{name} -> yendo a cover: {currentCover.name}");
        GoToCover();
    }

    void OnDrawGizmosSelected() {
        if (debugDrawDetectionRange) {
            Gizmos.color = debugColorDetection;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }

        if (debugDrawAttackRange) {
            Gizmos.color = debugColorAttack;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        if (debugDrawViewCone) {
            Gizmos.color = debugColorViewCone;
            Vector3 fwd = transform.forward;
            Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * fwd;
            Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * fwd;
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Gizmos.DrawLine(origin, origin + left * detectionRange);
            Gizmos.DrawLine(origin, origin + right * detectionRange);
        }

        if (debugDrawRuntimeAnchor && runtimeAnchor) {
            Gizmos.color = debugColorRuntimeAnchor;
            Gizmos.DrawSphere(runtimeAnchor.position, 0.1f);
        }

        if (debugDrawWanderArea && enableWander) {
            Gizmos.color = debugColorWander;
            Gizmos.DrawWireSphere(GetWanderCenter(), Mathf.Max(0.1f, wanderRadius));
        }

        if (debugDrawHearingRange && enableHearing) {
            Gizmos.color = debugColorHearing;
            Gizmos.DrawWireSphere(transform.position, hearingRange);
        }
    }

    public void SetLOD(EnemyAILOD level) {
        if (currentLOD == level) return;
        currentLOD = level;
        _aiTickTimer = 0f;
    }

    public float GetCurrentAITickInterval() {
        switch (currentLOD) {
            case EnemyAILOD.Medium: return aiTickIntervalMedium;
            case EnemyAILOD.Low: return aiTickIntervalLow;
            default: return aiTickIntervalHigh;
        }
    }

    public float GetLODRepathMultiplier() {
        switch (currentLOD) {
            case EnemyAILOD.Medium: return 2f;
            case EnemyAILOD.Low: return 4f;
            default: return 1f;
        }
    }

    public bool ShouldUseFullLOSCheck() {
        return currentLOD != EnemyAILOD.Low;
    }

#if UNITY_EDITOR
    public void ForceValidateForDesigner() {
        if (!visionCollider) visionCollider = GetComponent<SphereCollider>();
        if (!visionCollider) visionCollider = gameObject.AddComponent<SphereCollider>();
        visionCollider.isTrigger = true;
        visionCollider.radius = detectionRange;

        if (!bodyCollider) bodyCollider = GetComponent<CapsuleCollider>();

        if (!rb) rb = GetComponent<Rigidbody>();
        if (!rb) {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ |
                             RigidbodyConstraints.FreezePositionY;
        }

        if (!unit) unit = GetComponent<Unit>();
        if (!shooter) shooter = GetComponent<EnemyShooter>();
        if (!enemyAnimator) enemyAnimator = GetComponent<EnemyAnimator>();

        if (archetype) ApplyArchetype();

        unit?.ConfigureMovement(moveSpeed, turnSpeed, stoppingDistance, turnDst);
    }
#endif
}
