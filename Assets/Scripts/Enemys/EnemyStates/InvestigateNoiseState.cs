using UnityEngine;

public class InvestigateNoiseState : IEnemyState {
    float waitTimer;

    public void Enter(EnemyManager m) {
        waitTimer = m.investigateWaitSeconds;
        if (m.unit != null) {
            m.FollowPoint(m.lastHeardNoisePos);
        }
    }

    public void Update(EnemyManager m) {
        if (m.currentTarget != null) {
            float dist = Vector3.Distance(m.transform.position, m.currentTarget.position);
            if (dist <= m.attackRange) {
                m.GoToAttack();
            }
            else {
                m.GoToChase();
            }
            return;
        }

        if (m.unit == null) {
            m.GoToPatrol();
            return;
        }

        if (m.unit.HasReachedDestination) {
            if (waitTimer > 0f) {
                waitTimer -= Time.deltaTime;
            }
            else {
                m.GoToPatrol();
            }
        }
    }

    public void Exit(EnemyManager m) {
        m.unit?.StopFollowing();
    }
}