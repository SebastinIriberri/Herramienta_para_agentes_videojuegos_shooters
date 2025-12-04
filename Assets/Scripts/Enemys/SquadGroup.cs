using System.Collections.Generic;
using UnityEngine;

public class SquadGroup : MonoBehaviour {
    [Header("Referencias")]
    public Transform leader;

    [Header("Anillos / Slots")]
    public float ringRadius = 3.5f;
    public int slotsFirstRing = 4;
    public float ringRadiusStep = 2.5f;
    public int extraSlotsPerRing = 0;

    [Header("Orientación")]
    public bool alignToLeaderForward = true;

    [Header("Blackboard (por escuadra)")]
    public bool enableBlackboard = true;
    public float sharedMemorySeconds = 4f;

    public Vector3 lastPlayerSeenPos;
    public float lastPlayerSeenTime = -999f;

    public Vector3 lastNoisePos;
    public float lastNoiseTime = -999f;

    readonly Dictionary<EnemyManager, int> _indexByMember = new();
    readonly List<EnemyManager> _roster = new();

    void Awake() {
        if (!leader) leader = transform;
    }

    public int Register(EnemyManager m) {
        if (!m) return -1;
        if (!_indexByMember.ContainsKey(m)) {
            _indexByMember[m] = _roster.Count;
            _roster.Add(m);
        }
        return _indexByMember[m];
    }

    public void Unregister(EnemyManager m) {
        if (!m) return;
        if (_indexByMember.Remove(m)) {
            _roster.Remove(m);
        }
    }

    public int GetOrAssignIndex(EnemyManager m) => Register(m);

    public Vector3 GetSlotPosition(int slotIndex) {
        if (!leader) leader = transform;
        if (slotIndex < 0) slotIndex = 0;

        int baseSlots = Mathf.Max(1, slotsFirstRing);
        int ring = 0;
        int idxInRing = slotIndex;

        int slotsThisRing = baseSlots;
        while (idxInRing >= slotsThisRing) {
            idxInRing -= slotsThisRing;
            ring++;
            slotsThisRing = (extraSlotsPerRing > 0) ? extraSlotsPerRing : baseSlots;
        }

        float radius = ringRadius + ring * ringRadiusStep;
        int slotsCount = (ring == 0) ? baseSlots : ((extraSlotsPerRing > 0) ? extraSlotsPerRing : baseSlots);

        float angleStep = 360f / Mathf.Max(1, slotsCount);
        float baseAngle = alignToLeaderForward ? leader.eulerAngles.y : 0f;

        float angle = baseAngle + idxInRing * angleStep;
        Quaternion rot = Quaternion.Euler(0f, angle, 0f);
        Vector3 dir = rot * Vector3.forward;

        Vector3 pos = leader.position + dir * radius;
        pos.y = leader.position.y;
        return pos;
    }

    public void ReportPlayerSeen(Vector3 worldPos) {
        if (!enableBlackboard) return;
        lastPlayerSeenPos = worldPos;
        lastPlayerSeenTime = Time.time;
    }

    public bool TryGetRecentPlayerSeen(out Vector3 worldPos) {
        worldPos = lastPlayerSeenPos;
        if (!enableBlackboard) return false;
        if (Time.time - lastPlayerSeenTime > sharedMemorySeconds) return false;
        return true;
    }

    public void ReportNoise(Vector3 worldPos) {
        if (!enableBlackboard) return;
        lastNoisePos = worldPos;
        lastNoiseTime = Time.time;
    }

    public bool TryGetRecentNoise(out Vector3 worldPos) {
        worldPos = lastNoisePos;
        if (!enableBlackboard) return false;
        if (Time.time - lastNoiseTime > sharedMemorySeconds) return false;
        return true;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        if (!leader) leader = transform;
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.2f);
        Gizmos.DrawWireSphere(leader.position, ringRadius);
    }
#endif
}