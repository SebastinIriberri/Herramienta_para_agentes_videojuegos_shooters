using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour {
    [Serializable]
    public class Entry {
        public string id;
        public EnemyManager prefab;
        public int initialSize = 5;
    }

    [Header("Configuración")]
    public List<Entry> entries = new List<Entry>();

    readonly Dictionary<string, Queue<EnemyManager>> _poolById = new();
    readonly Dictionary<string, EnemyManager> _prefabById = new();
    readonly Dictionary<EnemyManager, string> _idByInstance = new();

    public event Action<EnemyManager> OnEnemyDespawned;

    void Awake() {
        foreach (var e in entries) {
            if (e == null || e.prefab == null || string.IsNullOrEmpty(e.id)) continue;

            if (!_poolById.ContainsKey(e.id)) {
                _poolById[e.id] = new Queue<EnemyManager>();
                _prefabById[e.id] = e.prefab;
            }

            for (int i = 0; i < e.initialSize; i++) {
                var inst = CreateInstance(e.id);
                _poolById[e.id].Enqueue(inst);
            }
        }
    }

    EnemyManager CreateInstance(string id) {
        if (!_prefabById.TryGetValue(id, out var prefab) || prefab == null) {
            Debug.LogWarning($"{name}: No prefab para id '{id}'");
            return null;
        }

        var inst = Instantiate(prefab, transform);
        inst.gameObject.SetActive(false);

        var health = inst.GetComponent<Health>();
        if (health != null) {
            health.usePooling = true;
            health.enemyPool = this;
            health.deactivateOnDeath = false;
        }

        _idByInstance[inst] = id;
        return inst;
    }

    public EnemyManager Spawn(string id, Vector3 position, Quaternion rotation) {
        if (!_poolById.TryGetValue(id, out var queue)) {
            Debug.LogWarning($"{name}: No existe pool para id '{id}'");
            return null;
        }

        EnemyManager inst = null;

        while (queue.Count > 0 && inst == null) {
            inst = queue.Dequeue();
        }

        if (inst == null) {
            inst = CreateInstance(id);
            if (inst == null) return null;
        }

        inst.transform.SetPositionAndRotation(position, rotation);
        inst.gameObject.SetActive(true);

        inst.ResetForRespawn(position, rotation);

        return inst;
    }

    public void Despawn(GameObject go) {
        if (!go) return;

        var enemy = go.GetComponent<EnemyManager>();
        if (!enemy) {
            go.SetActive(false);
            return;
        }

        if (!_idByInstance.TryGetValue(enemy, out var id)) {
            go.SetActive(false);
            return;
        }

        enemy.gameObject.SetActive(false);

        if (!_poolById.TryGetValue(id, out var queue)) {
            _poolById[id] = new Queue<EnemyManager>();
            queue = _poolById[id];
        }

        queue.Enqueue(enemy);
        enemy.transform.SetParent(transform, false);

        OnEnemyDespawned?.Invoke(enemy);
    }

}
