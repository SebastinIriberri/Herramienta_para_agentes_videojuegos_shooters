using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour {
    [Header("Referencias")]
    public EnemyPool enemyPool;
    public WavePreset[] waves;
    public Transform[] spawnPoints;

    [Header("Control")]
    public bool autoStartOnPlay = true;
    public float delayBetweenWaves = 5f;

    int currentWaveIndex = -1;
    bool sequenceRunning;
    int enemiesAliveInWave;
    readonly HashSet<EnemyManager> currentWaveEnemies = new HashSet<EnemyManager>();

    void OnEnable() {
        if (enemyPool != null)
            enemyPool.OnEnemyDespawned += HandleEnemyDespawned;
    }

    void OnDisable() {
        if (enemyPool != null)
            enemyPool.OnEnemyDespawned -= HandleEnemyDespawned;
    }

    void Start() {
        if (autoStartOnPlay && waves != null && waves.Length > 0 && enemyPool != null) {
            StartSequence();
        }
    }

    public void StartSequence() {
        if (sequenceRunning) return;
        if (waves == null || waves.Length == 0) return;
        if (enemyPool == null) return;

        StartCoroutine(RunWaveSequence());
    }

    IEnumerator RunWaveSequence() {
        sequenceRunning = true;

        for (int i = 0; i < waves.Length; i++) {
            var preset = waves[i];
            if (preset == null) continue;

            currentWaveIndex = i;
            currentWaveEnemies.Clear();
            enemiesAliveInWave = 0;

            yield return StartCoroutine(RunSingleWave(preset));

            yield return new WaitUntil(() => enemiesAliveInWave == 0);

            if (i < waves.Length - 1 && delayBetweenWaves > 0f) {
                yield return new WaitForSeconds(delayBetweenWaves);
            }
        }

        sequenceRunning = false;
    }

    IEnumerator RunSingleWave(WavePreset preset) {
        foreach (var block in preset.blocks) {
            if (block == null || block.count <= 0) continue;

            if (block.startDelay > 0f)
                yield return new WaitForSeconds(block.startDelay);

            float interval = (block.duration > 0f && block.count > 0)
                ? block.duration / block.count
                : 0f;

            for (int i = 0; i < block.count; i++) {
                SpawnOne(block.enemyId);

                if (interval > 0f)
                    yield return new WaitForSeconds(interval);
            }
        }
    }

    void SpawnOne(string enemyId) {
        if (enemyPool == null || spawnPoints == null || spawnPoints.Length == 0) return;

        var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var em = enemyPool.Spawn(enemyId, sp.position, sp.rotation);
        if (em != null) {
            enemiesAliveInWave++;
            currentWaveEnemies.Add(em);
        }
    }

    void HandleEnemyDespawned(EnemyManager em) {
        if (!currentWaveEnemies.Remove(em)) return;

        enemiesAliveInWave--;
        if (enemiesAliveInWave < 0) enemiesAliveInWave = 0;
    }

    public int GetEnemiesAliveInWave() => enemiesAliveInWave;
    public int GetCurrentWaveIndex() => currentWaveIndex;
}