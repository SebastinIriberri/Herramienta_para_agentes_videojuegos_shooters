using UnityEngine;
using static ShooterBase;

[CreateAssetMenu(fileName = "EnemyArchetype", menuName = "IA/Arquetipo de enemigo")]
public class EnemyArchetype : ScriptableObject
{
    [Header("Rol")]
    [Tooltip("Define el tipo de enemigo. Grunt suele funcionar como seguidor; Elite como enemigo más completo o líder.")]
    public EnemyRole role = EnemyRole.Grunt;

    [Header("Visión y rangos")]
    [Tooltip("Distancia máxima a la que el enemigo puede detectar al jugador.")]
    public float detectionRange = 12f;

    [Tooltip("Distancia a la que el enemigo puede intentar atacar al jugador.")]
    public float attackRange = 6f;

    [Tooltip("Ángulo de visión del enemigo en grados.")]
    [Range(0, 360)] public float viewAngle = 120f;

    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento del enemigo.")]
    public float moveSpeed = 3.5f;

    [Tooltip("Velocidad con la que el enemigo gira hacia su dirección de movimiento u objetivo.")]
    public float turnSpeed = 6f;

    [Tooltip("Distancia mínima a la que el enemigo se detiene respecto a su destino.")]
    public float stoppingDistance = 1.25f;

    [Tooltip("Distancia usada para suavizar los giros durante el seguimiento de ruta.")]
    public float turnDst = 5f;

    [Header("Memoria del objetivo")]
    [Tooltip("Tiempo que el enemigo recuerda la última posición conocida del jugador.")]
    public float targetMemorySeconds = 3f;

    [Header("Persecución")]
    [Tooltip("Tiempo máximo que el enemigo sigue persiguiendo después de perder de vista al jugador.")]
    public float chaseMaxLostSightTime = 4f;

    [Tooltip("Margen extra de distancia para abandonar la persecución.")]
    public float chaseExitDistanceExtra = 2f;

    [Tooltip("Cada cuánto tiempo se recalcula la ruta durante la persecución.")]
    public float chaseRepathInterval = 0.25f;

    [Tooltip("Si está activo, el enemigo requiere línea de visión para mantener la persecución.")]
    public bool chaseRequireLineOfSight = false;

    [Header("Ataque a distancia")]
    [Tooltip("Tiempo que el enemigo puede seguir en ataque después de perder línea de visión.")]
    public float maxLostSightTime = 3f;

    [Tooltip("Margen extra para evitar cambios bruscos entre ataque y persecución.")]
    public float exitAttackExtra = 0.5f;

    [Header("Cobertura")]
    [Tooltip("Permite que el enemigo use cobertura.")]
    public bool canUseCover = true;

    [Tooltip("Porcentaje de vida a partir del cual el enemigo puede buscar cobertura.")]
    [Range(0f, 1f)] public float coverLowHealthThreshold = 0.35f;

    [Tooltip("Tiempo durante el cual se considera que el enemigo está bajo amenaza.")]
    public float coverUnderFireWindow = 2.5f;

    [Tooltip("Distancia máxima para buscar un punto de cobertura.")]
    public float coverMaxSearchRadius = 12f;

    [Tooltip("Probabilidad de que el enemigo decida cubrirse al recibir daño.")]
    [Range(0f, 1f)] public float coverChanceOnHit = 0.6f;

    [Tooltip("Tiempo mínimo antes de volver a intentar buscar cobertura.")]
    public float coverRetryCooldown = 2.5f;

    [Tooltip("Tiempo que el enemigo permanece en cobertura.")]
    public float coverDuration = 2f;

    [Header("Ataque cuerpo a cuerpo")]
    [Tooltip("Permite que el enemigo use ataques cuerpo a cuerpo.")]
    public bool canUseMelee = true;

    [Tooltip("Distancia a partir de la cual puede iniciar el ataque cuerpo a cuerpo.")]
    public float meleeTriggerDistance = 2f;

    [Tooltip("Alcance efectivo del golpe cuerpo a cuerpo.")]
    public float meleeRange = 1.4f;

    [Tooltip("Radio del área de impacto del golpe.")]
    public float meleeHitRadius = 0.65f;

    [Tooltip("Desplazamiento frontal del área de impacto.")]
    public float meleeForwardOffset = 0.9f;

    [Tooltip("Ángulo frontal válido para que el golpe impacte.")]
    [Range(0f, 180f)] public float meleeAngle = 110f;

    [Tooltip("Daño aplicado por el ataque cuerpo a cuerpo.")]
    public float meleeDamage = 18f;

    [Tooltip("Tiempo mínimo entre ataques cuerpo a cuerpo.")]
    public float meleeCooldown = 2f;

    [Tooltip("Tiempo de seguridad para evitar que el enemigo quede atrapado en MeleeState.")]
    public float meleeFailSafeSeconds = 1.5f;

    [Tooltip("Capas que pueden recibir el golpe cuerpo a cuerpo.")]
    public LayerMask meleeHitMask = ~0;

    [Tooltip("Tiempo durante el cual se bloquea el disparo después del melee.")]
    public float postMeleeShootBlockSeconds = 0.15f;

    [Header("Combate: colisiones y movimiento lateral")]
    [Tooltip("Permite que el enemigo se mueva lateralmente durante el combate.")]
    public bool canStrafe = true;

    [Tooltip("Capas consideradas obstáculos durante el combate.")]
    public LayerMask combatObstacleMask = ~0;

    [Tooltip("Margen de seguridad contra obstáculos.")]
    public float combatSkin = 0.05f;

    [Tooltip("Multiplicador de velocidad al moverse lateralmente.")]
    public float strafeSpeedFactor = 0.6f;

    [Tooltip("Cantidad de frames bloqueado antes de intentar cambiar de lado.")]
    public int strafeBlockedFramesToFlip = 6;

    [Header("Seguimiento de líder")]
    [Tooltip("Cada cuánto tiempo se recalcula la ruta al seguir al líder.")]
    public float followRepathInterval = 0.35f;

    [Tooltip("Distancia mínima que debe moverse el punto guía para actualizar el seguimiento.")]
    public float followAnchorMoveThreshold = 0.25f;

    [Tooltip("Fuerza de separación entre miembros de la escuadra.")]
    public float followSeparationStrength = 0.6f;

    [Tooltip("Radio usado para mantener separación entre miembros de la escuadra.")]
    public float followSeparationRadius = 1.2f;

    [Header("Vagar por el escenario")]
    [Tooltip("Permite que el enemigo vague cuando no tiene objetivo.")]
    public bool enableWander = true;

    [Tooltip("Radio del área donde el enemigo puede vagar.")]
    public float wanderRadius = 10f;

    [Tooltip("Tiempo mínimo de espera antes de elegir otro destino.")]
    public float wanderWaitMin = 0.5f;

    [Tooltip("Tiempo máximo de espera antes de elegir otro destino.")]
    public float wanderWaitMax = 1.5f;

    [Tooltip("Cada cuánto tiempo se recalcula la ruta mientras vaga.")]
    public float wanderRepathInterval = 0.75f;

    [Tooltip("Distancia para considerar que llegó al destino de vagar.")]
    public float wanderArriveTolerance = 0.35f;

    [Tooltip("Rango de tiempo para elegir un nuevo destino.")]
    public Vector2 wanderRetargetEvery = new Vector2(4f, 7f);

    [Header("Oído e investigación de sonidos")]
    [Tooltip("Permite que el enemigo reaccione a sonidos o disparos.")]
    public bool enableHearing = true;

    [Tooltip("Distancia máxima a la que el enemigo puede escuchar sonidos.")]
    public float hearingRange = 18f;

    [Tooltip("Tiempo mínimo antes de reaccionar a otro sonido.")]
    public float hearingCooldownSeconds = 3f;

    [Tooltip("Tiempo que permanece investigando la zona del sonido.")]
    public float investigateWaitSeconds = 2f;

    [Header("Optimización de IA")]
    [Tooltip("Nivel de detalle inicial de la IA.")]
    public EnemyAILOD currentLOD = EnemyAILOD.High;

    [Tooltip("Intervalo de actualización para IA en nivel alto.")]
    public float aiTickIntervalHigh = 0f;

    [Tooltip("Intervalo de actualización para IA en nivel medio.")]
    public float aiTickIntervalMedium = 0.25f;

    [Tooltip("Intervalo de actualización para IA en nivel bajo.")]
    public float aiTickIntervalLow = 1.0f;

    [Header("Disparo")]
    [Tooltip("Si está activo, también aplica configuración al componente EnemyShooter.")]
    public bool applyShooterSettings = true;

    [Tooltip("Tipo de disparo del enemigo.")]
    public FireMode fireMode = FireMode.Projectile;

    [Tooltip("Rango máximo de disparo.")]
    public float fireRange = 25f;

    [Tooltip("Tiempo mínimo entre disparos.")]
    public float cooldownSeconds = 0.35f;

    [Tooltip("Desplazamiento usado al crear el proyectil.")]
    public float spawnOffset = 0.15f;

    [Tooltip("Configuración de la bala usada en modo proyectil.")]
    public BulletSettings bulletSettings;

    [Header("Raycast")]
    [Tooltip("Capas válidas para el disparo por Raycast.")]
    public LayerMask raycastMask = ~0;

    [Tooltip("Daño aplicado por el disparo por Raycast.")]
    public float raycastDamage = 10f;

    [Tooltip("Si está activo, el Raycast ignora colliders tipo Trigger.")]
    public bool raycastIgnoreTriggers = true;

    [Header("Apuntado y línea de tiro")]
    [Tooltip("Altura a la que el enemigo intenta apuntar al objetivo.")]
    public float targetHeightOffset = 1.5f;

    [Tooltip("Capas consideradas al comprobar línea de tiro.")]
    public LayerMask lineOfFireMask = ~0;

    [Tooltip("Si está activo, la línea de tiro ignora colliders tipo Trigger.")]
    public bool ignoreTriggersInLineOfFire = true;

    [Header("Munición y recarga")]
    [Tooltip("Activa el sistema de munición y recarga.")]
    public bool useAmmo = true;

    [Tooltip("Cantidad máxima de disparos antes de recargar.")]
    public int clipSize = 10;

    [Tooltip("Si está activo, el enemigo inicia con el cargador lleno al aplicar el arquetipo.")]
    public bool startWithFullAmmo = true;

    [Tooltip("Tiempo que tarda el enemigo en recargar.")]
    public float reloadDuration = 2f;

    [Tooltip("Si está activo, el enemigo recarga automáticamente al quedarse sin munición.")]
    public bool autoReload = true;
}