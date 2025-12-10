using UnityEngine;
public class EnemyShooter : ShooterBase {
    [Header("Line of Fire")]
    public LayerMask lineOfFireMask = ~0;
    public float targetHeightOffset = 1.5f;

    public bool LastShotBlockedByAlly { get; private set; }

    public void TryShoot(Transform target) {
        LastShotBlockedByAlly = false;
        if (!target || !firePoint) return;
        if (!CanShoot()) return;

        Vector3 toTarget = target.position - firePoint.position;
        float dist = toTarget.magnitude;
        if (dist > fireRange) return;

        if (!HasSafeLineOfFire(target, dist)) return;

        Fire(toTarget.normalized, transform);
        ResetShootTimer();
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
}