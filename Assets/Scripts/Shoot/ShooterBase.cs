using UnityEngine;

public abstract class ShooterBase : MonoBehaviour
{
    public enum FireMode { Projectile, Raycast }

    [Header("Fire Mode")]
    public FireMode fireMode = FireMode.Projectile;

    [Header("Common")]
    public Transform firePoint;
    [Min(0f)] public float cooldownSeconds = 0.25f;
    public float fireRange = 25f;
    public float spawnOffset = 0.15f;

    [Header("Projectile (Pool)")]
    public BulletPool bulletPool;
    public BulletSettings bulletSettings;

    [Header("Raycast")]
    public LayerMask raycastMask = ~0;
    public int raycastDamage = 10;

    [Tooltip("Si tus Hitbox son TRIGGER, pon esto en FALSE para que el raycast los detecte.")]
    public bool raycastIgnoreTriggers = true;

    [Header("SFX")]
    public AudioSystem.SoundData shootSound;

    float shootTimer = 0f;

    protected virtual void Update()
    {
        if (shootTimer > 0f)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer < 0f) shootTimer = 0f;
        }
    }

    protected bool CanShoot() => shootTimer <= 0f;
    protected void ResetShootTimer() => shootTimer = cooldownSeconds;

    protected void Fire(Vector3 worldDirection, Transform ownerToIgnore = null, float initialSpeedOverride = -1f)
    {
        if (!firePoint) return;

        worldDirection = worldDirection.sqrMagnitude > 0.0001f ? worldDirection.normalized : firePoint.forward;

        if (fireMode == FireMode.Projectile)
            FireProjectile(worldDirection, ownerToIgnore, initialSpeedOverride);
        else
            FireRaycast(worldDirection, ownerToIgnore);

        PlayShootSfx();
    }

    void FireProjectile(Vector3 worldDirection, Transform ownerToIgnore, float initialSpeedOverride)
    {
        if (!bulletPool || !bulletSettings) return;

        Vector3 spawnPos = firePoint.position + worldDirection * spawnOffset;

        GameObject bulletGO = bulletPool.GetBullet();
        if (!bulletGO) return;

        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (!bullet) bullet = bulletGO.AddComponent<Bullet>();

        bullet.Spawn(
            bulletPool,
            bulletSettings,
            spawnPos,
            worldDirection,
            ownerToIgnore ? ownerToIgnore : transform,
            initialSpeedOverride
        );
    }

    void FireRaycast(Vector3 worldDirection, Transform ownerToIgnore)
    {
        Vector3 origin = firePoint.position;
        float maxDist = fireRange;

        QueryTriggerInteraction qti = raycastIgnoreTriggers
            ? QueryTriggerInteraction.Ignore
            : QueryTriggerInteraction.Collide;

        if (!Physics.Raycast(origin, worldDirection, out RaycastHit hit, maxDist, raycastMask, qti))
            return;

        // Ignorar self / owner
        if (ownerToIgnore != null && hit.transform.IsChildOf(ownerToIgnore))
            return;

        // Construir DamageInfo (igual que tus balas)
        var info = new DamageInfo(
            raycastDamage,
            DamageType.Bullet,             // o Generic si prefieres
            ownerToIgnore ? ownerToIgnore : transform,
            hit.point,
            hit.normal
        );

        // 1) Hitbox (headshot etc.)
        Hitbox hb = hit.collider.GetComponent<Hitbox>();
        if (hb != null)
        {
            hb.ApplyHit(info);
            return;
        }

        // 2) Health en el objeto o en padres
        Health hp = hit.collider.GetComponentInParent<Health>();
        if (hp != null)
        {
            hp.ApplyDamage(info);
            return;
        }

        // Debug opcional
        // Debug.Log($"[ShooterBase] Raycast hit {hit.collider.name} pero no tiene Hitbox ni Health.");
    }

    void PlayShootSfx()
    {
        if (!shootSound) return;
        if (AudioSystem.SoundManager.Instance == null) return;

        AudioSystem.SoundManager.Instance
            .CreateSound()
            .WithPosition(firePoint.position)
            .WithRandomPitch()
            .Play(shootSound);
    }
}
