using System.Collections.Generic;
using UnityEngine;

public class CoverPoint : MonoBehaviour
{
    [Tooltip("Radio visual para debug, no afecta la búsqueda.")]
    public float radius = 1.5f;

    [Tooltip("Si es false, solo un enemy puede usar este cover a la vez.")]
    public bool allowMultipleUsers = false;

    readonly HashSet<EnemyManager> users = new HashSet<EnemyManager>();

    public bool IsAvailable => allowMultipleUsers || users.Count == 0;

    public Vector3 Position => transform.position;

    public void Reserve(EnemyManager enemy) {
        if (!allowMultipleUsers)
            users.Clear();

        users.Add(enemy);
    }

    public void Release(EnemyManager enemy) {
        if (users.Contains(enemy))
            users.Remove(enemy);
    }

    void OnEnable() {
        if (CoverManager.Instance != null)
            CoverManager.Instance.Register(this);
    }

    void OnDisable() {
        if (CoverManager.Instance != null)
            CoverManager.Instance.Unregister(this);
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}

