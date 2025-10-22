using UnityEngine;
using AudioSystem;

public class EnemyShooter : MonoBehaviour {
    [HideInInspector] public Transform firePoint;
    [HideInInspector] public BulletSettings bulletSettings;
    [HideInInspector] public float fireRate = 3f;
    [HideInInspector] public float fireRange = 12f;
    [HideInInspector] public BulletPool bulletPool;
    [HideInInspector] public SoundData shootSound;

    private float shootTimer;

    private void Update() {
        shootTimer -= Time.deltaTime;
    }

    public void TryShoot(Transform target) {
        if (!target) return;
        if (!firePoint || !bulletSettings || !bulletPool) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > fireRange || shootTimer > 0f) return;

        // Checar Raycast desde el arma al objetivo
        Vector3 dir = (target.position + Vector3.up * 1.3f - firePoint.position).normalized;
        if (Physics.Raycast(firePoint.position, dir, out RaycastHit hit, fireRange)) {
            if (!hit.collider.CompareTag("Player")) return; // algo bloquea
        }

        // Disparo con pool + flyweight
        GameObject bulletGO = bulletPool.GetBullet();
        bulletGO.transform.position = firePoint.position;
        bulletGO.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null) {
            bullet.settings = bulletSettings;
            bullet.SetDirection(dir);
        }

        if (shootSound != null) {
            SoundManager.Instance
                .CreateSound()
                .WithPosition(firePoint.position)
                .WithRandomPitch()
                .Play(shootSound);
        }

        shootTimer = 1f / fireRate;
    }
}