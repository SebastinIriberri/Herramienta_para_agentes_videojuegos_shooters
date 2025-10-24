using UnityEngine;

public class ShooterBase : MonoBehaviour{
    [Header("Disparo (común)")]
    public Transform firePoint;
    public BulletPool bulletPool;
    public BulletSettings bulletSettings;
    public float fireRate = 8f;
    public float fireRange = 25f;
    public float spawnOffset = 0.15f; 

    [Header("SFX (opcional)")]
    public AudioSystem.SoundData shootSound;

    protected float shootTimer;

    protected virtual void Update() {
        if (shootTimer > 0f) shootTimer -= Time.deltaTime;
    }

    protected bool CanShoot() => shootTimer <= 0f;
    protected void ResetShootTimer() => shootTimer = (fireRate > 0f) ? 1f / fireRate : 0f;

    protected void Fire(Vector3 worldDirection, Transform ownerToIgnore = null, float initialSpeedOverride = -1f) {
        if (!firePoint || !bulletPool || !bulletSettings) return;

        
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
