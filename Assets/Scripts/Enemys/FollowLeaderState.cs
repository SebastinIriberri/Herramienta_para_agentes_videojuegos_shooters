using UnityEngine;
public class FollowLeaderState : IEnemyState {
    int slotIndex = -1;
    float repathTimer;

    public void Enter(EnemyManager m) {
        repathTimer = 0f;
        slotIndex = -1;
        if (m.squadGroup != null) {
            slotIndex = m.squadGroup.GetOrAssignIndex(m);
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

        if (m.squadGroup == null || m.unit == null) {
            m.GoToPatrol();
            return;
        }

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f) {
            Vector3 slotPos = m.squadGroup.GetSlotPosition(slotIndex);
            m.FollowPoint(slotPos);
            repathTimer = Mathf.Max(0.05f, m.followRepathInterval);
        }
    }

    public void Exit(EnemyManager m) {
        m.unit?.StopFollowing();
    }
}
