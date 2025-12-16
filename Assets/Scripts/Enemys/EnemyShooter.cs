using UnityEngine;

public class EnemyShooter : ShooterBase {
    [Header("Line of Fire")]
    public LayerMask lineOfFireMask = ~0;
    public float targetHeightOffset = 1.5f;

    public bool LastShotBlockedByAlly { get; private set; }

    [Header("Ammo / Reload")]
    public bool useAmmo = true;
    public int clipSize = 10;
    public int currentAmmo = -1;
    public float reloadDuration = 2.0f;
    public bool autoReload = true;

    [SerializeField]
    private bool isReloading = false;

    private float reloadTimer = 0f;

    [Header("Refs opcionales")]
    public EnemyAnimator enemyAnimator;

    public bool IsReloading => isReloading;
    public bool IsMagazineEmpty => currentAmmo <= 0;
    EnemyManager manager;

    void Awake() {
        if (!enemyAnimator) enemyAnimator = GetComponent<EnemyAnimator>();

        if (currentAmmo <= 0)
            currentAmmo = clipSize;
       
    }

    protected override void Update() {
        base.Update();

        if (isReloading) {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f) {
                FinishReload();
            }
        }
    }

    public void TryShoot(Transform target) {
        LastShotBlockedByAlly = false;

        var m = manager != null ? manager : (manager = GetComponent<EnemyManager>());
        if (m != null) {
            if (m.IsInMelee) return;
            if (Time.time < m.ShootBlockedUntil) return;
        }

        if (isReloading) return;
        if (!target || !firePoint) return;
        if (!CanShoot()) return;

        if (useAmmo && currentAmmo <= 0) {
            if (autoReload) StartReload();
            return;
        }

        Vector3 toTarget = target.position - firePoint.position;
        float dist = toTarget.magnitude;
        if (dist > fireRange) return;

        if (!HasSafeLineOfFire(target, dist)) return;

        Fire(toTarget.normalized, transform);
        ResetShootTimer();

        if (useAmmo) {
            currentAmmo = Mathf.Max(0, currentAmmo - 1);
            if (currentAmmo <= 0 && autoReload) StartReload();
        }
    }

    bool HasSafeLineOfFire(Transform target, float maxDistance) {
        Vector3 origin = firePoint.position;
        Vector3 targetPos = target.position + Vector3.up * targetHeightOffset;
        Vector3 dir = (targetPos - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, lineOfFireMask, QueryTriggerInteraction.Ignore)) {
            Transform t = hit.collider.transform;
            while (t != null) {
                if (t.CompareTag("Player"))
                    return true;
                t = t.parent;
            }

            EnemyManager ally = hit.collider.GetComponentInParent<EnemyManager>();
            if (ally != null) {
                LastShotBlockedByAlly = true;
                return false;
            }

            return false;
        }

        return true;
    }

    public void StartReload() {
        if (isReloading) return;

        isReloading = true;
        reloadTimer = reloadDuration;

        if (enemyAnimator != null) {
            enemyAnimator.PlayReload();
        }
    }

    void FinishReload() {
        isReloading = false;
        currentAmmo = clipSize;
    }

    public void ForceInstantReload() {
        isReloading = false;
        reloadTimer = 0f;
        currentAmmo = clipSize;
    }
}
