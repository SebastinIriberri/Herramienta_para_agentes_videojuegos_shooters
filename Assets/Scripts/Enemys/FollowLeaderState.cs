using UnityEngine;
/// <summary>
/// Estado para Grunts: siguen al líder ocupando un "slot" alrededor de él.
/// No spamea rutas: reordena al Unit con un intervalo fijo y solo si la meta cambió lo suficiente.
/// </summary>
public class FollowLeaderState : IEnemyState {
    // Timers
    float _repathTimer;
    Vector3 _lastAnchor;

    // Cache
    int _mySlotIndex = -1;
    SquadGroup _squad;
    Unit _unit;

    // Parámetros
    float _repathInterval;
    float _anchorMoveThreshold;
    float _sepStrength;
    float _sepRadius;

    public void Enter(EnemyManager m) {
        _unit = m.unit;
        _squad = m.squadGroup;

        _repathTimer = 0f;
        _lastAnchor = Vector3.positiveInfinity;

        _mySlotIndex = (_squad != null) ? _squad.GetOrAssignIndex(m) : 0;

        _repathInterval = Mathf.Max(0.2f, m.followRepathInterval);
        _anchorMoveThreshold = Mathf.Max(0.05f, m.followAnchorMoveThreshold);
        _sepStrength = Mathf.Max(0f, m.followSeparationStrength);
        _sepRadius = Mathf.Max(0f, m.followSeparationRadius);

        Vector3 target = ComputeAnchor(m);
        Transform a = CreateOrGetAnchor(m, target);
        _unit?.StartFollowing(a);
        _lastAnchor = target;
    }

    public void Update(EnemyManager m) {
        // ? Si el EnemyManager ya no está activo o el Health está muerto, no hacer nada
        if (!m.isActiveAndEnabled) return;
        var h = m.GetComponent<Health>();
        if (h != null && h.IsDead) return;

        // Si aparece target, transiciona a persecución o ataque
        if (m.currentTarget != null) {
            float dist = Vector3.Distance(m.transform.position, m.currentTarget.position);
            if (dist <= m.attackRange) m.GoToAttack();
            else m.GoToChase();
            return;
        }

        // Sin squad o líder ? patrulla
        if (_squad == null || !_squad.leader) {
            m.GoToPatrol();
            return;
        }

        // Throttle de re-path
        _repathTimer -= Time.deltaTime;
        if (_repathTimer > 0f) return;

        Vector3 anchor = ComputeAnchor(m);

        // Solo reordenar si cambió lo suficiente
        if ((anchor - _lastAnchor).sqrMagnitude >= _anchorMoveThreshold * _anchorMoveThreshold) {
            Transform a = CreateOrGetAnchor(m, anchor);
            _unit?.StartFollowing(a);
            _lastAnchor = anchor;
        }

        _repathTimer = _repathInterval;
    }

    public void Exit(EnemyManager m) {
        // No-op
    }

    // === Helpers ===

    Transform CreateOrGetAnchor(EnemyManager m, Vector3 pos) {
        if (!m.runtimeAnchor) {
            var go = new GameObject($"{m.name}_FollowAnchor");
            m.runtimeAnchor = go.transform;
        }
        m.runtimeAnchor.position = pos;
        m.runtimeAnchor.rotation = Quaternion.identity;
        return m.runtimeAnchor;
    }

    Vector3 ComputeAnchor(EnemyManager m) {
        Vector3 target = (_squad != null)
            ? _squad.GetSlotPosition(_mySlotIndex)
            : m.transform.position;

        // Separación suave para evitar solapes si lo activaste
        if (_sepStrength > 0f && _sepRadius > 0f) {
            target += ComputeSeparationOffset(m, target) * _sepStrength;
        }

        if (_squad && _squad.leader) target.y = _squad.leader.position.y;
        return target;
    }

    Vector3 ComputeSeparationOffset(EnemyManager m, Vector3 around) {
        Vector3 push = Vector3.zero;
        int count = 0;

        Collider[] hits = Physics.OverlapSphere(around, _sepRadius, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++) {
            var other = hits[i];
            if (!other) continue;
            if (other.transform == m.transform) continue;
            if (!other.TryGetComponent<EnemyManager>(out var em)) continue;

            Vector3 diff = around - other.transform.position;
            diff.y = 0f;
            float d = diff.magnitude;
            if (d > 0.001f) {
                push += diff.normalized / d;
                count++;
            }
        }

        if (count > 0) push /= count;
        return push;
    }
}
