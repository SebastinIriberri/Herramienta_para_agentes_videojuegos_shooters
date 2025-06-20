using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyVision : MonoBehaviour {
    [Tooltip("Etiqueta del objetivo que el enemigo debe detectar")]
    public string targetTag = "Player";

    private SphereCollider sphereCollider;
    private EnemyManager manager;
    public float radioVsion;

    private void Awake() {
        manager = GetComponentInParent<EnemyManager>();
        sphereCollider = GetComponent<SphereCollider>();

        // Configurar el collider como trigger
        sphereCollider.isTrigger = true;

        // Sincronizar el radio con el valor de EnemyManager
        if (manager != null) {
            radioVsion = manager.detectionRange;
            sphereCollider.radius = radioVsion;
        }
        else {
            Debug.LogWarning("EnemyManager no encontrado en el padre.");
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(targetTag)) {
            manager.player = other.transform;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag(targetTag) && manager.player == other.transform) {
            manager.player = null;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioVsion);

    }
}