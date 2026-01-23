using UnityEngine;
using System.Collections;

public class PatrolState : IEnemyState {
    bool isWaiting;
    EnemyManager cached;

    public void Enter(EnemyManager m) {
        cached = m;
        isWaiting = false;

        if (m.unit != null && m.patrolPoints.Length > 0) {
            Transform p = m.patrolPoints[m.currentPatrolIndex];
            m.unit.StartFollowing(p);
        }
    }

    public void Update(EnemyManager m) {
        if (m.currentTarget != null) {
            if (m.squadGroup != null) m.squadGroup.ReportPlayerSeen(m.currentTarget.position);
            m.lastSeenPos = m.currentTarget.position;
            m.lastSeenTime = Time.time;
            m.GoToChase();
            return;
        }

        if (m.squadGroup != null && m.squadGroup.enableBlackboard) {
            Vector3 sharedPos;
            if (m.squadGroup.TryGetRecentPlayerSeen(out sharedPos)) {
                if (Time.time - m.lastSeenTime > m.targetMemorySeconds) {
                    m.lastSeenPos = sharedPos;
                    m.lastSeenTime = Time.time;
                    m.GoToChase();
                    return;
                }
            }
        }

        if (m.unit == null || m.patrolPoints.Length == 0) return;

        if (m.unit.HasReachedDestination && !isWaiting) {
            m.unit.StopFollowing();
            m.StartCoroutine(WaitThenNextPoint());
        }
    }

    public void Exit(EnemyManager m) {
        isWaiting = false;
        m.unit?.StopFollowing();
    }

    IEnumerator WaitThenNextPoint() {
        isWaiting = true;
        yield return new WaitForSeconds(cached.waitAtPointSeconds);

        if (cached == null || cached.unit == null || cached.patrolPoints.Length == 0) {
            isWaiting = false;
            yield break;
        }

        cached.currentPatrolIndex = (cached.currentPatrolIndex + 1) % cached.patrolPoints.Length;
        cached.unit.StartFollowing(cached.patrolPoints[cached.currentPatrolIndex]);
        isWaiting = false;
    }
}