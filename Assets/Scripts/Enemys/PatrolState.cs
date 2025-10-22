using UnityEngine;
using System.Collections;

public class PatrolState : IEnemyState {
    private bool isWaiting;
    private EnemyManager cached;

    public void Enter(EnemyManager m) {
        cached = m;
        isWaiting = false;

        if (m.unit != null && m.patrolPoints.Length > 0) {
            Transform p = m.patrolPoints[m.currentPatrolIndex];
            m.unit.StartFollowing(p);
        }
    }

    public void Update(EnemyManager m) {
        // Si hay target en FOV/trigger => CHASE
        if (m.currentTarget != null) {
            m.GoToChase();
            return;
        }

        if (m.unit == null || m.patrolPoints.Length == 0) return;

        if (m.unit.HasReachedDestination && !isWaiting) {
            m.StartCoroutine(WaitThenNextPoint());
        }
    }

    public void Exit(EnemyManager m) {
        isWaiting = false;
        m.unit?.StopFollowing();
    }

    private IEnumerator WaitThenNextPoint() {
        isWaiting = true;
        yield return new WaitForSeconds(cached.waitAtPointSeconds);
        cached.currentPatrolIndex = (cached.currentPatrolIndex + 1) % cached.patrolPoints.Length;
        cached.unit.StartFollowing(cached.patrolPoints[cached.currentPatrolIndex]);
        isWaiting = false;
    }
}