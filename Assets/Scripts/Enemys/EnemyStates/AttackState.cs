using UnityEngine;

public class AttackState : IEnemyState {
    float lostSightTimer;
    float strafeTimer;
    int strafeDir;
    int blockedFrames;

    public void Enter(EnemyManager m) {
        lostSightTimer = 0f;
        blockedFrames = 0;
        m.unit?.StopFollowing();

        strafeTimer = Random.Range(0.6f, 1.2f);
        strafeDir = Random.value < 0.5f ? -1 : 1;
    }

    public void Update(EnemyManager m) {
        var shooter = m.shooter;

        if (shooter != null && shooter.IsReloading) {
            m.unit?.StopFollowing();
            return;
        }

        if (shooter != null && shooter.useAmmo && shooter.IsMagazineEmpty) {
            shooter.StartReload();
            m.unit?.StopFollowing();
            return;
        }

        if (m.currentTarget == null) {
            m.GoToPatrol();
            return;
        }

        float dist = Vector3.Distance(m.transform.position, m.currentTarget.position);

        if (m.CanStartMelee(dist)) {
            m.GoToMelee();
            return;
        }

        if (dist > m.attackRange + m.exitAttackExtra) {
            m.GoToChase();
            return;
        }

        Vector3 to = m.currentTarget.position - m.transform.position;
        to.y = 0f;
        if (to.sqrMagnitude > 1e-4f) {
            Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
            float y = Mathf.LerpAngle(m.transform.eulerAngles.y, look.eulerAngles.y, Time.deltaTime * m.turnSpeed);
            m.transform.rotation = Quaternion.Euler(0f, y, 0f);
        }

        bool canSee = m.IsInFOV(m.currentTarget) && m.HasLineOfSight(m.currentTarget, m.detectionRange);
        if (canSee) {
            if (shooter != null) {
                shooter.TryShoot(m.currentTarget);
                if (shooter.LastShotBlockedByAlly && m.canStrafe) {
                    strafeDir *= -1;
                    strafeTimer = 0.15f;
                }
            }

            lostSightTimer = 0f;
            m.lastSeenPos = m.currentTarget.position;
            m.lastSeenTime = Time.time;
        }
        else {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= m.maxLostSightTime) {
                m.currentTarget = null;
                m.GoToChase();
                return;
            }
        }

        if (!m.canStrafe) return;

        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f) {
            strafeTimer = Random.Range(0.6f, 1.2f);
            strafeDir *= -1;
        }

        Vector3 side = Vector3.Cross(Vector3.up, to.sqrMagnitude > 1e-4f ? to.normalized : m.transform.forward) * strafeDir;

        float speed = m.moveSpeed * Mathf.Max(0.1f, m.strafeSpeedFactor);
        Vector3 desiredDelta = side * speed * Time.deltaTime;

        float minKeepDistance = Mathf.Max(0.5f, m.stoppingDistance);

        if (TryMoveCapsuleWithSlide(m, desiredDelta, minKeepDistance, out bool moved)) {
            if (moved) {
                blockedFrames = 0;
                return;
            }
        }

        blockedFrames++;
        if (blockedFrames >= Mathf.Max(2, m.strafeBlockedFramesToFlip)) {
            strafeDir *= -1;
            blockedFrames = 0;

            Vector3 dodgePoint = m.transform.position + side * 1.5f;
            if (!m.runtimeAnchor) {
                var go = new GameObject($"{m.name}_CombatAnchor");
                m.runtimeAnchor = go.transform;
            }
            m.runtimeAnchor.position = dodgePoint;
            m.unit?.StartFollowing(m.runtimeAnchor);
        }
    }

    public void Exit(EnemyManager m) {
        lostSightTimer = 0f;
        blockedFrames = 0;
    }

    bool TryMoveCapsuleWithSlide(EnemyManager m, Vector3 delta, float minKeepDistanceToTarget, out bool moved) {
        moved = false;
        if (delta.sqrMagnitude < 1e-8f) return true;

        if (!GetCapsule(m, out Vector3 p1, out Vector3 p2, out float radius)) return false;

        float skin = Mathf.Max(0f, m.combatSkin);
        float dist = delta.magnitude;
        Vector3 dir = delta / dist;

        if (Physics.CapsuleCast(p1, p2, radius - skin, dir, out RaycastHit hit, dist + skin, m.combatObstacleMask, QueryTriggerInteraction.Ignore)) {
            Vector3 slide = Vector3.ProjectOnPlane(delta, hit.normal);

            if (m.currentTarget) {
                Vector3 nextPos = m.transform.position + slide;
                float nextDistToTarget = Vector3.Distance(nextPos, m.currentTarget.position);
                if (nextDistToTarget < minKeepDistanceToTarget) {
                    slide = Vector3.zero;
                }
            }

            if (slide.sqrMagnitude > 1e-6f) {
                float sDist = slide.magnitude;
                Vector3 sDir = slide / sDist;

                if (!Physics.CapsuleCast(p1, p2, radius - skin, sDir, out RaycastHit hit2, sDist + skin, m.combatObstacleMask, QueryTriggerInteraction.Ignore)) {
                    m.transform.position += slide;
                    moved = true;
                    return true;
                }
            }

            return true;
        }
        else {
            m.transform.position += delta;
            moved = true;
            return true;
        }
    }

    bool GetCapsule(EnemyManager m, out Vector3 p1, out Vector3 p2, out float radius) {
        p1 = p2 = Vector3.zero;
        radius = 0f;
        var cc = m.bodyCollider;
        if (!cc) return false;

        Vector3 center = m.transform.TransformPoint(cc.center);
        float height = Mathf.Max(cc.height * Mathf.Abs(m.transform.lossyScale.y), cc.radius * 2f);
        radius = cc.radius * Mathf.Max(Mathf.Abs(m.transform.lossyScale.x), Mathf.Abs(m.transform.lossyScale.z));

        float half = Mathf.Max(0f, height * 0.5f - radius);
        Vector3 up = m.transform.up;

        p1 = center + up * half;
        p2 = center - up * half;
        return true;
    }
}
