using UnityEngine;

public class Bullet : MonoBehaviour {
    [Header("Datos (flyweight)")]
    public BulletSettings settings;

    Rigidbody rb;
    SphereCollider col;

    Transform owner;             // quien dispara (para ignorar colisiones)
    BulletPool pool;
    float lifeTimer;

    // Opcional: capa de proyectil para usar matriz de colisiones
    [Header("Opcional")]
    public string projectileLayerName = "Projectile";

    void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();

        // Configuraci�n recomendada para f�sicas
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // SphereCollider NO trigger para usar OnCollisionEnter
        col.isTrigger = false;
        if (col.radius <= 0f) col.radius = 0.05f; // peque�ito
    }

    /// <summary>
    /// Spawnea la bala lista para viajar. Se llama desde ShooterBase.Fire(...)
    /// </summary>
    public void Spawn(BulletPool poolOwner, BulletSettings cfg, Vector3 position, Vector3 direction, Transform shooterOwner, float initialSpeedOverride = -1f) {
        pool = poolOwner;
        settings = cfg;
        owner = shooterOwner;

        // Reset de vida
        lifeTimer = (settings != null && settings.lifeTime > 0f) ? settings.lifeTime : 2f;

        // Reposicionar & reorientar
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        // Capa opcional
        if (!string.IsNullOrEmpty(projectileLayerName)) {
            int layer = LayerMask.NameToLayer(projectileLayerName);
            if (layer >= 0) gameObject.layer = layer;
        }

        // Ignorar colisiones con el owner
        if (owner != null) {
            var ownerColliders = owner.GetComponentsInChildren<Collider>();
            foreach (var oc in ownerColliders) {
                if (oc && col) Physics.IgnoreCollision(col, oc, true);
            }
        }

        // Velocidad inicial
        float v = (initialSpeedOverride > 0f) ? initialSpeedOverride : (settings != null ? settings.speed : 20f);
        rb.linearVelocity = direction * v;
        gameObject.SetActive(true);
    }

    void Update() {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f) {
            Recycle();
        }
    }

    // Si prefieres trigger, cambia a OnTriggerEnter y marca col.isTrigger = true.
    void OnCollisionEnter(Collision collision) {
        // Ignora colisi�n con el owner por seguridad adicional
        if (owner && collision.transform.IsChildOf(owner)) return;

        // Aqu� puedes filtrar contra qu� reaccionar; por ahora cualquier cosa v�lida recicla
        Recycle();
    }

    void Recycle() {
        // Deja de ignorar al owner (opcional)
        if (owner != null) {
            var ownerColliders = owner.GetComponentsInChildren<Collider>();
            foreach (var oc in ownerColliders) {
                if (oc && col) Physics.IgnoreCollision(col, oc, false);
            }
        }

        // Vuelve al pool
        rb.linearVelocity = Vector3.zero;
        if (pool) pool.ReturnBullet(gameObject);
        else gameObject.SetActive(false);
    }
}