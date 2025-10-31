using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Representa un escuadrón: mantiene referencia al líder y a sus miembros.
/// Solo hay un líder activo por escuadrón (un SquadMember con rol Elite).
/// </summary>
public class Squad : MonoBehaviour
{
    [Header("Meta-datos")]
    [Tooltip("Nombre opcional para identificar el escuadrón en escena.")]
    public string squadName = "Alpha";

    [Header("Miembros (auto)")]
    [SerializeField] private SquadMember leader;
    [SerializeField] private List<SquadMember> members = new List<SquadMember>();

    public SquadMember Leader => leader;
    public IReadOnlyList<SquadMember> Members => members;

    /// <summary>Registro automático de un miembro (lo llama SquadMember.Awake).</summary>
    public void Register(SquadMember m) {
        if (!m) return;
        if (!members.Contains(m)) members.Add(m);
        if (m.Role == EnemyRole.Elite) {
            leader = m;
        }
    }

    /// <summary>Elimina un miembro del registro (por muerte o des-spawn).</summary>
    public void Unregister(SquadMember m) {
        if (!m) return;
        members.Remove(m);
        if (leader == m) leader = null;
    }

    /// <summary>Promueve al primer Grunt válido a Elite. Devuelve el nuevo líder o null.</summary>
    public SquadMember PromoteFirstGruntAsLeader() {
        foreach (var m in members) {
            if (m && m.Role == EnemyRole.Grunt && !m.IsDead) {
                m.PromoteToLeader();
                leader = m;
                return leader;
            }
        }
        return null;
    }
}
