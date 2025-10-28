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

    [Header("SFX (opcional)")]
    [Tooltip("Sonido a reproducir al disparar (si se utiliza el sistema de audio).")]
    public AudioSystem.SoundData shootSound;

    [Header("Depuración")]
    [Tooltip("Temporizador interno del cooldown (solo lectura).")]
    [SerializeField] private float shootTimer = 0f;

    protected virtual void Update() {
        if (shootTimer > 0f) {
            shootTimer -= Time.deltaTime;

            // Evitar valores negativos o demasiado largos en el inspector
            if (shootTimer < 0f)
                shootTimer = 0f;
        }
    }

    /// <summary>Devuelve true si el arma puede disparar (cooldown completado).</summary>
    protected bool CanShoot() => shootTimer <= 0f;

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

        if (shootSound) {
            AudioSystem.SoundManager.Instance
                .CreateSound()
                .WithPosition(firePoint.position)
                .WithRandomPitch()
                .Play(shootSound);
        }
    }
}
