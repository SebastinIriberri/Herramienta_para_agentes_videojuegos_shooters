using UnityEngine;

public class EnemyShooter : MonoBehaviour {
    public Transform firePoint;               // Lugar de donde salen las balas
    public GameObject bulletPrefab;           // Prefab de la bala
    public float fireRate = 1f;               // Disparos por segundo
    public float fireRange = 10f;             // Rango máximo de disparo

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
        if (bulletPrefab == null || firePoint == null) return;

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Debug.Log("Enemy dispara!");
    }

    private bool CanSeePlayer() {
        Vector3 direction = (player.position - firePoint.position).normalized;
        if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, fireRange)) {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }
}