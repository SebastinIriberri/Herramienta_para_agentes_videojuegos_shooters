using UnityEngine;

public class ChaseState : IEnemyState {
    private float lostSightTimer;
    private float repathTimer;

    public void Enter(EnemyManager m) {
        lostSightTimer = 0f;
        repathTimer = 0f;

        if (m.currentTarget != null) {
            m.unit?.StartFollowing(m.currentTarget);
            m.lastSeenPos = m.currentTarget.position;
            m.lastSeenTime = Time.time;
        }
    }

    public void Update(EnemyManager m) {
        // Si no hay target, sigue la última posición vista si aún está en memoria
        if (m.currentTarget == null) {
            if (Time.time - m.lastSeenTime <= m.targetMemorySeconds) {
                // Crear (o reusar) un transform temporal para la última posición
                if (m.runtimeAnchor == null) {
                    GameObject temp = new GameObject($"{m.name}_LastSeenAnchor");
                    m.runtimeAnchor = temp.transform;
                }
                m.runtimeAnchor.position = m.lastSeenPos;
                m.unit?.StartFollowing(m.runtimeAnchor);
            }
            else {
                m.GoToPatrol();
            }
            return;
        }

        float dist = Vector3.Distance(m.transform.position, m.currentTarget.position);

        // Si está en rango de ataque
        if (dist <= m.attackRange) {
            m.GoToAttack();
            return;
        }

        // Si el jugador se aleja demasiado
        if (dist > m.detectionRange + m.chaseExitDistanceExtra) {
            m.GoToPatrol();
            return;
        }

        // Ver si puede ver al jugador
        bool inFOV = m.IsInFOV(m.currentTarget);
        bool hasLOS = m.HasLineOfSight(m.currentTarget, m.detectionRange);
        bool visible = inFOV && (!m.chaseRequireLineOfSight || hasLOS);

        if (visible) {
            lostSightTimer = 0f;
            m.lastSeenPos = m.currentTarget.position;
            m.lastSeenTime = Time.time;
        }
        else {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= m.chaseMaxLostSightTime) {
                if (Time.time - m.lastSeenTime <= m.targetMemorySeconds) {
                    // Reusar el transform temporal
                    if (m.runtimeAnchor == null) {
                        GameObject temp = new GameObject($"{m.name}_LastSeenAnchor");
                        m.runtimeAnchor = temp.transform;
                    }
                    m.runtimeAnchor.position = m.lastSeenPos;
                    m.unit?.StartFollowing(m.runtimeAnchor);
                }
                else {
                    m.GoToPatrol();
                }
                return;
            }
        }

        // Girar hacia el jugador
        Vector3 dir = m.currentTarget.position - m.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f) {
            Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            float y = Mathf.LerpAngle(m.transform.eulerAngles.y, lookRot.eulerAngles.y, Time.deltaTime * m.turnSpeed);
            m.transform.rotation = Quaternion.Euler(0f, y, 0f);
        }

        // Recalcular ruta periódicamente
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