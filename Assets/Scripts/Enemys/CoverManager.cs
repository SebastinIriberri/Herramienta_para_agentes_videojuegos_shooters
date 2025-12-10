using System.Collections.Generic;
using UnityEngine;

public class CoverManager : MonoBehaviour
{
    public static CoverManager Instance { get; private set; }

    readonly List<CoverPoint> _covers = new List<CoverPoint>();

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _covers.Clear();
        _covers.AddRange(FindObjectsOfType<CoverPoint>());
        Debug.Log($"[CoverManager] Registrados iniciales: {_covers.Count} cover points.");
    }

    public void Register(CoverPoint cp) {
        if (cp == null) return;
        if (!_covers.Contains(cp))
            _covers.Add(cp);
    }

    public void Unregister(CoverPoint cp) {
        if (cp == null) return;
        _covers.Remove(cp);
    }

    public bool TryFindBestCover(Vector3 enemyPos, Vector3 threatPos,
                                 float maxSearchRadius,
                                 out CoverPoint best) {
        best = null;
        float bestScore = float.NegativeInfinity;

        foreach (var cp in _covers) {
            if (cp == null || !cp.isActiveAndEnabled) continue;
            if (!cp.IsAvailable) continue;

            float dist = Vector3.Distance(enemyPos, cp.Position);
            if (dist > maxSearchRadius) continue;

            // Queremos que el cover quede ENTRE el enemy y el threat
            Vector3 toCover = (cp.Position - enemyPos).normalized;
            Vector3 fromCoverToThreat = (threatPos - cp.Position).normalized;

            // 1 = muy bueno, -1 = muy malo
            float facing = Vector3.Dot(-fromCoverToThreat, toCover);

            // Si facing es muy bajo, esta cobertura no ayuda a bloquear al jugador
            if (facing < 0.2f) continue;

            // Score simple: mejor facing y más cerca
            float score = facing * 2f - dist * 0.1f;

            if (score > bestScore) {
                bestScore = score;
                best = cp;
            }
        }

        if (best != null)
            Debug.Log($"[CoverManager] Best cover encontrado: {best.name}");
        return best != null;
    }
}
