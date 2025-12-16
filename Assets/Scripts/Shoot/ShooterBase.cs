using UnityEngine;

public class ShooterBase : MonoBehaviour {
    [Header("Disparo")]
    public Transform firePoint;
    public BulletPool bulletPool;
    public BulletSettings bulletSettings;

    [Min(0f)]
    public float cooldownSeconds = 0.25f;
    public float fireRange = 25f;
    public float spawnOffset = 0.15f;

    [Header("SFX")]
    public AudioSystem.SoundData shootSound;

    [SerializeField]
    private float shootTimer = 0f;

    protected virtual void Update() {
        if (shootTimer > 0f) {
            shootTimer -= Time.deltaTime;
            if (shootTimer < 0f)
                shootTimer = 0f;
        }
    }

    protected bool CanShoot() => shootTimer <= 0f;

    protected void ResetShootTimer() => shootTimer = cooldownSeconds;

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
