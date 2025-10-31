using UnityEngine;
/// <summary>
/// Vaga entre puntos aleatorios dentro de un radio alrededor de un centro (spawn o transform asignado).
/// - Solo ordena movimiento cada cierto intervalo (anti-spam).
/// - Espera un tiempo aleatorio al llegar antes de escoger un nuevo punto.
/// - Si detecta al jugador, transiciona a Chase/Attack según distancia.
/// </summary>
public class WanderState : IEnemyState {
    float waitTimer;          // tiempo restante de espera al llegar
    float repathTimer;        // throttle para StartFollowing
    float nextRetargetTime;   // tiempo absoluto para retarget forzado

    Vector3 currentGoal;

    public void Enter(EnemyManager m) {
        waitTimer = 0f;
        repathTimer = 0f;

        // Programa primer retarget forzado
        nextRetargetTime = Time.time + Random.Range(m.wanderRetargetEvery.x, m.wanderRetargetEvery.y);

        PickNewGoal(m);
        m.FollowPoint(currentGoal);
    }

    public void Update(EnemyManager m) {
        // 1) Prioridad: si detecta jugador, transiciona
        if (m.currentTarget != null) {
            float dist = Vector3.Distance(m.transform.position, m.currentTarget.position);
            if (dist <= m.attackRange) m.GoToAttack();
            else m.GoToChase();
            return;
        }

        // 2) ¿Llegó al objetivo? -> espera y luego elige otro
        float arriveTol = Mathf.Max(0.05f, m.wanderArriveTolerance) + Mathf.Max(0f, m.stoppingDistance);
        float distToGoal = Vector3.Distance(m.transform.position, currentGoal);

        if (distToGoal <= arriveTol) {
            // Detener el path para que no empuje en el borde del destino
            m.unit?.StopFollowing();

            if (waitTimer <= 0f) {
                waitTimer = Random.Range(m.wanderWaitMin, m.wanderWaitMax);
            }
            else {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f) {
                    // Al terminar la espera, elige un nuevo punto y reprograma retarget
                    PickNewGoal(m);
                    m.FollowPoint(currentGoal);
                    nextRetargetTime = Time.time + Random.Range(m.wanderRetargetEvery.x, m.wanderRetargetEvery.y);
                }
            }

            // Aún así, rotación suave hacia el objetivo (solo estética)
            RotateTowards(m, currentGoal);
            return;
        }

        // 3) Retarget programado aunque no haya llegado
        if (Time.time >= nextRetargetTime) {
            PickNewGoal(m);
            m.FollowPoint(currentGoal);
            nextRetargetTime = Time.time + Random.Range(m.wanderRetargetEvery.x, m.wanderRetargetEvery.y);
        }

        // 4) Reafirmar destino de vez en cuando (anti-spam)
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f) {
            m.FollowPoint(currentGoal);
            repathTimer = Mathf.Max(0.25f, m.wanderRepathInterval);
        }

        // 5) Giro suave hacia la meta
        RotateTowards(m, currentGoal);
    }

    public void Exit(EnemyManager m) {
        // opcional: m.unit?.StopFollowing();
    }

    // === Helpers ===

    void PickNewGoal(EnemyManager m) {
        Vector3 center = m.GetWanderCenter();
        Vector2 rnd = Random.insideUnitCircle * Mathf.Max(0.5f, m.wanderRadius);
        currentGoal = new Vector3(center.x + rnd.x, center.y, center.z + rnd.y);
        m.lastSeenPos = currentGoal; // debug opcional
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
