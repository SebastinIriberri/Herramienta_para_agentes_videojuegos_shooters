using UnityEngine;
/// <summary>
/// Clase base genérica para sistemas de disparo (enemigos y jugador).
/// Contiene la lógica común: control del cooldown, generación de balas (pooling),
/// velocidad, rango, sonido, etc. Heredada por EnemyShooter y PlayerShooter.
/// </summary>
public class ShooterBase : MonoBehaviour{
    [Header("Configuración de disparo")]
    [Tooltip("Punto desde el cual se originan las balas (normalmente la boca del arma).")]
    public Transform firePoint;

    [Tooltip("Pool de balas para reutilizar instancias y mejorar el rendimiento.")]
    public BulletPool bulletPool;

    [Tooltip("Configuración del proyectil (velocidad, daño, duración, etc.).")]
    public BulletSettings bulletSettings;

    [Tooltip("Segundos de espera entre cada disparo. Ejemplo: 0.1 = automática, 0.75 = semiautomática, 1.5 = rifle.")]
    [Min(0f)]
    public float cooldownSeconds = 0.25f;

    [Tooltip("Distancia máxima efectiva del disparo.")]
    public float fireRange = 25f;

    [Tooltip("Distancia adicional para separar la bala del FirePoint y evitar colisiones al aparecer.")]
    public float spawnOffset = 0.15f;

    [Header("Munición (opcional)")]
    [Tooltip("Si es false, munición infinita. Si es true, usa cargador + recarga.")]
    public bool useAmmo = false;

    [Min(1)]
    [Tooltip("Balas por cargador.")]
    public int clipSize = 12;

    [Min(0f)]
    [Tooltip("Tiempo de recarga en segundos.")]
    public float reloadSeconds = 1.5f;

    [SerializeField, Tooltip("Munición actual del cargador (solo lectura en inspector).")]
    private int currentAmmo;

    [Header("SFX (opcional)")]
    [Tooltip("Sonido a reproducir al disparar (si se utiliza el sistema de audio).")]
    public AudioSystem.SoundData shootSound;

    [Header("Depuración")]
    [Tooltip("Temporizador interno del cooldown (solo lectura).")]
    [SerializeField] private float shootTimer = 0f;

    public int CurrentAmmo => currentAmmo;
    public bool IsMagazineEmpty => useAmmo && currentAmmo <= 0;

    protected virtual void OnEnable() {
        // Inicializamos el cargador cuando se habilita el arma
        if (useAmmo) {
            if (clipSize < 1) clipSize = 1;
            if (currentAmmo <= 0 || currentAmmo > clipSize) {
                currentAmmo = clipSize;
            }
        }
    }

    protected virtual void Update() {
        if (shootTimer > 0f) {
            shootTimer -= Time.deltaTime;

            // Evitar valores negativos o demasiado largos en el inspector
            if (shootTimer < 0f)
                shootTimer = 0f;
        }
    }

    /// <summary>Devuelve true si el arma puede disparar (cooldown completado y hay balas si useAmmo).</summary>
    protected bool CanShoot() {
        if (shootTimer > 0f) return false;
        if (useAmmo && currentAmmo <= 0) return false;
        return true;
    }

    /// <summary>Reinicia el cooldown tras disparar.</summary>
    protected void ResetShootTimer() => shootTimer = cooldownSeconds;

    /// <summary>
    /// Crea y dispara una bala en la dirección especificada.
    /// </summary>
    protected void Fire(Vector3 worldDirection, Transform ownerToIgnore = null, float initialSpeedOverride = -1f) {
        if (!firePoint || !bulletPool || !bulletSettings) return;

        // Posición de aparición adelantada
        Vector3 spawnPos = firePoint.position + worldDirection.normalized * spawnOffset;

        GameObject bulletGO = bulletPool.GetBullet();
        if (!bulletGO) return;

        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (!bullet) bullet = bulletGO.AddComponent<Bullet>();

        bullet.Spawn(
            bulletPool,
            bulletSettings,
            spawnPos,
            worldDirection.normalized,
            ownerToIgnore ? ownerToIgnore : transform,
            initialSpeedOverride
        );

        // Consumir munición si aplica
        if (useAmmo && clipSize > 0) {
            currentAmmo = Mathf.Max(0, currentAmmo - 1);
        }

        if (shootSound) {
            AudioSystem.SoundManager.Instance
                .CreateSound()
                .WithPosition(firePoint.position)
                .WithRandomPitch()
                .Play(shootSound);
        }
    }

    /// <summary>Rellena instantáneamente el cargador (la animación/tiempo la gestiona la IA).</summary>
    public void ReloadInstant() {
        if (!useAmmo) return;
        currentAmmo = clipSize;
    }
}
