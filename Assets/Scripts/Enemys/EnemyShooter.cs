using UnityEngine;

public class EnemyShooter : MonoBehaviour {
    [Header("Configuración de disparo")]
    public Transform firePoint;               // Punto de salida de la bala    
    public BulletSettings bulletSettings; // Flyweight asignado
    public float fireRate = 1f;               // Disparos por segundo
    public float fireRange = 10f;             // Rango máximo de disparo
    public BulletPool bulletPool; // Pool asignado a este tipo de bala

    private float shootTimer;
    private Transform player;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update() {
        shootTimer -= Time.deltaTime;
    }

    public void TryShoot() {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= fireRange && shootTimer <= 0f && CanSeePlayer()) {
            Shoot();
            shootTimer = 1f / fireRate;
        }
    }

    private void Shoot() {
        if (firePoint == null || bulletSettings == null || bulletPool == null) return;

        GameObject bulletGO = bulletPool.GetBullet();
        bulletGO.transform.position = firePoint.position;
        bulletGO.transform.rotation = firePoint.rotation;

        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null) {
            bullet.settings = bulletSettings;
            Vector3 shootDir = (player.position - firePoint.position).normalized;
            bullet.SetDirection(shootDir);
        }

        Debug.Log("Enemy dispara con su pool y settings");
    }

    private bool CanSeePlayer() {
        Vector3 direction = (player.position - firePoint.position).normalized;
        if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, fireRange)) {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    // Gizmo para depurar visualmente la dirección del disparo
    private void OnDrawGizmosSelected() {
        if (firePoint != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(firePoint.position, firePoint.forward * 2f);
        }
    }

}