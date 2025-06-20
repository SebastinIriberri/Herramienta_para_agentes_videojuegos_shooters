using UnityEngine;

public class Bullet : MonoBehaviour {
    public float speed = 20f;
    public float lifeTime = 2f;

    private void Start() {
        Destroy(gameObject, lifeTime);
    }

    private void Update() {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Debug.Log("¡Jugador alcanzado!");
            Destroy(gameObject);
        }
    }
}