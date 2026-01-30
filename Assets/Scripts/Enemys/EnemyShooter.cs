using UnityEngine;

public class EnemyShooter : ShooterBase
{
    [Header("Aim Setup")]
    [Tooltip("Origen del aim/raycast. Recomendado: Head/Chest (más estable que el arma).")]
    public Transform aimOrigin;

    [Tooltip("Punta del arma para proyectiles. Si no lo asignas, usa firePoint.")]
    public Transform muzzlePoint;

    [Tooltip("Altura a la que intentamos pegarle al target (pecho/cabeza).")]
    public float targetHeightOffset = 1.5f;

    [Header("Line of Fire (ally check / obstacles)")]
    public LayerMask lineOfFireMask = ~0;
    public bool ignoreTriggersInLineOfFire = true;

    [Header("Ammo / Reload")]
    public bool useAmmo = true;
    public int clipSize = 10;
    public int currentAmmo = -1;
    public float reloadDuration = 2.0f;
    public bool autoReload = true;

    [SerializeField] private bool isReloading = false;
    private float reloadTimer = 0f;

    [Header("Refs opcionales")]
    public EnemyAnimator enemyAnimator;

    [Header("Debug")]
    public bool debugDraw = true;

    public bool LastShotBlockedByAlly { get; private set; }
    public bool IsReloading => isReloading;
    public bool IsMagazineEmpty => currentAmmo <= 0;

    EnemyManager manager;

    void Awake()
    {
        if (!enemyAnimator) enemyAnimator = GetComponent<EnemyAnimator>();
        if (currentAmmo <= 0) currentAmmo = clipSize;

        if (!aimOrigin) aimOrigin = transform;

        // muzzlePoint opcional, pero firePoint es obligatorio para ShooterBase.
        if (!muzzlePoint) muzzlePoint = firePoint;
        if (firePoint == null && muzzlePoint != null) firePoint = muzzlePoint;
    }

    protected override void Update()
    {
        base.Update();

        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f) FinishReload();
        }
    }

    public void TryShoot(Transform target)
    {
        LastShotBlockedByAlly = false;

        var m = manager != null ? manager : (manager = GetComponent<EnemyManager>());
        if (m != null)
        {
            if (m.IsInMelee) return;
            if (Time.time < m.ShootBlockedUntil) return;
        }

        if (isReloading) return;
        if (!target) return;
        if (!CanShoot()) return;

        if (useAmmo && currentAmmo <= 0)
        {
            if (autoReload) StartReload();
            return;
        }

        if (!aimOrigin) aimOrigin = transform;
        if (!muzzlePoint) muzzlePoint = firePoint;

        Vector3 targetPos = target.position + Vector3.up * targetHeightOffset;

        // Dir inicial (desde aimOrigin)
        Vector3 aimDir = (targetPos - aimOrigin.position);
        float dist = aimDir.magnitude;
        if (dist <= 0.0001f) return;
        if (dist > fireRange) return;

        aimDir /= dist;

        // Raycast para LoF + obtener hit real (obstáculo/aliado/player)
        QueryTriggerInteraction qti = ignoreTriggersInLineOfFire ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;

        bool hasHit = Physics.Raycast(aimOrigin.position, aimDir, out RaycastHit hit, fireRange, lineOfFireMask, qti);

        if (debugDraw)
        {
            Vector3 end = hasHit ? hit.point : (aimOrigin.position + aimDir * fireRange);
            Debug.DrawLine(aimOrigin.position, end, Color.yellow, 0.05f);
        }

        // Si pegó algo, validamos
        if (hasHit)
        {
            // Ally block?
            EnemyManager ally = hit.collider.GetComponentInParent<EnemyManager>();
            if (ally != null && ally != manager)
            {
                LastShotBlockedByAlly = true;
                return;
            }

            // Si no es player (ni hijo), lo consideramos obstáculo y no disparamos
            if (!IsPlayerHit(hit.collider.transform))
            {
                return;
            }

            // Si es Projectile: dispara hacia el punto de impacto (desde el muzzle)
            if (fireMode == FireMode.Projectile)
            {
                Vector3 shootDir = (hit.point - muzzlePoint.position).normalized;

                if (debugDraw)
                    Debug.DrawLine(muzzlePoint.position, muzzlePoint.position + shootDir * 3f, Color.red, 0.05f);

                // Asegura que ShooterBase spawnee desde firePoint (muzzle)
                firePoint = muzzlePoint;

                Fire(shootDir, transform); // ShooterBase -> Projectile
            }
            else
            {
                // Raycast mode: usa ShooterBase tal cual (ya aplica daño)
                firePoint = aimOrigin;      // IMPORTANTE: el raycast del base sale desde firePoint
                Fire(aimDir, transform);    // ShooterBase -> RaycastDamage
            }

            ConsumeAmmoAndCooldown();
            return;
        }

        // Si NO hay hit: línea libre
        // Projectile: dispara hacia targetPos
        if (fireMode == FireMode.Projectile)
        {
            Vector3 shootDir = (targetPos - muzzlePoint.position).normalized;

            if (debugDraw)
                Debug.DrawLine(muzzlePoint.position, muzzlePoint.position + shootDir * 3f, Color.red, 0.05f);

            firePoint = muzzlePoint;
            Fire(shootDir, transform);
            ConsumeAmmoAndCooldown();
        }
        else
        {
            // Raycast: si no hay collider, no hay daño (normal en hitscan)
            // Si quisieras “daño igual” tendrías que hacer un SphereCast, pero lo dejamos correcto por ahora.
        }
    }

    void ConsumeAmmoAndCooldown()
    {
        ResetShootTimer();

        if (useAmmo)
        {
            currentAmmo = Mathf.Max(0, currentAmmo - 1);
            if (currentAmmo <= 0 && autoReload) StartReload();
        }
    }

    bool IsPlayerHit(Transform hitT)
    {
        Transform t = hitT;
        while (t != null)
        {
            if (t.CompareTag("Player")) return true;
            t = t.parent;
        }
        return false;
    }

    public void StartReload()
    {
        if (isReloading) return;

        isReloading = true;
        reloadTimer = reloadDuration;

        if (enemyAnimator != null) enemyAnimator.PlayReload();
    }

    void FinishReload()
    {
        isReloading = false;
        currentAmmo = clipSize;
    }

    public void ForceInstantReload()
    {
        isReloading = false;
        reloadTimer = 0f;
        currentAmmo = clipSize;
    }
}
