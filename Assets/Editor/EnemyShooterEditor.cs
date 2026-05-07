using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyShooter))]
[CanEditMultipleObjects]
public class EnemyShooterEditor : Editor
{
    static bool foldMode = true;
    static bool foldCommon = true;
    static bool foldProjectile = true;
    static bool foldRaycast = true;
    static bool foldSFX = true;
    static bool foldAim = true;
    static bool foldLineOfFire = true;
    static bool foldAmmo = true;
    static bool foldRefs = true;
    static bool foldDebug = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty fireMode = serializedObject.FindProperty("fireMode");

        DrawFold("Modo de disparo", ref foldMode,
            "fireMode"
        );

        DrawFold("Configuración común", ref foldCommon,
            "firePoint",
            "cooldownSeconds",
            "fireRange",
            "spawnOffset"
        );

        if (ShouldShowMode(fireMode, "Projectile"))
        {
            DrawFold("Disparo con proyectil", ref foldProjectile,
                "bulletPool",
                "bulletSettings"
            );
        }

        if (ShouldShowMode(fireMode, "Raycast"))
        {
            DrawFold("Disparo por Raycast", ref foldRaycast,
                "raycastMask",
                "raycastDamage",
                "raycastIgnoreTriggers"
            );
        }

        DrawFold("Sonido", ref foldSFX,
            "shootSound"
        );

        DrawFold("Apuntado", ref foldAim,
            "aimOrigin",
            "muzzlePoint",
            "targetHeightOffset"
        );

        DrawFold("Línea de tiro", ref foldLineOfFire,
            "lineOfFireMask",
            "ignoreTriggersInLineOfFire"
        );

        DrawFold("Munición y recarga", ref foldAmmo,
            "useAmmo",
            "clipSize",
            "currentAmmo",
            "reloadDuration",
            "autoReload",
            "isReloading"
        );

        DrawFold("Referencias opcionales", ref foldRefs,
            "enemyAnimator"
        );

        DrawFold("Depuración", ref foldDebug,
            "debugDraw"
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
            case "fireMode":
                DrawFireMode(sp);
                break;

            case "isReloading":
                DrawReadOnlyProperty(sp, Label(propertyName));
                break;

            default:
                EditorGUILayout.PropertyField(sp, Label(propertyName), true);
                break;
        }
    }

    void DrawFireMode(SerializedProperty sp)
    {
        if (sp.propertyType != SerializedPropertyType.Enum)
        {
            EditorGUILayout.PropertyField(sp, Label("fireMode"), true);
            return;
        }

        string[] translatedOptions = new string[sp.enumDisplayNames.Length];

        for (int i = 0; i < translatedOptions.Length; i++)
        {
            string enumName = sp.enumNames[i];

            switch (enumName)
            {
                case "Projectile":
                    translatedOptions[i] = "Proyectil";
                    break;

                case "Raycast":
                    translatedOptions[i] = "Raycast";
                    break;

                default:
                    translatedOptions[i] = ObjectNames.NicifyVariableName(enumName);
                    break;
            }
        }

        EditorGUI.showMixedValue = sp.hasMultipleDifferentValues;

        EditorGUI.BeginChangeCheck();

        int newIndex = EditorGUILayout.Popup(Label("fireMode"), sp.enumValueIndex, translatedOptions);

        if (EditorGUI.EndChangeCheck())
        {
            sp.enumValueIndex = newIndex;
        }

        EditorGUI.showMixedValue = false;
    }

    void DrawReadOnlyProperty(SerializedProperty sp, GUIContent label)
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(sp, label, true);
        EditorGUI.EndDisabledGroup();
    }

    bool ShouldShowMode(SerializedProperty fireMode, string expectedMode)
    {
        if (fireMode == null)
            return true;

        if (fireMode.propertyType != SerializedPropertyType.Enum)
            return true;

        if (fireMode.hasMultipleDifferentValues)
            return true;

        if (fireMode.enumValueIndex < 0 || fireMode.enumValueIndex >= fireMode.enumNames.Length)
            return true;

        string currentMode = fireMode.enumNames[fireMode.enumValueIndex];
        return currentMode == expectedMode;
    }

    GUIContent Label(string propertyName)
    {
        switch (propertyName)
        {
            // Modo de disparo
            case "fireMode":
                return new GUIContent(
                    "Tipo de disparo",
                    "Define si el enemigo dispara con proyectiles o mediante Raycast."
                );

            // Configuración común
            case "firePoint":
                return new GUIContent(
                    "Punto de disparo",
                    "Transform desde donde se ejecuta el disparo base."
                );

            case "cooldownSeconds":
                return new GUIContent(
                    "Tiempo entre disparos",
                    "Tiempo mínimo que debe pasar entre un disparo y el siguiente."
                );

            case "fireRange":
                return new GUIContent(
                    "Rango de disparo",
                    "Distancia máxima a la que el enemigo puede disparar."
                );

            case "spawnOffset":
                return new GUIContent(
                    "Desplazamiento de aparición",
                    "Pequeño desplazamiento usado al crear el proyectil."
                );

            // Proyectil
            case "bulletPool":
                return new GUIContent(
                    "Pool de balas",
                    "Sistema que reutiliza proyectiles para evitar crearlos y destruirlos constantemente."
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

            // SFX
            case "shootSound":
                return new GUIContent(
                    "Sonido de disparo",
                    "Sonido reproducido cuando el enemigo dispara."
                );

            // Apuntado
            case "aimOrigin":
                return new GUIContent(
                    "Origen de apuntado",
                    "Punto desde donde el enemigo calcula la dirección hacia el objetivo."
                );

            case "muzzlePoint":
                return new GUIContent(
                    "Punta del arma",
                    "Punto desde donde aparecen los proyectiles."
                );

            case "targetHeightOffset":
                return new GUIContent(
                    "Altura del objetivo",
                    "Altura aproximada a la que el enemigo intenta apuntar, por ejemplo pecho o cabeza."
                );

            // Línea de tiro
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

            // Munición / recarga
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

            case "currentAmmo":
                return new GUIContent(
                    "Munición actual",
                    "Cantidad actual de disparos disponibles en el cargador."
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

            case "isReloading":
                return new GUIContent(
                    "Recargando",
                    "Indica si el enemigo está recargando actualmente. Solo lectura."
                );

            // Referencias
            case "enemyAnimator":
                return new GUIContent(
                    "Animador del enemigo",
                    "Referencia opcional al componente encargado de reproducir animaciones."
                );

            // Debug
            case "debugDraw":
                return new GUIContent(
                    "Mostrar depuración de disparo",
                    "Dibuja líneas de ayuda para revisar la dirección y validación del disparo."
                );

            default:
                return new GUIContent(ObjectNames.NicifyVariableName(propertyName));
        }
    }
}