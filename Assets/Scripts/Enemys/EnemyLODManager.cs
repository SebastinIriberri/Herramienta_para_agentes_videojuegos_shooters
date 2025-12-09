using System.Collections.Generic;
using UnityEngine;

public class EnemyLODManager : MonoBehaviour {
    public static EnemyLODManager Instance { get; private set; }

    [Header("Referencia")]
    [Tooltip("Transform objetivo para calcular distancia. Si está vacío, buscará al 'Player' y luego a la cámara principal automáticamente.")]
    public Transform target;

    [Header("Actualización de LOD")]
    [Tooltip("Cada cuántos segundos se recalcula el LOD de cada enemigo. Intervalos más grandes reducen coste pero tardan más en reaccionar.")]
    public float updateInterval = 0.25f;

    [Header("Rangos de LOD")]
    [Tooltip("Dentro de esta distancia, los enemigos usan IA completa (máxima frecuencia de update y raycasts).")]
    public float highRange = 15f;

    [Tooltip("Entre HighRange y MediumRange, los enemigos usan IA de coste medio. Más allá pasan a Low.")]
    public float mediumRange = 35f;

    [Header("Debug Visual")]
    [Tooltip("Dibujar en la escena las zonas de rango LOD alrededor del target.")]
    public bool debugDrawRanges = true;

    [Tooltip("Color del rango HIGH (IA completa).")]
    public Color debugColorHigh = new Color(0f, 1f, 0f, 0.5f);

    [Tooltip("Color del rango MEDIUM (IA media).")]
    public Color debugColorMedium = new Color(1f, 1f, 0f, 0.5f);

    [Tooltip("Color del rango LOW (más allá de mediumRange). Solo referencia visual.")]
    public Color debugColorLow = new Color(1f, 0f, 0f, 0.15f);

    readonly List<EnemyManager> enemies = new List<EnemyManager>();
    float _timer;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    public void Register(EnemyManager m) {
        if (m == null) return;
        if (!enemies.Contains(m)) enemies.Add(m);
    }

    public void Unregister(EnemyManager m) {
        if (m == null) return;
        enemies.Remove(m);
    }

    void Update() {
        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = updateInterval;

        if (!target) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
            else if (Camera.main) target = Camera.main.transform;
        }
        if (!target) return;

        float highSqr = highRange * highRange;
        float medSqr = mediumRange * mediumRange;

        for (int i = 0; i < enemies.Count; i++) {
            var e = enemies[i];
            if (!e || !e.isActiveAndEnabled) continue;

            Vector3 diff = e.transform.position - target.position;
            float sqr = diff.sqrMagnitude;

            if (sqr <= highSqr) e.SetLOD(EnemyAILOD.High);
            else if (sqr <= medSqr) e.SetLOD(EnemyAILOD.Medium);
            else e.SetLOD(EnemyAILOD.Low);
        }
    }

    void OnDrawGizmos() {
        if (!debugDrawRanges) return;

        Transform centerTarget = target;
        if (!centerTarget) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) centerTarget = player.transform;
            else if (Camera.main) centerTarget = Camera.main.transform;
        }
        if (!centerTarget) return;

        Vector3 center = centerTarget.position;

        if (mediumRange > 0f) {
            Gizmos.color = debugColorLow;
            Gizmos.DrawWireSphere(center, mediumRange * 1.3f);
        }

        if (mediumRange > 0f) {
            Gizmos.color = debugColorMedium;
            Gizmos.DrawWireSphere(center, mediumRange);
        }

        if (highRange > 0f) {
            Gizmos.color = debugColorHigh;
            Gizmos.DrawWireSphere(center, highRange);
        }
    }
}
