using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyManager))]
[CanEditMultipleObjects]

public class EnemyManagerEditor : Editor
{
    static bool foldRole = true;
    static bool foldArchetype = true;
    static bool foldVision = true;
    static bool foldMovement = true;
    static bool foldMemory = true;
    static bool foldChase = true;
    static bool foldAttack = true;
    static bool foldCombat = true;
    static bool foldFollow = true;
    static bool foldWander = true;
    static bool foldHearing = true;
    static bool foldDebug = true;
    static bool foldDeps = true;
    static bool foldPatrol = true;

    public override void OnInspectorGUI() {
        serializedObject.Update();

        DrawFold("Rol / Escuadra", ref foldRole,
            "role",
            "squadGroup"
        );

        DrawFold("Arquetipo (SO Opcional)", ref foldArchetype,
            "archetype",
            "applyOnAwake",
            "applyInEditor"
        );

        DrawFold("Visión y Rangos", ref foldVision,
            "detectionRange",
            "attackRange",
            "viewAngle"
        );

        DrawFold("Movimiento", ref foldMovement,
            "moveSpeed",
            "turnSpeed",
            "stoppingDistance",
            "turnDst"
        );

        DrawFold("Memoria visual", ref foldMemory,
            "targetMemorySeconds"
        );

        DrawFold("Persecución (Chase)", ref foldChase,
            "chaseMaxLostSightTime",
            "chaseExitDistanceExtra",
            "chaseRepathInterval",
            "chaseRequireLineOfSight"
        );

        DrawFold("Ataque (Attack)", ref foldAttack,
            "maxLostSightTime",
            "exitAttackExtra"
        );

        DrawFold("Combate: colisiones y strafe", ref foldCombat,
            "combatObstacleMask",
            "combatSkin",
            "strafeSpeedFactor",
            "strafeBlockedFramesToFlip"
        );

        DrawFold("Follow (solo Grunt)", ref foldFollow,
            "followRepathInterval",
            "followAnchorMoveThreshold",
            "followSeparationStrength",
            "followSeparationRadius"
        );

        DrawFold("Vagar (Wander)", ref foldWander,
            "enableWander",
            "wanderCenter",
            "wanderRadius",
            "wanderWaitMin",
            "wanderWaitMax",
            "wanderRepathInterval",
            "wanderArriveTolerance",
            "wanderRetargetEvery"
        );

        DrawFold("Oído (Hearing)", ref foldHearing,
            "enableHearing",
            "hearingRange",
            "hearingCooldownSeconds",
            "investigateWaitSeconds"
        );

        DrawFold("Debug", ref foldDebug,
            "currentStateName",
            "currentTarget",
            "debugDrawDetectionRange",
            "debugDrawAttackRange",
            "debugDrawViewCone",
            "debugDrawRuntimeAnchor",
            "debugDrawWanderArea",
            "debugDrawPath",
            "debugDrawTurnLines",
            "debugDrawLookPoints",
            "debugDrawHearingRange",
            "debugColorDetection",
            "debugColorAttack",
            "debugColorViewCone",
            "debugColorRuntimeAnchor",
            "debugColorWander",
            "debugColorPath",
            "debugColorTurnLines",
            "debugColorLookPoints",
            "debugColorHearing"
        );

        DrawFold("Dependencias (auto)", ref foldDeps,
            "visionCollider",
            "bodyCollider",
            "rb",
            "unit",
            "shooter",
            "enemyAnimator"
        );

        DrawFold("Patrullaje (solo Elite o fallback)", ref foldPatrol,
            "patrolPoints",
            "waitAtPointSeconds"
        );

        serializedObject.ApplyModifiedProperties();
    }

    void DrawFold(string label, ref bool fold, params string[] props) {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        fold = EditorGUILayout.Foldout(fold, label, true);
        if (fold) {
            EditorGUI.indentLevel++;
            foreach (string p in props) {
                var sp = serializedObject.FindProperty(p);
                if (sp != null)
                    EditorGUILayout.PropertyField(sp);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }
}
