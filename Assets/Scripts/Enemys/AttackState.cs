using UnityEngine;

public class AttackState : IEnemyState {
    private float lostSightTimer;

    // Movimiento lateral (strafe)
    private float strafeTimer;
    private int strafeDir; // -1 izquierda, +1 derecha

    public void Enter(EnemyManager m) {
        lostSightTimer = 0f;
        m.unit?.StopFollowing();

        // Inicializa el strafe
        strafeTimer = Random.Range(0.6f, 1.2f);
        strafeDir = (Random.value < 0.5f) ? -1 : 1;
    }

    public void Update(EnemyManager m) {
        // Si no hay objetivo, volver a patrulla
        if (m.currentTarget == null) {
            m.GoToPatrol();
            return;
        }

        float dist = Vector3.Distance(m.transform.position, m.currentTarget.position);

        // Si está fuera del rango de ataque, volver a persecución
        if (dist > m.attackRange + m.exitAttackExtra) {
            m.GoToChase();
            return;
        }

        // Rotar hacia el jugador
        Vector3 to = m.currentTarget.position - m.transform.position;
        to.y = 0f;
        if (to.sqrMagnitude > 0.0001f) {
            Quaternion lookRot = Quaternion.LookRotation(to.normalized, Vector3.up);
            float y = Mathf.LerpAngle(m.transform.eulerAngles.y, lookRot.eulerAngles.y, Time.deltaTime * m.turnSpeed);
            m.transform.rotation = Quaternion.Euler(0f, y, 0f);
        }

        // Ver si puede ver al jugador
        bool canSee = m.IsInFOV(m.currentTarget) && m.HasLineOfSight(m.currentTarget, m.detectionRange);
        if (canSee) {
            m.shooter?.TryShoot(m.currentTarget);
            lostSightTimer = 0f;
            m.lastSeenPos = m.currentTarget.position;
            m.lastSeenTime = Time.time;
        }
        else {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= m.maxLostSightTime) {
                m.currentTarget = null;
                m.GoToChase(); // Chase intentará seguir la última posición
                return;
            }
        }
        // Movimiento lateral (strafe simple)
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f) {
            strafeTimer = Random.Range(0.6f, 1.2f);
            strafeDir *= -1; // alterna dirección
        }

        Vector3 side = Vector3.Cross(Vector3.up, to.normalized) * strafeDir;
        float strafeSpeed = m.moveSpeed * 0.6f;
        m.transform.position += side * strafeSpeed * Time.deltaTime;
    }

    public void Exit(EnemyManager m) {
        lostSightTimer = 0f;
    }
}