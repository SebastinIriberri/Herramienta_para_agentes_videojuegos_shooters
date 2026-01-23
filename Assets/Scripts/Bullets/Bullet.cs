using UnityEngine;

public enum DamageType { Generic, Bullet, Explosion, Melee }

public struct DamageInfo
{
    public float amount;
    public DamageType type;
    public Transform source;
    public Vector3 hitPoint;
    public Vector3 hitNormal;

    public DamageInfo(float amount, DamageType type, Transform source, Vector3 hitPoint, Vector3 hitNormal)
    {
        this.amount = amount;
        this.type = type;
        this.source = source;
        this.hitPoint = hitPoint;
        this.hitNormal = hitNormal;
    }
}

public class Bullet : MonoBehaviour
{
    [Header("Datos (flyweight)")]
    public BulletSettings settings;

    Rigidbody rb;
    SphereCollider col;

    Transform owner;
    BulletPool pool;
    float lifeTimer;

    [Header("Opcional")]
    [Tooltip("Capa de la bala para ajustar la matriz de colisiones.")]
    public string projectileLayerName = "Projectile";

    [Header("Sweep / Anti-tunneling")]
    [Tooltip("Qué capas puede golpear la bala al hacer el 'sweep'. Pon aquí Default/Enemy/Unwalkable, etc.")]
    public LayerMask sweepMask = ~0;

    [Tooltip("Ignorar triggers en el sweep (recomendado).")]
    public bool sweepIgnoreTriggers = true;

    Vector3 lastPos;
    bool spawned;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        col = GetComponent<SphereCollider>();
        if (!col) col = gameObject.AddComponent<SphereCollider>();
        if (col.radius <= 0f) col.radius = 0.05f;

        col.isTrigger = true;
    }

    public void Spawn(BulletPool poolOwner, BulletSettings cfg, Vector3 position, Vector3 direction, Transform shooterOwner, float initialSpeedOverride = -1f)
    {
        pool = poolOwner;
        settings = cfg;
        owner = shooterOwner;

        lifeTimer = (settings != null && settings.lifeTime > 0f) ? settings.lifeTime : 2f;

        transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction, Vector3.up));

        if (!string.IsNullOrEmpty(projectileLayerName))
        {
            int layer = LayerMask.NameToLayer(projectileLayerName);
            if (layer >= 0) gameObject.layer = layer;
        }

        if (owner != null)
        {
            var ownerColliders = owner.GetComponentsInChildren<Collider>();
            foreach (var oc in ownerColliders)
            {
                if (oc && col) Physics.IgnoreCollision(col, oc, true);
            }
        }

        float v = (initialSpeedOverride > 0f) ? initialSpeedOverride : (settings ? settings.speed : 20f);
        rb.linearVelocity = direction.normalized * v;

        lastPos = transform.position;
        spawned = true;

        gameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        if (!spawned) return;

        Vector3 currentPos = transform.position;
        Vector3 delta = currentPos - lastPos;
        float dist = delta.magnitude;

        if (dist > 0.0001f)
        {
            Vector3 dir = delta / dist;
            QueryTriggerInteraction qti = sweepIgnoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;

          
            if (Physics.SphereCast(lastPos, col.radius, dir, out RaycastHit hit, dist, sweepMask, qti))
            {
              
                if (owner != null && hit.transform.IsChildOf(owner))
                {
                    lastPos = currentPos;
                    return;
                }
                HandleHit(hit.collider, hit.point, hit.normal);
                return;
            }
        }

        lastPos = currentPos;
    }

    void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f) Recycle();
    }

    void OnTriggerEnter(Collider other)
    {
        // Por si pega con hitboxes triggers (enemigos)
        if (owner != null && other.transform.IsChildOf(owner)) return;

        HandleHit(other, transform.position, -transform.forward);
    }

    void HandleHit(Collider other, Vector3 hitPoint, Vector3 hitNormal)
    {
        // DEBUG (opcional)
        Debug.Log($"[Bullet] Hit -> {other.name} | Layer: {LayerMask.LayerToName(other.gameObject.layer)} | IsTrigger: {other.isTrigger}");

        // 1) Hitbox
        Hitbox hb = other.GetComponent<Hitbox>();
        if (hb)
        {
            hb.ApplyHit(new DamageInfo(
                settings ? settings.damage : 10f,
                DamageType.Bullet,
                owner,
                hitPoint,
                hitNormal
            ));
            Recycle();
            return;
        }

        // 2) Health en el objeto o en padres
        Health hp = other.GetComponentInParent<Health>();
        if (hp)
        {
            hp.ApplyDamage(new DamageInfo(
                settings ? settings.damage : 10f,
                DamageType.Bullet,
                owner,
                hitPoint,
                hitNormal
            ));
            Recycle();
            return;
        }

        // 3) Si no tiene nada, igual reciclamos si es “sólido”
        if (!other.isTrigger)
        {
            Recycle();
        }
    }

    void Recycle()
    {
        // Dejar de ignorar al owner
        if (owner != null)
        {
            var ownerColliders = owner.GetComponentsInChildren<Collider>();
            foreach (var oc in ownerColliders)
            {
                if (oc && col) Physics.IgnoreCollision(col, oc, false);
            }
        }

        rb.linearVelocity = Vector3.zero;
        owner = null;
        spawned = false;

        if (pool) pool.ReturnBullet(gameObject);
        else gameObject.SetActive(false);
    }
}
