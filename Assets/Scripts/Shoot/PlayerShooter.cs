using UnityEngine;

public class PlayerShooter : ShooterBase
{
    [Header("Opcional")]
    [Range(0f, 10f)] public float spreadDegrees = 0f;

    [Header("Ruido")]
    public float noiseRadius = 18f;

    [Header("Ammo")]
    public bool useAmmo = true;
    public int clipSize = 30;
    public int currentAmmo = 30;
    public float reloadDuration = 1.8f;
    public bool autoReload = true;

    [Header("Animator (Attacking Layer)")]
    [SerializeField] Animator animator;
    [SerializeField] string fireTriggerName = "Fire";
    [SerializeField] string reloadTriggerName = "Reload";
    [SerializeField] string reloadingBoolName = "IsReloading";

    public int CurrentAmmo => currentAmmo;
    public int ClipSize => clipSize;
    public bool UseAmmo => useAmmo;

    int fireHash;
    int reloadHash;
    int reloadingBoolHash;

    bool isFiring = false;
    bool isReloading = false;
    float reloadTimer = 0f;

    void Awake()
    {
        if (!animator) animator = GetComponentInParent<Animator>();

        fireHash = Animator.StringToHash(fireTriggerName);
        reloadHash = Animator.StringToHash(reloadTriggerName);
        reloadingBoolHash = Animator.StringToHash(reloadingBoolName);

        if (currentAmmo <= 0) currentAmmo = clipSize;
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

    public void SetFiring(bool firing) => isFiring = firing;

   
    public void RequestReload()
    {
        if (!useAmmo) return;
        if (isReloading) return;
        if (currentAmmo >= clipSize) return;

        StartReload();
    }

    public bool IsReloading => isReloading;

    public void TickShoot(Vector3 direction, Transform owner)
    {
        if (!isFiring) return;
        if (!firePoint) return;
        if (isReloading) return;         
        if (!CanShoot()) return;

        if (useAmmo)
        {
            if (currentAmmo <= 0)
            {
                if (autoReload) StartReload();
                return;
            }
        }

        Vector3 dir = direction;
        if (spreadDegrees > 0f)
            dir = ApplySpread(dir, spreadDegrees);

        Fire(dir, owner);
        ResetShootTimer();

        if (useAmmo)
        {
            currentAmmo = Mathf.Max(0, currentAmmo - 1);
            if (currentAmmo <= 0 && autoReload) StartReload();
        }

        if (animator) animator.SetTrigger(fireHash);

        NoiseSystem.EmitNoise(firePoint.position, noiseRadius, NoiseType.Gunshot, owner);
    }

    void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadDuration;

        if (animator)
        {
            animator.SetBool(reloadingBoolHash, true);
            animator.SetTrigger(reloadHash);
        }
    }

    void FinishReload()
    {
        isReloading = false;
        reloadTimer = 0f;
        currentAmmo = clipSize;

        if (animator)
        {
            animator.SetBool(reloadingBoolHash, false);
        }
    }

    Vector3 ApplySpread(Vector3 baseDir, float degrees)
    {
        Quaternion randomYaw = Quaternion.AngleAxis(Random.Range(-degrees, degrees), Vector3.up);
        Quaternion randomPitch = Quaternion.AngleAxis(Random.Range(-degrees, degrees), Vector3.right);
        return (randomYaw * randomPitch) * baseDir.normalized;
    }
}
