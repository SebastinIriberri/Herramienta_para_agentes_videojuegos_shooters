using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyManager))]
[CanEditMultipleObjects]
public class EnemyManagerEditor : Editor {
    static bool foldRole = true;
    static bool foldArchetype = true;
    static bool foldVision = true;
    static bool foldMovement = true;
    static bool foldMemory = true;
    static bool foldChase = true;
    static bool foldAttack = true;
    static bool foldCover = true;
    static bool foldMelee = true;
    static bool foldCombat = true;
    static bool foldFollow = true;
    static bool foldWander = true;
    static bool foldHearing = true;
    static bool foldLOD = true;
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
            "viewAngle",
            "debugDrawDetectionRange",
            "debugColorDetection",
            "debugDrawViewCone",
            "debugColorViewCone"
        );

        DrawFold("Movimiento", ref foldMovement,
            "moveSpeed",
            "turnSpeed",
            "stoppingDistance",
            "turnDst",
            "debugDrawTurnLines",
            "debugColorTurnLines"
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
            "exitAttackExtra",
            "debugDrawAttackRange",
            "debugColorAttack"
        );

        DrawFold("Cobertura", ref foldCover,
            "canUseCover",
            "coverLowHealthThreshold",
            "coverUnderFireWindow",
            "coverMaxSearchRadius",
            "coverChanceOnHit",
            "coverRetryCooldown",
            "coverDuration"
        );

        DrawFold("Melee", ref foldMelee,
            "canUseMelee",
            "meleeTriggerDistance",
            "meleeRange",
            "meleeHitRadius",
            "meleeForwardOffset",
            "meleeAngle",
            "meleeDamage",
            "meleeCooldown",
            "meleeFailSafeSeconds",
            "meleeHitMask",
            "postMeleeShootBlockSeconds",
            "debugDrawMeleeTrigger",
            "debugColorMeleeTrigger",
            "debugDrawMeleeHit",
            "debugColorMeleeHit",
            "debugDrawMeleeAngle",
            "debugColorMeleeAngle"
        );

        DrawFold("Combate: colisiones y strafe", ref foldCombat,
            "canStrafe",
            "combatObstacleMask",
            "combatSkin",
            "strafeSpeedFactor",
            "strafeBlockedFramesToFlip",
            "debugDrawPath",
            "debugColorPath",
            "debugDrawLookPoints",
            "debugColorLookPoints"
        );

        DrawFold("Follow (solo Grunt)", ref foldFollow,
            "followRepathInterval",
            "followAnchorMoveThreshold",
            "followSeparationStrength",
            "followSeparationRadius",
            "debugDrawRuntimeAnchor",
            "debugColorRuntimeAnchor"
        );

        DrawFold("Vagar (Wander)", ref foldWander,
            "enableWander",
            "wanderCenter",
            "wanderRadius",
            "wanderWaitMin",
            "wanderWaitMax",
            "wanderRepathInterval",
            "wanderArriveTolerance",
            "wanderRetargetEvery",
            "debugDrawWanderArea",
            "debugColorWander"
        );

        DrawFold("Oído (Hearing)", ref foldHearing,
            "enableHearing",
            "hearingRange",
            "hearingCooldownSeconds",
            "investigateWaitSeconds",
            "debugDrawHearingRange",
            "debugColorHearing"
        );

        DrawFold("LOD de IA", ref foldLOD,
            "currentLOD",
            "aiTickIntervalHigh",
            "aiTickIntervalMedium",
            "aiTickIntervalLow"
        );

        DrawFold("Debug", ref foldDebug,
            "currentStateName",
            "currentTarget"
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
                if (sp != null) EditorGUILayout.PropertyField(sp);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }
}
