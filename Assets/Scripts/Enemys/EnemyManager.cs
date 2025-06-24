using UnityEngine;

public class EnemyManager : MonoBehaviour {
    [Header("Rangos de comportamiento ")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float fieldOfView = 120f;

    [Header("Campo de visión")]
    [Range(0, 360)]
    public float viewAngle = 90f; // Ángulo de visión en grados

    [Header("Puntos de patrullaje")]
    public Transform[] patrolPoints;
    public float timeToChanguedPatrolPoint;
    [HideInInspector] public int currentPatrolIndex = 0;
    public float speed = 3f;
    public float health = 100f;

    [Header("Animator")]
    public EnemyAnimator enemyAnimator;

    [HideInInspector] public Transform currentTarget;

    private void Start() {
        enemyAnimator = GetComponent<EnemyAnimator>();

        // Configurar el SphereCollider como trigger
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = detectionRange;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (IsInFOV(other.transform)) {
                currentTarget = other.transform;
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("Player")) {
            if (IsInFOV(other.transform)) {
                currentTarget = other.transform;
            }
            else if (currentTarget == other.transform) {
                currentTarget = null; // Si ya no está en el FOV
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && currentTarget == other.transform) {
            currentTarget = null;
        }
    }

    // Verifica si el jugador está dentro del campo de visión
    private bool IsInFOV(Transform target) {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle < fieldOfView * 0.5f;
    }

    private void OnDrawGizmos() {
        // Rango de detección (esfera amarilla)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Campo de visión (ángulo)
        Gizmos.color = Color.red;

        Vector3 forward = transform.forward;
        Vector3 leftLimit = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
        Vector3 rightLimit = Quaternion.Euler(0, viewAngle / 2, 0) * forward;

        Vector3 origin = transform.position + Vector3.up * 1.5f; // elevar un poco para que se vea mejor

        Gizmos.DrawLine(origin, origin + leftLimit * detectionRange);
        Gizmos.DrawLine(origin, origin + rightLimit * detectionRange);

        // Opcional: dibujar líneas del sector para mejor visual
        int segments = 20;
        for (int i = 0; i <= segments; i++) {
            float angle = -viewAngle / 2 + (viewAngle / segments) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Gizmos.DrawLine(origin, origin + dir * detectionRange);
        }
    }
}
