using UnityEngine;

public enum DamageType { Generic, Bullet, Explosion, Melee }

public struct DamageInfo {
    public float amount;
    public DamageType type;
    public Transform source;
    public Vector3 hitPoint;
    public Vector3 hitNormal;

    public DamageInfo(float amount, DamageType type, Transform source, Vector3 hitPoint, Vector3 hitNormal) {
        this.amount = amount;
        this.type = type;
        this.source = source;
        this.hitPoint = hitPoint;
        this.hitNormal = hitNormal;
    }
}

public class Bullet : MonoBehaviour {
    [Header("Datos (flyweight)")]
    public BulletSettings settings;

    Rigidbody rb;
    SphereCollider col;

    Transform owner;         // quién disparó (para ignorar sus colliders)
    BulletPool pool;
    float lifeTimer;

    [Header("Opcional")]
    [Tooltip("Capa de la bala para ajustar la matriz de colisiones si lo deseas.")]
    public string projectileLayerName = "Projectile";

    void Awake() {
        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // preciso en alta velocidad
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        col = GetComponent<SphereCollider>();
        if (!col) col = gameObject.AddComponent<SphereCollider>();
        if (col.radius <= 0f) col.radius = 0.05f;

        // IMPORTANTE: trigger para evitar rebotes físicos y usar OnTriggerEnter
        col.isTrigger = true;
    }

    /// <summary>Inicializa y lanza la bala. Llamado desde ShooterBase.Fire(...)</summary>
    public void Spawn(BulletPool poolOwner, BulletSettings cfg, Vector3 position, Vector3 direction, Transform shooterOwner, float initialSpeedOverride = -1f) {
        pool = poolOwner;
        settings = cfg;
        owner = shooterOwner;

        lifeTimer = (settings != null && settings.lifeTime > 0f) ? settings.lifeTime : 2f;

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (!string.IsNullOrEmpty(projectileLayerName)) {
            int layer = LayerMask.NameToLayer(projectileLayerName);
            if (layer >= 0) gameObject.layer = layer;
        }

        // Ignorar colisiones con el owner (funciona también para triggers)
        if (owner != null) {
            var ownerColliders = owner.GetComponentsInChildren<Collider>();
            foreach (var oc in ownerColliders) {
                if (oc && col) Physics.IgnoreCollision(col, oc, true);
            }
        }

        float v = (initialSpeedOverride > 0f) ? initialSpeedOverride : (settings ? settings.speed : 20f);
        rb.linearVelocity = direction.normalized * v;

        gameObject.SetActive(true);
    }

    void Update() {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f) {
            Recycle();
        }
    }

    // Requiere que esta bala (o el otro) tenga collider trigger (nosotros sí)
    void OnTriggerEnter(Collider other) {
        // Evita autocolisión si algo no se ignoró correctamente
        if (owner != null && other.transform.IsChildOf(owner)) return;

        // 1) Si hay Hitbox, aplica con multiplicadores/eventos propios del hitbox
        // Ajusta el nombre de la clase si tu script es "HitBox"
        Hitbox hb = other.GetComponent<Hitbox>();
        if (hb) {
            hb.ApplyHit(new DamageInfo(
                settings ? settings.damage : 10f,
                DamageType.Bullet,
                owner,
                transform.position,
                -transform.forward
            ));
            Recycle();
            return;
        }

        // 2) Si no hay Hitbox, intenta dañar por Health en el objeto o sus padres
        Health hp = other.GetComponentInParent<Health>();
        if (hp) {
            hp.ApplyDamage(new DamageInfo(
                settings ? settings.damage : 10f,
                DamageType.Bullet,
                owner,
                transform.position,
                -transform.forward
            ));
            Recycle();
            return;
        }

        // 3) Si tocamos cualquier cosa no-trigger (pared/obstáculo), reciclamos
        if (!other.isTrigger) {
            Recycle();
        }
    }

    void Recycle() {
        // Dejar de ignorar al owner (limpieza)
        if (owner != null) {
            var ownerColliders = owner.GetComponentsInChildren<Collider>();
            foreach (var oc in ownerColliders) {
                if (oc && col) Physics.IgnoreCollision(col, oc, false);
            }
        }

        rb.linearVelocity = Vector3.zero;

        if (pool) pool.ReturnBullet(gameObject);
        else gameObject.SetActive(false);
    }
}