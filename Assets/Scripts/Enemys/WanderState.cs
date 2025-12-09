using UnityEngine;


public class WanderState : IEnemyState {
    float waitTimer;
    float repathTimer;
    float nextRetargetTime;

    Vector3 currentGoal;

    public void Enter(EnemyManager m) {
        waitTimer = 0f;
        repathTimer = 0f;
        nextRetargetTime = Time.time + Random.Range(m.wanderRetargetEvery.x, m.wanderRetargetEvery.y);

        PickNewGoal(m);
        m.FollowPoint(currentGoal);
    }

    public void Update(EnemyManager m) {
        if (m.currentTarget != null) {
            float dist = Vector3.Distance(m.transform.position, m.currentTarget.position);
            if (dist <= m.attackRange) m.GoToAttack();
            else m.GoToChase();
            return;
        }

        float arriveTol = Mathf.Max(0.05f, m.wanderArriveTolerance) + Mathf.Max(0f, m.stoppingDistance);
        float distToGoal = Vector3.Distance(m.transform.position, currentGoal);

        bool arrived = false;
        if (m.unit != null && m.unit.HasReachedDestination) {
            arrived = true;
        }
        else if (distToGoal <= arriveTol) {
            arrived = true;
        }

        if (arrived) {
            m.unit?.StopFollowing();

            if (waitTimer <= 0f) {
                waitTimer = Random.Range(m.wanderWaitMin, m.wanderWaitMax);
            }
            else {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f) {
                    PickNewGoal(m);
                    m.FollowPoint(currentGoal);
                    nextRetargetTime = Time.time + Random.Range(m.wanderRetargetEvery.x, m.wanderRetargetEvery.y);
                }
            }

            RotateTowards(m, currentGoal);
            return;
        }

        if (Time.time >= nextRetargetTime) {
            PickNewGoal(m);
            m.FollowPoint(currentGoal);
            nextRetargetTime = Time.time + Random.Range(m.wanderRetargetEvery.x, m.wanderRetargetEvery.y);
        }

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f) {
            m.FollowPoint(currentGoal);
            float factor = m.GetLODRepathMultiplier();
            repathTimer = Mathf.Max(0.25f, m.wanderRepathInterval * factor);
        }

        RotateTowards(m, currentGoal);
    }

    public void Exit(EnemyManager m) {
        m.unit?.StopFollowing();
    }

    void PickNewGoal(EnemyManager m) {
        Vector3 center = m.GetWanderCenter();
        Vector2 rnd = Random.insideUnitCircle * Mathf.Max(0.5f, m.wanderRadius);
        currentGoal = new Vector3(center.x + rnd.x, center.y, center.z + rnd.y);
        m.lastSeenPos = currentGoal;
    }

    void RotateTowards(EnemyManager m, Vector3 point) {
        Vector3 dir = point - m.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
        float y = Mathf.LerpAngle(m.transform.eulerAngles.y, look.eulerAngles.y, Time.deltaTime * m.turnSpeed);
        m.transform.rotation = Quaternion.Euler(0f, y, 0f);
    }
}
