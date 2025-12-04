using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Path {
    public readonly Vector3[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;
    public readonly int slowDownIndex;

    public Path(Vector3[] waypoints, Vector3 startPos, float turnDst, float stoppingDst) {
        lookPoints = waypoints;
        turnBoundaries = new Line[waypoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = V3ToV2(startPos);

        for (int i = 0; i < lookPoints.Length; i++) {
            Vector2 currentPoint = V3ToV2(lookPoints[i]);
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;

            Vector2 turnBoundaryPoint =
                (i == finishLineIndex)
                ? currentPoint
                : currentPoint - dirToCurrentPoint * turnDst;

            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);
            previousPoint = turnBoundaryPoint;
        }

        float dstFromEndPoint = 0;

        for (int i = lookPoints.Length - 1; i > 0; i--) {
            dstFromEndPoint += Vector3.Distance(lookPoints[i], lookPoints[i - 1]);

            if (dstFromEndPoint > stoppingDst) {
                slowDownIndex = i;
                break;
            }
        }
    }

    Vector2 V3ToV2(Vector3 v3) {
        return new Vector2(v3.x, v3.z);
    }

    // === NUEVO: Gizmos configurables desde EnemyManager ===
    public void DrawWithGizmos(
        bool drawPath,
        bool drawTurnBoundaries,
        bool drawPoints,
        Color pathColor,
        Color turnLineColor,
        Color pointColor) {
        if (drawPoints) {
            Gizmos.color = pointColor;
            foreach (Vector3 p in lookPoints) {
                Gizmos.DrawSphere(p + Vector3.up * 0.1f, 0.15f);
            }
        }

        if (drawTurnBoundaries) {
            Gizmos.color = turnLineColor;
            foreach (Line l in turnBoundaries) {
                l.DrawWithGizmos(10);
            }
        }

        if (drawPath && lookPoints.Length > 1) {
            Gizmos.color = pathColor;

            for (int i = 0; i < lookPoints.Length - 1; i++) {
                Gizmos.DrawLine(lookPoints[i] + Vector3.up * 0.1f, lookPoints[i + 1] + Vector3.up * 0.1f);
            }
        }
    }
}
