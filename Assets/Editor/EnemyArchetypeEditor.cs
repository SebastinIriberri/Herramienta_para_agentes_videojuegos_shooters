using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyArchetype))]
[CanEditMultipleObjects]
public class EnemyArchetypeEditor : Editor
{
    static bool foldRole = true;
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
    static bool foldShooter = true;
    static bool foldRaycast = true;
    static bool foldAim = true;
    static bool foldAmmo = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.HelpBox(
            "Arquetipo reutilizable para configurar enemigos de forma rápida. " +
            "Estos valores se aplican desde EnemyManager o Enemy Designer Pro.",
            MessageType.Info
        );

        DrawFold("Rol", ref foldRole,
            "role"
        );

        DrawFold("Visión y rangos", ref foldVision,
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
            "exitAttackExtra"
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
            "postMeleeShootBlockSeconds"
        );

        DrawFold("Combate: colisiones y movimiento lateral", ref foldCombat,
            "canStrafe",
            "combatObstacleMask",
            "combatSkin",
            "strafeSpeedFactor",
            "strafeBlockedFramesToFlip"
        );

        DrawFold("Seguimiento de líder", ref foldFollow,
            "followRepathInterval",
            "followAnchorMoveThreshold",
            "followSeparationStrength",
            "followSeparationRadius"
        );

        DrawFold("Vagar por el escenario", ref foldWander,
            "enableWander",
            "wanderRadius",
            "wanderWaitMin",
            "wanderWaitMax",
            "wanderRepathInterval",
            "wanderArriveTolerance",
            "wanderRetargetEvery"
        );

        DrawFold("Oído e investigación de sonidos", ref foldHearing,
            "enableHearing",
            "hearingRange",
            "hearingCooldownSeconds",
            "investigateWaitSeconds"
        );

        DrawFold("Optimización de IA", ref foldLOD,
            "currentLOD",
            "aiTickIntervalHigh",
            "aiTickIntervalMedium",
            "aiTickIntervalLow"
        );

        DrawFold("Disparo", ref foldShooter,
            "applyShooterSettings",
            "fireMode",
            "fireRange",
            "cooldownSeconds",
            "spawnOffset",
            "bulletSettings"
        );

        DrawFold("Disparo por Raycast", ref foldRaycast,
            "raycastMask",
            "raycastDamage",
            "raycastIgnoreTriggers"
        );

        DrawFold("Apuntado y línea de tiro", ref foldAim,
            "targetHeightOffset",
            "lineOfFireMask",
            "ignoreTriggersInLineOfFire"
        );

        DrawFold("Munición y recarga", ref foldAmmo,
            "useAmmo",
            "clipSize",
            "startWithFullAmmo",
            "reloadDuration",
            "autoReload"
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
            case "currentLOD":
            case "fireMode":
                DrawTranslatedEnum(sp, Label(propertyName));
                break;

            default:
                EditorGUILayout.PropertyField(sp, Label(propertyName), true);
                break;
        }
    }

    void DrawTranslatedEnum(SerializedProperty sp, GUIContent label)
    {
        if (sp.propertyType != SerializedPropertyType.Enum)
        {
            EditorGUILayout.PropertyField(sp, label, true);
            return;
        }

        string[] translatedOptions = new string[sp.enumNames.Length];

        for (int i = 0; i < sp.enumNames.Length; i++)
        {
            translatedOptions[i] = TranslateEnumName(sp.enumNames[i]);
        }

        EditorGUI.showMixedValue = sp.hasMultipleDifferentValues;

        EditorGUI.BeginChangeCheck();

        int newIndex = EditorGUILayout.Popup(label, sp.enumValueIndex, translatedOptions);

        if (EditorGUI.EndChangeCheck())
        {
            sp.enumValueIndex = newIndex;
        }

        EditorGUI.showMixedValue = false;
    }

    string TranslateEnumName(string enumName)
    {
        switch (enumName)
        {
            case "Grunt": return "Grunt / Básico";
            case "Elite": return "Elite / Avanzado";

            case "High": return "Alto";
            case "Medium": return "Medio";
            case "Low": return "Bajo";

            case "Projectile": return "Proyectil";
            case "Raycast": return "Raycast";

            default:
                return ObjectNames.NicifyVariableName(enumName);
        }
    }

    GUIContent Label(string propertyName)
    {
        switch (propertyName)
        {
            // Rol
            case "role":
                return new GUIContent(
                    "Rol del enemigo",
                    "Define el tipo de enemigo. Grunt suele funcionar como enemigo básico; Elite como enemigo más avanzado."
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

            // Movimiento
            case "moveSpeed":
                return new GUIContent(
                    "Velocidad de movimiento",
                    "Velocidad con la que se desplaza el enemigo."
                );

            case "turnSpeed":
                return new GUIContent(
                    "Velocidad de giro",
                    "Qué tan rápido gira el enemigo hacia su dirección de movimiento u objetivo."
                );

            case "stoppingDistance":
                return new GUIContent(
                    "Distancia de frenado",
                    "Distancia mínima a la que el enemigo se detiene respecto a su destino."
                );

            case "turnDst":
                return new GUIContent(
                    "Distancia para suavizar giro",
                    "Distancia usada para anticipar y suavizar giros durante el seguimiento de ruta."
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
                    "Si está activo, el enemigo requiere línea de visión para mantener la persecución."
                );

            // Ataque
            case "maxLostSightTime":
                return new GUIContent(
                    "Tiempo máximo sin visión en ataque",
                    "Tiempo que el enemigo puede seguir en ataque después de perder línea de visión."
                );

            case "exitAttackExtra":
                return new GUIContent(
                    "Margen extra para salir del ataque",
                    "Distancia adicional para evitar cambios bruscos entre ataque y persecución."
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

            // Melee
            case "canUseMelee":
                return new GUIContent(
                    "Puede usar ataque cuerpo a cuerpo",
                    "Permite que el enemigo use ataques cuerpo a cuerpo."
                );

            case "meleeTriggerDistance":
                return new GUIContent(
                    "Distancia para iniciar cuerpo a cuerpo",
                    "Distancia a partir de la cual el enemigo puede iniciar un ataque cuerpo a cuerpo."
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

            // Combate lateral
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
                    "Margen de seguridad para evitar que el enemigo se acerque demasiado a obstáculos."
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

            // Wander
            case "enableWander":
                return new GUIContent(
                    "Permitir vagar",
                    "Permite que el enemigo se mueva dentro de un área cuando no tiene objetivo."
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
                   "Cambiar destino entre (seg)",
                   " X = tiempo mínimo. Y = tiempo máximo. Define cada cuánto el enemigo elige un nuevo punto para vagar."
                );

            // Hearing
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

            // LOD
            case "currentLOD":
                return new GUIContent(
                    "Nivel de detalle de IA",
                    "Controla la frecuencia de actualización inicial de la IA."
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

            // Shooter
            case "applyShooterSettings":
                return new GUIContent(
                    "Aplicar configuración de disparo",
                    "Si está activo, el arquetipo también aplica valores al componente EnemyShooter."
                );

            case "fireMode":
                return new GUIContent(
                    "Tipo de disparo",
                    "Define si el enemigo dispara con proyectiles o mediante Raycast."
                );

            case "fireRange":
                return new GUIContent(
                    "Rango de disparo",
                    "Distancia máxima a la que el enemigo puede disparar."
                );

            case "cooldownSeconds":
                return new GUIContent(
                    "Tiempo entre disparos",
                    "Tiempo mínimo que debe pasar entre un disparo y el siguiente."
                );

            case "spawnOffset":
                return new GUIContent(
                    "Desplazamiento de aparición",
                    "Pequeño desplazamiento usado al crear el proyectil."
                );

            case "bulletSettings":
                return new GUIContent(
                    "Configuración de bala",
                    "Datos del proyectil, como velocidad, daño o comportamiento."
                );

            // Raycast
            case "raycastMask":
                return new GUIContent(
                    "Capas que puede golpear",
                    "Capas válidas para el disparo por Raycast."
                );

            case "raycastDamage":
                return new GUIContent(
                    "Daño por Raycast",
                    "Cantidad de daño aplicado cuando el disparo por Raycast impacta al objetivo."
                );

            case "raycastIgnoreTriggers":
                return new GUIContent(
                    "Ignorar triggers",
                    "Si está activo, el Raycast ignora colliders marcados como Trigger."
                );

            // Aim / LoF
            case "targetHeightOffset":
                return new GUIContent(
                    "Altura del objetivo",
                    "Altura aproximada a la que el enemigo intenta apuntar."
                );

            case "lineOfFireMask":
                return new GUIContent(
                    "Capas para línea de tiro",
                    "Capas consideradas al comprobar si hay obstáculos o aliados entre el enemigo y el objetivo."
                );

            case "ignoreTriggersInLineOfFire":
                return new GUIContent(
                    "Ignorar triggers en línea de tiro",
                    "Si está activo, la comprobación de línea de tiro ignora colliders tipo Trigger."
                );

            // Ammo
            case "useAmmo":
                return new GUIContent(
                    "Usar munición",
                    "Activa o desactiva el sistema de cargador y recarga."
                );

            case "clipSize":
                return new GUIContent(
                    "Tamaño del cargador",
                    "Cantidad máxima de disparos antes de recargar."
                );

            case "startWithFullAmmo":
                return new GUIContent(
                    "Iniciar con cargador lleno",
                    "Si está activo, el enemigo inicia con la munición llena al aplicar el arquetipo."
                );

            case "reloadDuration":
                return new GUIContent(
                    "Duración de recarga",
                    "Tiempo que tarda el enemigo en recargar."
                );

            case "autoReload":
                return new GUIContent(
                    "Recarga automática",
                    "Si está activo, el enemigo recarga automáticamente al quedarse sin munición."
                );

            default:
                return new GUIContent(ObjectNames.NicifyVariableName(propertyName));
        }
    }
}