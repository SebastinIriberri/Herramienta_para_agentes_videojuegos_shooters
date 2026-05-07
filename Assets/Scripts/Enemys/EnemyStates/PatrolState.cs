using UnityEngine;
using System.Collections;

public class PatrolState : IEnemyState
{
    bool isWaiting;
    Coroutine waitRoutine;

    public void Enter(EnemyManager m)
    {
        isWaiting = false;
        waitRoutine = null;

        if (m.unit != null && m.patrolPoints != null && m.patrolPoints.Length > 0)
        {
            Transform p = m.patrolPoints[m.currentPatrolIndex];
            m.unit.StartFollowing(p);
        }
    }

    public void Update(EnemyManager m)
    {
        if (m.currentTarget != null)
        {
            if (m.squadGroup != null) m.squadGroup.ReportPlayerSeen(m.currentTarget.position);
            m.lastSeenPos = m.currentTarget.position;
            m.lastSeenTime = Time.time;
            m.GoToChase();
            return;
        }

        if (m.squadGroup != null && m.squadGroup.enableBlackboard)
        {
            Vector3 sharedPos;
            if (m.squadGroup.TryGetRecentPlayerSeen(out sharedPos))
            {
                if (Time.time - m.lastSeenTime > m.targetMemorySeconds)
                {
                    m.lastSeenPos = sharedPos;
                    m.lastSeenTime = Time.time;
                    m.GoToChase();
                    return;
                }
            }
        }

        if (m.unit == null || m.patrolPoints == null || m.patrolPoints.Length == 0) return;

        if (m.unit.HasReachedDestination && !isWaiting && waitRoutine == null)
        {
            m.unit.StopFollowing();
            waitRoutine = m.StartCoroutine(WaitThenNextPoint(m));
        }
    }

    public void Exit(EnemyManager m)
    {
        if (waitRoutine != null)
        {
            m.StopCoroutine(waitRoutine);
            waitRoutine = null;
        }

        isWaiting = false;
        m.unit?.StopFollowing();
    }

    IEnumerator WaitThenNextPoint(EnemyManager m)
    {
        isWaiting = true;
        yield return new WaitForSeconds(m.waitAtPointSeconds);

        if (m == null || m.unit == null || m.patrolPoints == null || m.patrolPoints.Length == 0)
        {
            isWaiting = false;
            waitRoutine = null;
            yield break;
        }

        m.currentPatrolIndex = (m.currentPatrolIndex + 1) % m.patrolPoints.Length;
        m.unit.StartFollowing(m.patrolPoints[m.currentPatrolIndex]);

        isWaiting = false;
        waitRoutine = null;
    }
}
