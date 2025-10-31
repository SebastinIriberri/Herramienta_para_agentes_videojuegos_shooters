using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Gestiona una escuadra: líder + miembros con slots alrededor del líder.
/// No hace Update; solo provee cálculos de "slot position" bajo demanda.
/// </summary>
public class SquadGroup : MonoBehaviour {
    [Header("Referencias")]
    [Tooltip("Transform del líder (normalmente un Elite). Si está vacío, usa este GameObject.")]
    public Transform leader;

    [Header("Anillos / Slots")]
    [Tooltip("Radio del primer anillo alrededor del líder.")]
    public float ringRadius = 2.5f;

    [Tooltip("Número de slots en el primer anillo.")]
    public int slotsFirstRing = 6;

    [Tooltip("Aumento de radio por cada anillo adicional.")]
    public float ringRadiusStep = 1.5f;

    [Tooltip("Número de slots en anillos extra (0 = usar el del primer anillo).")]
    public int extraSlotsPerRing = 0;

    [Header("Orientación")]
    [Tooltip("Si true, los slots se orientan respecto al forward del líder; si false, al norte mundial.")]
    public bool alignToLeaderForward = true;

    // Registro simple: enemigo -> índice de slot
    private readonly Dictionary<EnemyManager, int> _indexByMember = new();
    private readonly List<EnemyManager> _roster = new();

    void Awake() {
        if (!leader) leader = transform;
    }

    /// <summary>Registra un miembro y devuelve su índice de slot asignado (idempotente).</summary>
    public int Register(EnemyManager m) {
        if (!m) return -1;
        if (!_indexByMember.ContainsKey(m)) {
            _indexByMember[m] = _roster.Count;
            _roster.Add(m);
        }
        return _indexByMember[m];
    }

    /// <summary>Desregistra a un miembro (opcional, útil si muere/desaparece).</summary>
    public void Unregister(EnemyManager m) {
        if (!m) return;
        if (_indexByMember.Remove(m)) {
            _roster.Remove(m);
            // Nota: no recompactamos índices para mantener estabilidad en runtime.
        }
    }

    /// <summary>Atajo: obtiene (o asigna) un índice de slot para el miembro.</summary>
    public int GetOrAssignIndex(EnemyManager m) => Register(m);

    /// <summary>Devuelve la posición destino para un índice de slot.</summary>
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

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        if (!leader) leader = transform;
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.2f);
        // Dibuja primer anillo
        Gizmos.DrawWireSphere(leader.position, ringRadius);
    }
#endif
}
