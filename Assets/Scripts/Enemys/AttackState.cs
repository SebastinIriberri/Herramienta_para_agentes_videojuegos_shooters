using UnityEngine;

public class AttackState : IEnemyState {

    private float lostSightTimer;

    // Strafe
    private float strafeTimer;
    private int strafeDir;                 // -1 izquierda, +1 derecha
    private int blockedFrames;             // contador de bloqueos consecutivos

    public void Enter(EnemyManager m) {
        lostSightTimer = 0f;
        blockedFrames = 0;
        m.unit?.StopFollowing();

        strafeTimer = Random.Range(0.6f, 1.2f);
        strafeDir = (Random.value < 0.5f) ? -1 : 1;
    }

    public void Update(EnemyManager m) {
        // 1) Sin objetivo => salir
        if (m.currentTarget == null) { m.GoToPatrol(); return; }

        // 2) Distancia de combate
        float dist = Vector3.Distance(m.transform.position, m.currentTarget.position);
        if (dist > m.attackRange + m.exitAttackExtra) { m.GoToChase(); return; }

        // 3) Girar hacia el jugador
        Vector3 to = m.currentTarget.position - m.transform.position; to.y = 0f;
        if (to.sqrMagnitude > 1e-4f) {
            Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
            float y = Mathf.LerpAngle(m.transform.eulerAngles.y, look.eulerAngles.y, Time.deltaTime * m.turnSpeed);
            m.transform.rotation = Quaternion.Euler(0f, y, 0f);
        }

        // 4) Línea de visión / disparo
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
                m.GoToChase(); // Chase seguirá la última posición
                return;
            }
        }

        // 5) STRAFE con pruebas de colisión
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f) {
            strafeTimer = Random.Range(0.6f, 1.2f);
            strafeDir *= -1; // alterna dirección periódicamente
        }

        // Lateral puro respecto al objetivo
        Vector3 side = Vector3.Cross(Vector3.up, to.sqrMagnitude > 1e-4f ? to.normalized : m.transform.forward) * strafeDir;

        float speed = m.moveSpeed * Mathf.Max(0.1f, m.strafeSpeedFactor);
        Vector3 desiredDelta = side * speed * Time.deltaTime;

        // No acercarse DEMASIADO al player al hacer slide
        float minKeepDistance = Mathf.Max(0.5f, m.stoppingDistance);

        // Intentar mover con pruebas de cápsula (slide si choca)
        if (TryMoveCapsuleWithSlide(m, desiredDelta, minKeepDistance, out bool moved)) {
            if (moved) {
                blockedFrames = 0;
                return;
            }
        }

        // Si aquí llega, es que estuvo bloqueado
        blockedFrames++;

        // Si se bloquea varios frames seguidos, invierte lado
        if (blockedFrames >= Mathf.Max(2, m.strafeBlockedFramesToFlip)) {
            strafeDir *= -1;
            blockedFrames = 0;

            // Fallback 2: pedir un micro-path a un ancla lateral para rodear obstáculo
            Vector3 dodgePoint = m.transform.position + side * 1.5f; // 1.5m al costado
            if (!m.runtimeAnchor) {
                var go = new GameObject($"{m.name}_CombatAnchor");
                m.runtimeAnchor = go.transform;
            }
            m.runtimeAnchor.position = dodgePoint;
            m.unit?.StartFollowing(m.runtimeAnchor);
        }
    }

    public void Exit(EnemyManager m) {
        lostSightTimer = 0f;
        blockedFrames = 0;
    }

    // ================== Helpers de movimiento seguro ==================

    /// Intenta mover la cápsula del enemigo por 'delta'. Si golpea, hace "slide" (proyección en el plano de la normal).
    /// Devuelve true si la operación fue válida y 'moved' indica si hubo desplazamiento final.
    bool TryMoveCapsuleWithSlide(EnemyManager m, Vector3 delta, float minKeepDistanceToTarget, out bool moved) {
        moved = false;
        if (delta.sqrMagnitude < 1e-8f) return true;

        if (!GetCapsule(m, out Vector3 p1, out Vector3 p2, out float radius)) return false;

        float skin = Mathf.Max(0f, m.combatSkin);
        float dist = delta.magnitude;
        Vector3 dir = delta / dist;

        // 1) CapsuleCast hacia delta
        if (Physics.CapsuleCast(p1, p2, radius - skin, dir, out RaycastHit hit, dist + skin, m.combatObstacleMask, QueryTriggerInteraction.Ignore)) {
            // 1a) Intentar SLIDE: proyectar delta sobre el plano de la normal del impacto
            Vector3 slide = Vector3.ProjectOnPlane(delta, hit.normal);

            // Evitar movimientos que acerquen demasiado al blanco
            if (m.currentTarget) {
                Vector3 nextPos = m.transform.position + slide;
                float nextDistToTarget = Vector3.Distance(nextPos, m.currentTarget.position);
                if (nextDistToTarget < minKeepDistanceToTarget) {
                    // descartamos slide que "se meta" contra el objetivo
                    slide = Vector3.zero;
                }
            }

            if (slide.sqrMagnitude > 1e-6f) {
                // Probar el slide con otra CapsuleCast corta
                float sDist = slide.magnitude;
                Vector3 sDir = slide / sDist;

                if (!Physics.CapsuleCast(p1, p2, radius - skin, sDir, out RaycastHit hit2, sDist + skin, m.combatObstacleMask, QueryTriggerInteraction.Ignore)) {
                    // libre para deslizar: mover
                    m.transform.position += slide;
                    moved = true;
                    return true;
                }
            }

            // Bloqueado: no mover
            return true;
        }
        else {
            // 2) Camino libre: mover delta completo
            m.transform.position += delta;
            moved = true;
            return true;
        }
    }

    /// Obtiene extremos y radio de la cápsula a partir del CapsuleCollider del EnemyManager.
    bool GetCapsule(EnemyManager m, out Vector3 p1, out Vector3 p2, out float radius) {
        p1 = p2 = Vector3.zero; radius = 0f;
        var cc = m.bodyCollider;
        if (!cc) return false;

        // world center
        Vector3 center = m.transform.TransformPoint(cc.center);
        float height = Mathf.Max(cc.height * Mathf.Abs(m.transform.lossyScale.y), cc.radius * 2f);
        radius = cc.radius * Mathf.Max(Mathf.Abs(m.transform.lossyScale.x), Mathf.Abs(m.transform.lossyScale.z));

        float half = Mathf.Max(0f, height * 0.5f - radius);
        Vector3 up = m.transform.up;

        p1 = center + up * half;
        p2 = center - up * half;
        return true;
    }
}