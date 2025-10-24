using UnityEngine;

public class ChaseState : IEnemyState {
    private float lostSightTimer;
    private float repathTimer;

    public void Enter(EnemyManager m) {
        lostSightTimer = 0f;
        repathTimer = 0f;

        if (m.currentTarget != null) {
            m.unit?.StartFollowing(m.currentTarget);
        }
    }

    public void Update(EnemyManager m) {
        if (m.currentTarget == null) {
            m.GoToPatrol();
            return;
        }

        float dist = Vector3.Distance(m.transform.position, m.currentTarget.position);

        if (dist <= m.attackRange) {
            m.GoToAttack();
            return;
        }

        if (dist > m.detectionRange + m.chaseExitDistanceExtra) {
            m.GoToPatrol();
            return;
        }

        
        bool inFOV = m.IsInFOV(m.currentTarget);
        bool hasLOS = m.HasLineOfSight(m.currentTarget, m.detectionRange);
        bool considerVisible = inFOV && (!m.chaseRequireLineOfSight || hasLOS);

        if (considerVisible) {
            lostSightTimer = 0f;
        }
        else {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= m.chaseMaxLostSightTime) {
                m.GoToPatrol();
                return;
            }
        }

       
        Vector3 dir = m.currentTarget.position - m.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f) {
            Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            Vector3 e = m.transform.eulerAngles;
            float y = Mathf.LerpAngle(e.y, lookRot.eulerAngles.y, Time.deltaTime * m.turnSpeed);
            m.transform.rotation = Quaternion.Euler(0f, y, 0f);
        }

      
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f) {
            m.unit?.StartFollowing(m.currentTarget);
            repathTimer = Mathf.Max(0.05f, m.chaseRepathInterval);
        }
    }

    public void Exit(EnemyManager m) {
        lostSightTimer = 0f;
        
    }
}