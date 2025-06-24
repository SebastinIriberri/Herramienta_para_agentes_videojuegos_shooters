using UnityEngine;

public class Bullet : MonoBehaviour {
    public BulletSettings settings;
    private Vector3 direction;     // Dirección en la que debe viajar


    private float lifeTimer; // Temporizador interno

    private void OnEnable() {
        lifeTimer = settings.lifeTime;
    }

    public void SetDirection(Vector3 dir) {
        direction = dir.normalized;
    }

    private void Update() {
        // Mover la bala
        transform.position += direction * settings.speed * Time.deltaTime;

        // Contador de vida
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f) {
            Deactivate();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Debug.Log("¡Jugador alcanzado!");
            // Aquí podrías aplicar daño u otros efectos
            Deactivate();
        }
    }

    private void Deactivate() {
        // Importante: siempre cancelar invokes y timers antes de reciclar
        BulletPool myPool = GetComponentInParent<BulletPool>();
        if (myPool != null) {
            myPool.ReturnBullet(gameObject);
        }
        else {
            // Seguridad en caso de que no esté en un pool (fallback)
            gameObject.SetActive(false);
        }
    }
}