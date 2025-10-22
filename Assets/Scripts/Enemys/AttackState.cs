using UnityEngine;

public class AttackState : IEnemyState {
    private float lostSightTimer;
   
    public void Enter(EnemyManager m) {
        lostSightTimer = 0f;
        m.unit?.StopFollowing(); 
    }
    public void Update(EnemyManager m) {
        if (m.currentTarget == null) {
            m.GoToPatrol();
            return;
        }

        float dist = Vector3.Distance(m.transform.position, m.currentTarget.position);
        if (dist > m.attackRange + m.exitAttackExtra) {
            m.GoToChase();
            return;
        }

       
        Vector3 dir = m.currentTarget.position - m.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f) {
            Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            Vector3 e = m.transform.eulerAngles;
            float y = Mathf.LerpAngle(e.y, lookRot.eulerAngles.y, Time.deltaTime * m.turnSpeed);
            m.transform.rotation = Quaternion.Euler(0f, y, 0f);
        }

        // Ver línea de visión
        bool canSee = m.IsInFOV(m.currentTarget) && m.HasLineOfSight(m.currentTarget, m.detectionRange);
        if (canSee) {
            m.shooter?.TryShoot(m.currentTarget);
            lostSightTimer = 0f;
        }
        else {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= m.maxLostSightTime) {
                m.GoToPatrol();
                return;
            }
        }
    }

    public void Exit(EnemyManager m) {
        lostSightTimer = 0f;
    }
}