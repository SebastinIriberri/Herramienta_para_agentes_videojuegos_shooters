using UnityEngine;
[CreateAssetMenu(fileName = "EnemyArchetype", menuName = "AI/Enemy Archetype")]
public class EnemyArchetype : ScriptableObject {
    [Header("Rol")]
    [Tooltip("Define el arquetipo lógicamente (Grunt = seguidor; Elite = líder).")]
    public EnemyRole role = EnemyRole.Grunt;

    [Header("Visión y rangos")]
    [Tooltip("Radio de detección (m) para entrar a persecución.")]
    public float detectionRange = 12f;

    [Tooltip("Radio de ataque (m) para pasar a disparar/combate.")]
    public float attackRange = 6f;

    [Tooltip("Ángulo de visión en grados (0-360).")]
    [Range(0, 360)] public float viewAngle = 120f;

    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento del enemigo (m/s).")]
    public float moveSpeed = 3.5f;

    [Tooltip("Velocidad de giro (interpolación de rotación).")]
    public float turnSpeed = 6f;

    [Tooltip("Distancia a la que deja de empujar hacia el destino (llegada suave).")]
    public float stoppingDistance = 1.25f;

    [Tooltip("Distancia de adelantamiento para suavizar giros del path.")]
    public float turnDst = 5f;

    [Header("Memoria visual")]
    [Tooltip("Segundos que recordará la última posición vista del jugador.")]
    public float targetMemorySeconds = 3f;

    [Header("Chase (persecución)")]
    [Tooltip("Segundos que tolera sin ver al objetivo antes de abandonar 'chase'.")]
    public float chaseMaxLostSightTime = 4f;

    [Tooltip("Margen extra sobre 'detectionRange' para salir de 'chase'.")]
    public float chaseExitDistanceExtra = 2f;

    [Tooltip("Intervalo (s) entre re-cálculos de ruta durante 'chase'.")]
    public float chaseRepathInterval = 0.25f;

    [Tooltip("Si es true, exige línea de visión además de FOV para considerar visible.")]
    public bool chaseRequireLineOfSight = false;

    [Header("Attack (combate)")]
    [Tooltip("Segundos sin ver al objetivo para abandonar 'attack'.")]
    public float maxLostSightTime = 3f;

    [Tooltip("Margen adicional de distancia para salir de 'attack'.")]
    public float exitAttackExtra = 0.5f;

    [Header("Follow (solo Grunt)")]
    [Tooltip("Intervalo (s) para reordenar el follow hacia su slot (anti-spam).")]
    public float followRepathInterval = 0.35f;

    [Tooltip("Umbral (m) de movimiento mínimo del anchor para volver a ordenar.")]
    public float followAnchorMoveThreshold = 0.25f;

    [Tooltip("Fuerza de separación local entre miembros (0 = desactivado).")]
    public float followSeparationStrength = 0.6f;

    [Tooltip("Radio (m) para calcular separación local.")]
    public float followSeparationRadius = 1.2f;

    [Header("Disparo")]
    [Tooltip("Rango máximo de disparo (m).")]
    public float fireRange = 25f;

    [Tooltip("Segundos de cooldown entre disparos (cadencia).")]
    public float cooldownSeconds = 0.35f;

    [Tooltip("Desfase (m) desde el firePoint hacia delante al spawnear la bala.")]
    public float spawnOffset = 0.15f;
}
