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

    readonly string[] roleOptions =
    {
        "Básico / Grunt",
        "Élite / Elite"
    };

    readonly string[] lodOptions =
    {
        "Alto",
        "Medio",
        "Bajo"
    };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawFold("Rol y escuadra", ref foldRole,
            "role",
            "squadGroup"
        );

        DrawFold("Arquetipo opcional", ref foldArchetype,
            "archetype",
            "applyOnAwake",
            "applyInEditor"
        );

        DrawFold("Visión y rangos", ref foldVision,
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

        DrawFold("Memoria del objetivo", ref foldMemory,
            "targetMemorySeconds"
        );

        DrawFold("Persecución", ref foldChase,
            "chaseMaxLostSightTime",
            "chaseExitDistanceExtra",
            "chaseRepathInterval",
            "chaseRequireLineOfSight"
        );

        DrawFold("Ataque a distancia", ref foldAttack,
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

        DrawFold("Ataque cuerpo a cuerpo", ref foldMelee,
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

        DrawFold("Combate: colisiones y movimiento lateral", ref foldCombat,
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

        DrawFold("Seguimiento de líder", ref foldFollow,
            "followRepathInterval",
            "followAnchorMoveThreshold",
            "followSeparationStrength",
            "followSeparationRadius",
            "debugDrawRuntimeAnchor",
            "debugColorRuntimeAnchor"
        );

        DrawFold("Vagar por el escenario", ref foldWander,
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

        DrawFold("Oído e investigación de sonidos", ref foldHearing,
            "enableHearing",
            "hearingRange",
            "hearingCooldownSeconds",
            "investigateWaitSeconds",
            "debugDrawHearingRange",
            "debugColorHearing"
        );

        DrawFold("Optimización de IA", ref foldLOD,
            "currentLOD",
            "aiTickIntervalHigh",
            "aiTickIntervalMedium",
            "aiTickIntervalLow"
        );

        DrawFold("Depuración", ref foldDebug,
            "currentStateName",
            "currentTarget"
        );

        DrawFold("Dependencias automáticas", ref foldDeps,
            "visionCollider",
            "bodyCollider",
            "rb",
            "unit",
            "shooter",
            "enemyAnimator"
        );

        DrawFold("Patrullaje", ref foldPatrol,
            "patrolPoints",
            "waitAtPointSeconds"
        );

        serializedObject.ApplyModifiedProperties();
    }

    void DrawFold(string label, ref bool fold, params string[] props)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        fold = EditorGUILayout.Foldout(fold, label, true);

        if (fold)
        {
            EditorGUI.indentLevel++;

            foreach (string propertyName in props)
            {
                DrawProperty(propertyName);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    void DrawProperty(string propertyName)
    {
        SerializedProperty sp = serializedObject.FindProperty(propertyName);

        if (sp == null)
            return;

        switch (propertyName)
        {
            case "role":
                DrawEnumPopup(sp, Label(propertyName), roleOptions);
                break;

            case "currentLOD":
                DrawEnumPopup(sp, Label(propertyName), lodOptions);
                break;

            case "currentStateName":
                DrawCurrentState(sp);
                break;

            case "patrolPoints":
                DrawPatrolPoints(sp);
                break;

            default:
                EditorGUILayout.PropertyField(sp, Label(propertyName), true);
                break;
        }
    }

    void DrawEnumPopup(SerializedProperty sp, GUIContent label, string[] options)
    {
        if (sp.propertyType != SerializedPropertyType.Enum)
        {
            EditorGUILayout.PropertyField(sp, label, true);
            return;
        }

        EditorGUI.showMixedValue = sp.hasMultipleDifferentValues;

        EditorGUI.BeginChangeCheck();

        int newValue = EditorGUILayout.Popup(label, sp.enumValueIndex, options);

        if (EditorGUI.EndChangeCheck())
        {
            sp.enumValueIndex = newValue;
        }

        EditorGUI.showMixedValue = false;
    }

    void DrawCurrentState(SerializedProperty sp)
    {
        EditorGUI.BeginDisabledGroup(true);

        string stateName = sp.hasMultipleDifferentValues
            ? "Varios estados"
            : TranslateStateName(sp.stringValue);

        EditorGUILayout.TextField(Label("currentStateName"), stateName);

        EditorGUI.EndDisabledGroup();
    }

    void DrawPatrolPoints(SerializedProperty sp)
    {
        sp.isExpanded = EditorGUILayout.Foldout(sp.isExpanded, Label("patrolPoints"), true);

        if (!sp.isExpanded)
            return;

        EditorGUI.indentLevel++;

        EditorGUI.BeginChangeCheck();

        int newSize = EditorGUILayout.IntField(
            new GUIContent(
                "Cantidad de puntos",
                "Número de puntos que el enemigo puede usar para patrullar."
            ),
            sp.arraySize
        );

        if (EditorGUI.EndChangeCheck())
        {
            sp.arraySize = Mathf.Max(0, newSize);
        }

        for (int i = 0; i < sp.arraySize; i++)
        {
            SerializedProperty element = sp.GetArrayElementAtIndex(i);

            EditorGUILayout.PropertyField(
                element,
                new GUIContent(
                    $"Punto {i + 1}",
                    "Transform usado como punto de patrullaje."
                )
            );
        }

        EditorGUI.indentLevel--;
    }

    string TranslateStateName(string stateName)
    {
        switch (stateName)
        {
            case "(none)": return "Sin estado";
            case "PatrolState": return "Patrullaje";
            case "ChaseState": return "Persecución";
            case "AttackState": return "Ataque";
            case "FollowLeaderState": return "Seguir líder";
            case "WanderState": return "Vagar";
            case "InvestigateNoiseState": return "Investigar sonido";
            case "CoverState": return "Cobertura";
            case "ReloadState": return "Recarga";
            case "MeleeState": return "Ataque cuerpo a cuerpo";
            default: return stateName;
        }
    }

    GUIContent Label(string propertyName)
    {
        switch (propertyName)
        {
            // Rol y escuadra
            case "role":
                return new GUIContent(
                    "Rol del enemigo",
                    "Define el tipo de enemigo. Puede afectar su comportamiento base y su relación con la escuadra."
                );

            case "squadGroup":
                return new GUIContent(
                    "Grupo o escuadra",
                    "Grupo al que pertenece el enemigo. Sirve para comportamientos de escuadra o seguimiento."
                );

            // Arquetipo
            case "archetype":
                return new GUIContent(
                    "Arquetipo",
                    "Configuración reutilizable con valores base para crear variantes de enemigos."
                );

            case "applyOnAwake":
                return new GUIContent(
                    "Aplicar al iniciar",
                    "Si está activo, el arquetipo se aplica cuando inicia la escena."
                );

            case "applyInEditor":
                return new GUIContent(
                    "Aplicar en el editor",
                    "Si está activo, el arquetipo actualiza los valores mientras editas el enemigo en Unity."
                );

            // Visión y rangos
            case "detectionRange":
                return new GUIContent(
                    "Rango de detección",
                    "Distancia máxima a la que el enemigo puede detectar al jugador."
                );

            case "attackRange":
                return new GUIContent(
                    "Rango de ataque",
                    "Distancia a la que el enemigo puede intentar atacar al jugador."
                );

            case "viewAngle":
                return new GUIContent(
                    "Ángulo de visión",
                    "Apertura del campo de visión del enemigo."
                );

            case "debugDrawDetectionRange":
                return new GUIContent(
                    "Mostrar rango de detección",
                    "Dibuja el rango de detección en la vista de escena."
                );

            case "debugColorDetection":
                return new GUIContent(
                    "Color del rango de detección",
                    "Color usado para visualizar el rango de detección."
                );

            case "debugDrawViewCone":
                return new GUIContent(
                    "Mostrar cono de visión",
                    "Dibuja las líneas del campo de visión del enemigo."
                );

            case "debugColorViewCone":
                return new GUIContent(
                    "Color del cono de visión",
                    "Color usado para visualizar el campo de visión."
                );

            // Movimiento
            case "moveSpeed":
                return new GUIContent(
                    "Velocidad de movimiento",
                    "Velocidad a la que se desplaza el enemigo."
                );

            case "turnSpeed":
                return new GUIContent(
                    "Velocidad de giro",
                    "Qué tan rápido rota el enemigo al cambiar de dirección."
                );

            case "stoppingDistance":
                return new GUIContent(
                    "Distancia de frenado",
                    "Distancia mínima a la que el enemigo se detiene respecto a su objetivo."
                );

            case "turnDst":
                return new GUIContent(
                    "Distancia para suavizar giro",
                    "Distancia usada para anticipar giros y suavizar el seguimiento de la ruta."
                );

            case "debugDrawTurnLines":
                return new GUIContent(
                    "Mostrar líneas de giro",
                    "Dibuja líneas de ayuda para visualizar los giros del enemigo."
                );

            case "debugColorTurnLines":
                return new GUIContent(
                    "Color de líneas de giro",
                    "Color usado para visualizar las líneas de giro."
                );

            // Memoria
            case "targetMemorySeconds":
                return new GUIContent(
                    "Tiempo de memoria del objetivo",
                    "Tiempo que el enemigo recuerda la última posición conocida del jugador."
                );

            // Persecución
            case "chaseMaxLostSightTime":
                return new GUIContent(
                    "Tiempo máximo sin ver al jugador",
                    "Tiempo que el enemigo sigue persiguiendo después de perder de vista al jugador."
                );

            case "chaseExitDistanceExtra":
                return new GUIContent(
                    "Margen extra para dejar persecución",
                    "Distancia adicional antes de abandonar la persecución."
                );

            case "chaseRepathInterval":
                return new GUIContent(
                    "Intervalo para recalcular ruta",
                    "Cada cuánto tiempo se recalcula la ruta durante la persecución."
                );

            case "chaseRequireLineOfSight":
                return new GUIContent(
                    "Requerir línea de visión",
                    "Si está activo, el enemigo necesita línea de visión para mantener la persecución."
                );

            // Ataque
            case "maxLostSightTime":
                return new GUIContent(
                    "Tiempo máximo sin visión en ataque",
                    "Tiempo que el enemigo puede seguir atacando después de perder línea de visión."
                );

            case "exitAttackExtra":
                return new GUIContent(
                    "Margen extra para salir del ataque",
                    "Distancia adicional para evitar cambios bruscos entre ataque y persecución."
                );

            case "debugDrawAttackRange":
                return new GUIContent(
                    "Mostrar rango de ataque",
                    "Dibuja el rango de ataque en la vista de escena."
                );

            case "debugColorAttack":
                return new GUIContent(
                    "Color del rango de ataque",
                    "Color usado para visualizar el rango de ataque."
                );

            // Cobertura
            case "canUseCover":
                return new GUIContent(
                    "Puede usar cobertura",
                    "Permite que el enemigo busque cobertura bajo ciertas condiciones."
                );

            case "coverLowHealthThreshold":
                return new GUIContent(
                    "Umbral de vida baja",
                    "Porcentaje de vida a partir del cual el enemigo puede buscar cobertura."
                );

            case "coverUnderFireWindow":
                return new GUIContent(
                    "Ventana bajo fuego",
                    "Tiempo durante el cual se considera que el enemigo está bajo amenaza."
                );

            case "coverMaxSearchRadius":
                return new GUIContent(
                    "Radio máximo para buscar cobertura",
                    "Distancia máxima para buscar un punto de cobertura disponible."
                );

            case "coverChanceOnHit":
                return new GUIContent(
                    "Probabilidad de cubrirse al recibir daño",
                    "Probabilidad de que el enemigo busque cobertura cuando recibe daño."
                );

            case "coverRetryCooldown":
                return new GUIContent(
                    "Tiempo entre intentos de cobertura",
                    "Tiempo mínimo antes de volver a intentar buscar cobertura."
                );

            case "coverDuration":
                return new GUIContent(
                    "Duración en cobertura",
                    "Tiempo que el enemigo permanece en cobertura antes de decidir otra acción."
                );

            // Cuerpo a cuerpo
            case "canUseMelee":
                return new GUIContent(
                    "Puede usar ataque cuerpo a cuerpo",
                    "Permite que el enemigo use ataques cuerpo a cuerpo."
                );

            case "meleeTriggerDistance":
                return new GUIContent(
                    "Distancia para iniciar cuerpo a cuerpo",
                    "Distancia a partir de la cual el enemigo puede intentar un ataque cuerpo a cuerpo."
                );

            case "meleeRange":
                return new GUIContent(
                    "Alcance del golpe",
                    "Distancia efectiva del ataque cuerpo a cuerpo."
                );

            case "meleeHitRadius":
                return new GUIContent(
                    "Radio de impacto",
                    "Tamaño del área que puede golpear el ataque cuerpo a cuerpo."
                );

            case "meleeForwardOffset":
                return new GUIContent(
                    "Desplazamiento frontal del golpe",
                    "Qué tan adelante del enemigo se coloca el área de impacto."
                );

            case "meleeAngle":
                return new GUIContent(
                    "Ángulo del golpe",
                    "Ángulo frontal en el que el golpe puede impactar al objetivo."
                );

            case "meleeDamage":
                return new GUIContent(
                    "Daño cuerpo a cuerpo",
                    "Cantidad de daño que aplica el ataque cuerpo a cuerpo."
                );

            case "meleeCooldown":
                return new GUIContent(
                    "Tiempo entre ataques cuerpo a cuerpo",
                    "Tiempo mínimo entre un ataque cuerpo a cuerpo y el siguiente."
                );

            case "meleeFailSafeSeconds":
                return new GUIContent(
                    "Tiempo de seguridad del estado",
                    "Tiempo de respaldo para evitar que el enemigo quede atrapado en el ataque cuerpo a cuerpo."
                );

            case "meleeHitMask":
                return new GUIContent(
                    "Capas que puede golpear",
                    "Capas consideradas válidas para el impacto cuerpo a cuerpo."
                );

            case "postMeleeShootBlockSeconds":
                return new GUIContent(
                    "Pausa de disparo después del melee",
                    "Tiempo durante el cual se bloquea el disparo después de un ataque cuerpo a cuerpo."
                );

            case "debugDrawMeleeTrigger":
                return new GUIContent(
                    "Mostrar distancia de inicio",
                    "Dibuja la distancia para iniciar ataque cuerpo a cuerpo."
                );

            case "debugColorMeleeTrigger":
                return new GUIContent(
                    "Color de distancia de inicio",
                    "Color usado para visualizar la distancia de inicio del cuerpo a cuerpo."
                );

            case "debugDrawMeleeHit":
                return new GUIContent(
                    "Mostrar área de impacto",
                    "Dibuja el área de impacto del golpe cuerpo a cuerpo."
                );

            case "debugColorMeleeHit":
                return new GUIContent(
                    "Color del área de impacto",
                    "Color usado para visualizar el área de impacto."
                );

            case "debugDrawMeleeAngle":
                return new GUIContent(
                    "Mostrar ángulo del golpe",
                    "Dibuja el ángulo frontal del ataque cuerpo a cuerpo."
                );

            case "debugColorMeleeAngle":
                return new GUIContent(
                    "Color del ángulo del golpe",
                    "Color usado para visualizar el ángulo del golpe."
                );

            // Combate y strafe
            case "canStrafe":
                return new GUIContent(
                    "Puede moverse lateralmente",
                    "Permite que el enemigo se desplace lateralmente durante el combate."
                );

            case "combatObstacleMask":
                return new GUIContent(
                    "Capas de obstáculos",
                    "Capas que el enemigo considera como obstáculos durante el combate."
                );

            case "combatSkin":
                return new GUIContent(
                    "Margen contra obstáculos",
                    "Margen de seguridad para evitar que el enemigo se pegue demasiado a obstáculos."
                );

            case "strafeSpeedFactor":
                return new GUIContent(
                    "Velocidad lateral",
                    "Multiplicador de velocidad cuando el enemigo se mueve lateralmente."
                );

            case "strafeBlockedFramesToFlip":
                return new GUIContent(
                    "Frames bloqueado antes de cambiar lado",
                    "Cantidad de frames bloqueado antes de intentar moverse hacia el lado contrario."
                );

            case "debugDrawPath":
                return new GUIContent(
                    "Mostrar ruta",
                    "Dibuja la ruta calculada del enemigo."
                );

            case "debugColorPath":
                return new GUIContent(
                    "Color de ruta",
                    "Color usado para visualizar la ruta."
                );

            case "debugDrawLookPoints":
                return new GUIContent(
                    "Mostrar puntos de seguimiento",
                    "Dibuja los puntos que el enemigo sigue dentro de la ruta."
                );

            case "debugColorLookPoints":
                return new GUIContent(
                    "Color de puntos de seguimiento",
                    "Color usado para visualizar los puntos de seguimiento."
                );

            // Follow
            case "followRepathInterval":
                return new GUIContent(
                    "Intervalo para recalcular seguimiento",
                    "Cada cuánto tiempo se recalcula la ruta al seguir al líder."
                );

            case "followAnchorMoveThreshold":
                return new GUIContent(
                    "Distancia mínima para mover punto guía",
                    "Distancia que debe moverse el punto guía para actualizar el seguimiento."
                );

            case "followSeparationStrength":
                return new GUIContent(
                    "Fuerza de separación",
                    "Intensidad con la que los enemigos se separan entre sí al seguir al líder."
                );

            case "followSeparationRadius":
                return new GUIContent(
                    "Radio de separación",
                    "Distancia usada para mantener separación entre miembros de la escuadra."
                );

            case "debugDrawRuntimeAnchor":
                return new GUIContent(
                    "Mostrar punto guía",
                    "Dibuja el punto guía que el enemigo sigue en tiempo de ejecución."
                );

            case "debugColorRuntimeAnchor":
                return new GUIContent(
                    "Color del punto guía",
                    "Color usado para visualizar el punto guía."
                );

            // Wander
            case "enableWander":
                return new GUIContent(
                    "Permitir vagar",
                    "Permite que el enemigo se mueva dentro de un área cuando no tiene objetivo."
                );

            case "wanderCenter":
                return new GUIContent(
                    "Centro del área",
                    "Transform usado como centro del área donde el enemigo puede vagar."
                );

            case "wanderRadius":
                return new GUIContent(
                    "Radio del área",
                    "Tamaño del área donde el enemigo puede vagar."
                );

            case "wanderWaitMin":
                return new GUIContent(
                    "Espera mínima",
                    "Tiempo mínimo que el enemigo espera antes de elegir otro destino."
                );

            case "wanderWaitMax":
                return new GUIContent(
                    "Espera máxima",
                    "Tiempo máximo que el enemigo espera antes de elegir otro destino."
                );

            case "wanderRepathInterval":
                return new GUIContent(
                    "Intervalo para recalcular ruta",
                    "Cada cuánto tiempo se recalcula la ruta mientras vaga."
                );

            case "wanderArriveTolerance":
                return new GUIContent(
                    "Tolerancia de llegada",
                    "Distancia usada para considerar que el enemigo llegó a su destino."
                );

            case "wanderRetargetEvery":
                return new GUIContent(
                    "Cambiar destino cada",
                    "Rango de tiempo para elegir un nuevo destino mientras vaga."
                );

            case "debugDrawWanderArea":
                return new GUIContent(
                    "Mostrar área de vagar",
                    "Dibuja el área donde el enemigo puede vagar."
                );

            case "debugColorWander":
                return new GUIContent(
                    "Color del área de vagar",
                    "Color usado para visualizar el área de vagar."
                );

            // Oído
            case "enableHearing":
                return new GUIContent(
                    "Activar oído",
                    "Permite que el enemigo reaccione a sonidos o disparos."
                );

            case "hearingRange":
                return new GUIContent(
                    "Rango de audición",
                    "Distancia máxima a la que el enemigo puede escuchar sonidos."
                );

            case "hearingCooldownSeconds":
                return new GUIContent(
                    "Tiempo entre sonidos detectados",
                    "Tiempo mínimo antes de reaccionar a otro sonido."
                );

            case "investigateWaitSeconds":
                return new GUIContent(
                    "Tiempo de investigación",
                    "Tiempo que el enemigo permanece investigando el lugar donde escuchó el sonido."
                );

            case "debugDrawHearingRange":
                return new GUIContent(
                    "Mostrar rango de audición",
                    "Dibuja el rango de audición en la vista de escena."
                );

            case "debugColorHearing":
                return new GUIContent(
                    "Color del rango de audición",
                    "Color usado para visualizar el rango de audición."
                );

            // LOD
            case "currentLOD":
                return new GUIContent(
                    "Nivel de detalle de IA",
                    "Controla la frecuencia de actualización de la IA."
                );

            case "aiTickIntervalHigh":
                return new GUIContent(
                    "Intervalo IA alto",
                    "Tiempo entre actualizaciones cuando la IA está en nivel alto."
                );

            case "aiTickIntervalMedium":
                return new GUIContent(
                    "Intervalo IA medio",
                    "Tiempo entre actualizaciones cuando la IA está en nivel medio."
                );

            case "aiTickIntervalLow":
                return new GUIContent(
                    "Intervalo IA bajo",
                    "Tiempo entre actualizaciones cuando la IA está en nivel bajo."
                );

            // Debug
            case "currentStateName":
                return new GUIContent(
                    "Estado actual",
                    "Estado de comportamiento en el que se encuentra el enemigo."
                );

            case "currentTarget":
                return new GUIContent(
                    "Objetivo actual",
                    "Transform del jugador u objetivo que el enemigo está siguiendo o atacando."
                );

            // Dependencias
            case "visionCollider":
                return new GUIContent(
                    "Collider de visión",
                    "Collider usado para detectar al jugador dentro del rango."
                );

            case "bodyCollider":
                return new GUIContent(
                    "Collider del cuerpo",
                    "Collider físico principal del enemigo."
                );

            case "rb":
                return new GUIContent(
                    "Rigidbody",
                    "Componente físico del enemigo."
                );

            case "unit":
                return new GUIContent(
                    "Movimiento",
                    "Componente encargado del desplazamiento y seguimiento de rutas."
                );

            case "shooter":
                return new GUIContent(
                    "Sistema de disparo",
                    "Componente encargado del ataque a distancia."
                );

            case "enemyAnimator":
                return new GUIContent(
                    "Animador del enemigo",
                    "Componente encargado de reproducir animaciones del enemigo."
                );

            // Patrullaje
            case "patrolPoints":
                return new GUIContent(
                    "Puntos de patrullaje",
                    "Lista de puntos que el enemigo puede recorrer durante el patrullaje."
                );

            case "waitAtPointSeconds":
                return new GUIContent(
                    "Espera en cada punto",
                    "Tiempo que el enemigo espera al llegar a un punto de patrullaje."
                );

            default:
                return new GUIContent(ObjectNames.NicifyVariableName(propertyName));
        }
    }
}