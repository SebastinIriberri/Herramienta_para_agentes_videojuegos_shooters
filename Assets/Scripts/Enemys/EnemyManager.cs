using UnityEngine;
public enum EnemyRole { Grunt, Elite }
[RequireComponent(typeof(SphereCollider))]
public class EnemyManager : MonoBehaviour {
    [Header("Rol / Escuadra")]
    public EnemyRole role = EnemyRole.Grunt;
    [Tooltip("Grupo/escuadra al que pertenece (para slotting).")]
    public SquadGroup squadGroup;

    [Header("Visión y rangos")]
    public float detectionRange = 12f;
    public float attackRange = 6f;
    [Range(0, 360)] public float viewAngle = 120f;

    [Header("Movimiento (Unit)")]
    public float moveSpeed = 3.5f;
    public float turnSpeed = 6f;
    public float stoppingDistance = 1.25f;
    public float turnDst = 5f;

    [Header("Patrullaje (solo Elite o fallback)")]
    public Transform[] patrolPoints;
    public float waitAtPointSeconds = 1.5f;
    [HideInInspector] public int currentPatrolIndex = 0;

    [Header("Memoria visual")]
    [Tooltip("Segundos que el enemigo recuerda la última posición vista del jugador.")]
    public float targetMemorySeconds = 3f;

    [HideInInspector] public Vector3 lastSeenPos;
    [HideInInspector] public float lastSeenTime;

    [Header("Chase")]
    public float chaseMaxLostSightTime = 4f;
    public float chaseExitDistanceExtra = 2f;
    public float chaseRepathInterval = 0.25f;
    public bool chaseRequireLineOfSight = false;

    [Header("Attack")]
    public float maxLostSightTime = 3f;
    public float exitAttackExtra = 0.5f;

    [Header("FollowLeader (solo Grunt)")]
    [Tooltip("Cada cuánto reordena el follow hacia su slot (anti-spam).")]
    public float followRepathInterval = 0.35f;

    [Tooltip("Metros mínimos de cambio de anchor para reordenar.")]
    public float followAnchorMoveThreshold = 0.25f;

    [Tooltip("Separación opcional entre miembros (fuerza).")]
    public float followSeparationStrength = 0.6f;

    [Tooltip("Radio para calcular separación entre miembros.")]
    public float followSeparationRadius = 1.2f;


    [Header("Debug")]
    [SerializeField] private string currentStateName = "(none)";
    public Transform currentTarget;

    [Header("Dependencias (auto)")]
    public SphereCollider visionCollider;
    public CapsuleCollider bodyCollider;
    public Rigidbody rb;
    public Unit unit;
    public EnemyShooter shooter;
    public EnemyAnimator enemyAnimator;

    // Anchor runtime para FollowLeader
    [HideInInspector] public Transform runtimeAnchor;

    // FSM
    private IEnemyState currentState;
    private readonly PatrolState patrolState = new PatrolState();
    private readonly ChaseState chaseState = new ChaseState();
    private readonly AttackState attackState = new AttackState();
    private readonly FollowLeaderState followLeaderState = new FollowLeaderState();

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
    }

    void Awake() {
        unit?.ConfigureMovement(moveSpeed, turnSpeed, stoppingDistance, turnDst);
    }

    void Start() {
        // Estado inicial según rol
        if (role == EnemyRole.Grunt && squadGroup != null) {
            TransitionTo(followLeaderState);
        }
        else {
            TransitionTo(patrolState); // Elite o sin grupo
        }
    }

    void Update() {
        currentState?.Update(this);
    }

    // FSM
    public void TransitionTo(IEnemyState s) {
        if (currentState == s) return;
        currentState?.Exit(this);
        currentState = s;
        currentStateName = currentState?.GetType().Name ?? "(none)";
        currentState?.Enter(this);
    }

    // Visión
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

        // Dibuja anchor (debug)
        if (runtimeAnchor) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(runtimeAnchor.position, 0.1f);
        }
    }

    // Helpers de transición (con rol-aware en llamadas desde otros estados)
    public void GoToPatrol() {
        if (role == EnemyRole.Grunt && squadGroup != null) TransitionTo(followLeaderState);
        else TransitionTo(patrolState);
    }
    public void GoToChase() => TransitionTo(chaseState);
    public void GoToAttack() => TransitionTo(attackState);
    public string GetCurrentStateName() => currentStateName;

}