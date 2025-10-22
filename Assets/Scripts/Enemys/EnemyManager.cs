using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyManager : MonoBehaviour {
    [Header("Visión y rangos")]
    public float detectionRange = 12f;
    public float attackRange = 6f;
    [Range(0, 360)]
    public float viewAngle = 120f;


    [Header("Movimiento (Unit)")]
    public float moveSpeed = 3.5f;    
    public float turnSpeed = 6f;
    public float stoppingDistance = 1.25f;

    [Header("Patrullaje")]
    public Transform[] patrolPoints;
    public float waitAtPointSeconds = 1.5f;
    [HideInInspector] public int currentPatrolIndex = 0;

    [Header("Chase (persecución)")]
    [Tooltip("Tiempo que el enemigo tolera sin FOV/LoS antes de rendirse y volver a patrulla.")]
    public float chaseMaxLostSightTime = 4f;

    [Tooltip("Margen extra sobre detectionRange para abandonar Chase inmediatamente si el jugador se aleja demasiado.")]
    public float chaseExitDistanceExtra = 2f;

    [Tooltip("Cada cuánto reenvía la orden de seguir al target (throttle de path requests).")]
    public float chaseRepathInterval = 0.25f;

    [Tooltip("Si es true, además de FOV exige línea de visión para NO aumentar el temporizador de pérdida.")]
    public bool chaseRequireLineOfSight = false;

    [Header("Disparo")]
    public Transform firePoint;       
    public BulletPool bulletPool;     
    public BulletSettings bulletSettings;
    public float fireRate = 3f;
    public float fireRange = 12f;
    public AudioSystem.SoundData shootSound;

    [Header("Comportamiento de combate")]
    [Tooltip("Tiempo que el enemigo espera sin ver al jugador antes de volver a patrullar.")]
    public float maxLostSightTime = 3f;

    [Tooltip("Margen adicional de distancia antes de salir del modo ataque.")]
    public float exitAttackExtra = 0.5f;

    [Header("Debug")]
    [SerializeField] private string currentStateName = "(none)";
    public Transform currentTarget;  

    [Header("Dependencias (auto-asignadas)")]
    public SphereCollider visionCollider;
    public Unit unit;
    public EnemyShooter shooter;
    public EnemyAnimator enemyAnimator;

    // === FSM ===
    private IEnemyState currentState;
    // Estados reusables (opcional, evita new cada vez)
    private readonly PatrolState patrolState = new PatrolState();
    private readonly ChaseState chaseState = new ChaseState();
    private readonly AttackState attackState = new AttackState();

    private SphereCollider triggerCol;

    private void OnValidate() {
       
        if (!visionCollider) {
            visionCollider = GetComponent<SphereCollider>();
            if (!visionCollider) {
                visionCollider = gameObject.AddComponent<SphereCollider>();
                visionCollider.isTrigger = true;
                visionCollider.radius = detectionRange;
                Debug.Log($"{name}: SphereCollider agregado automáticamente ?");
            }
        }

       
        if (!unit) {
            unit = GetComponent<Unit>();
            if (!unit) {
                unit = gameObject.AddComponent<Unit>();
                Debug.Log($"{name}: Unit agregado automáticamente ?");
            }
        }

        if (!shooter) {
            shooter = GetComponent<EnemyShooter>();
            if (!shooter) {
                shooter = gameObject.AddComponent<EnemyShooter>();
                Debug.Log($"{name}: EnemyShooter agregado automáticamente ?");
            }
        }

        if (!enemyAnimator) {
            enemyAnimator = GetComponent<EnemyAnimator>();
            if (!enemyAnimator) {
                enemyAnimator = gameObject.AddComponent<EnemyAnimator>();
                Debug.Log($"{name}: EnemyAnimator agregado automáticamente ?");
            }
        }

        
        if (visionCollider) {
            visionCollider.isTrigger = true;
            visionCollider.radius = detectionRange;
        }
    }

    private void Awake() {
        
        if (!unit) {
            unit = GetComponent<Unit>();
            if (!unit) unit = gameObject.AddComponent<Unit>();
        }

      
        if (!shooter) {
            shooter = GetComponent<EnemyShooter>();
            if (!shooter) shooter = gameObject.AddComponent<EnemyShooter>();
        }

       
        triggerCol = GetComponent<SphereCollider>();
        if (!triggerCol) triggerCol = gameObject.AddComponent<SphereCollider>();
        triggerCol.isTrigger = true;
        triggerCol.radius = detectionRange;

       
        if (unit) {
            unit.speed = moveSpeed;
            unit.turnSpeed = turnSpeed;
            unit.stoppingDst = stoppingDistance;
        }

       
        if (shooter) {
            shooter.firePoint = firePoint;
            shooter.bulletPool = bulletPool;
            shooter.bulletSettings = bulletSettings;
            shooter.fireRate = fireRate;
            shooter.fireRange = fireRange;
            shooter.shootSound = shootSound;
        }
    }

    private void Start() {
        TransitionTo(patrolState);
    }

    private void Update() {
        currentState?.Update(this);
    }

  
    public void TransitionTo(IEnemyState newState) {
        if (currentState == newState) return;
        currentState?.Exit(this);
        currentState = newState;
        currentStateName = currentState?.GetType().Name ?? "(none)";
        currentState?.Enter(this);
    }

    
    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
       
        if (IsInFOV(other.transform)) {
            currentTarget = other.transform;
        }
    }

    private void OnTriggerStay(Collider other) {
        if (!other.CompareTag("Player")) return;

       
        if (IsInFOV(other.transform)) {
            currentTarget = other.transform;
        }
        else if (currentTarget == other.transform) {
            currentTarget = null;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && currentTarget == other.transform) {
            currentTarget = null;
        }
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
        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance)) {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    // === Gizmos ===
    private void OnDrawGizmosSelected() {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // FOV (visual)
        Gizmos.color = Color.red;
        Vector3 fwd = transform.forward;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * fwd;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * fwd;
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Gizmos.DrawLine(origin, origin + left * detectionRange);
        Gizmos.DrawLine(origin, origin + right * detectionRange);
    }

    // === Helpers de transición desde estados ===
    public void GoToPatrol() => TransitionTo(patrolState);
    public void GoToChase() => TransitionTo(chaseState);
    public void GoToAttack() => TransitionTo(attackState);

    // Exponer el nombre del estado actual (solo lectura)
    public string GetCurrentStateName() => currentStateName;
}