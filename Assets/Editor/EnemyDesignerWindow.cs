using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using System.Collections.Generic;

public class EnemyDesignerWindow : EditorWindow {
    GUIStyle h1, h2, help;

    bool foldGlobal = true;
    bool foldSpawn = true;
    bool foldSquad = true;
    bool foldExtras = true;
    bool foldTips = false;

    GameObject enemyPrefab;
    EnemyArchetype defaultArchetype;
    GameObject enemyRootParent;

    enum ForceRole { AutoFromArchetype, Grunt, Elite }
    ForceRole forceRole = ForceRole.AutoFromArchetype;

    int spawnCount = 6;
    float spawnRadius = 10f;
    bool snapToGround = true;
    LayerMask groundMask = ~0;
    float groundRayHeight = 40f;
    bool randomYaw = true;

    bool assignToSquad = true;
    SquadGroup squadToUse;

    bool selectAfterCreate = true;
    bool pingSelection = true;

    bool showScenePreview = true;
    Color previewColor = new Color(0f, 1f, 0.5f, 0.25f);

    // NUEVO: opciones de componentes extra
    RuntimeAnimatorController animatorController;
    bool autoAddHealth = true;
    bool autoAddHealthBar = true;

    struct QuickPreset {
        public string name;
        public System.Action<EnemyArchetype, EnemyDesignerWindow> apply;
    }
    List<QuickPreset> presets;

    [MenuItem("Tools/Shooter AI/Enemy Designer Pro")]
    public static void Open() {
        var w = GetWindow<EnemyDesignerWindow>("Enemy Designer Pro");
        w.minSize = new Vector2(460, 560);
    }

    void OnEnable() {
        BuildStyles();
        BuildPresets();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable() {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void BuildStyles() {
        if (h1 != null) return;          // por si lo llamamos varias veces

        h1 = new GUIStyle(EditorStyles.label);
        h1.fontSize = 14;
        h1.fontStyle = FontStyle.Bold;

        h2 = new GUIStyle(EditorStyles.label);
        h2.fontSize = 12;
        h2.fontStyle = FontStyle.Bold;

        help = new GUIStyle(EditorStyles.helpBox);
        help.wordWrap = true;
    }

    void BuildPresets() {
        presets = new List<QuickPreset>() {
            new QuickPreset {
                name = "Grunt (rápido, visión media)",
                apply = (arch, w) => {
                    w.spawnCount = 6;
                    w.spawnRadius = 8f;
                    w.forceRole = ForceRole.Grunt;
                    w.randomYaw = true;
                }
            },
            new QuickPreset {
                name = "Elite (pocos, rango mayor)",
                apply = (arch, w) => {
                    w.spawnCount = 2;
                    w.spawnRadius = 6f;
                    w.forceRole = ForceRole.Elite;
                    w.randomYaw = false;
                }
            }
        };
    }

    void OnGUI() {
        if (h1 == null) BuildStyles();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Enemy Designer Pro", h1);
        EditorGUILayout.LabelField("Crea enemigos con presets, vista previa y squad.", help);

        DrawPresetsToolbar();

        EditorGUILayout.Space(6);
        DrawGlobal();
        DrawSpawn();
        DrawSquad();
        DrawExtras();
        DrawActions();
        DrawTips();
    }

    void DrawPresetsToolbar() {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Presets rápidos:", h2);
        foreach (var p in presets) {
            if (GUILayout.Button(p.name, GUILayout.Height(22))) {
                p.apply(defaultArchetype, this);
                Repaint();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawGlobal() {
        EditorGUILayout.Space(6);
        foldGlobal = EditorGUILayout.BeginFoldoutHeaderGroup(foldGlobal, "Opciones Globales");
        if (foldGlobal) {
            EditorGUILayout.Space(4);
            enemyPrefab = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("Prefab base", "Prefab visual que se va a instanciar."),
                enemyPrefab, typeof(GameObject), false);

            defaultArchetype = (EnemyArchetype)EditorGUILayout.ObjectField(
                new GUIContent("Arquetipo (opcional)", "EnemyArchetype a aplicar tras crear."),
                defaultArchetype, typeof(EnemyArchetype), false);

            enemyRootParent = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("Parent en jerarquía", "Opcional: carpeta en la jerarquía."),
                enemyRootParent, typeof(GameObject), true);

            forceRole = (ForceRole)EditorGUILayout.EnumPopup(
                new GUIContent("Forzar Rol", "Auto (SO) o fijar Grunt/Elite."),
                forceRole);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    void DrawSpawn() {
        EditorGUILayout.Space(4);
        foldSpawn = EditorGUILayout.BeginFoldoutHeaderGroup(foldSpawn, "Spawn / Distribución");
        if (foldSpawn) {
            spawnCount = EditorGUILayout.IntSlider(new GUIContent("Cantidad"), spawnCount, 1, 100);
            spawnRadius = EditorGUILayout.Slider(new GUIContent("Radio"), spawnRadius, 0.1f, 100f);
            randomYaw = EditorGUILayout.ToggleLeft("Rotación aleatoria Y", randomYaw);

            EditorGUILayout.Space(4);
            showScenePreview = EditorGUILayout.ToggleLeft("Preview en SceneView", showScenePreview);
            previewColor = EditorGUILayout.ColorField("Color preview", previewColor);

            EditorGUILayout.Space(4);
            snapToGround = EditorGUILayout.Toggle(new GUIContent("Ajustar al suelo (raycast)"), snapToGround);
            using (new EditorGUI.DisabledScope(!snapToGround)) {
                groundMask = LayerMaskField("Ground Mask", groundMask);
                groundRayHeight = EditorGUILayout.Slider("Altura Raycast", groundRayHeight, 1f, 200f);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    void DrawSquad() {
        EditorGUILayout.Space(4);
        foldSquad = EditorGUILayout.BeginFoldoutHeaderGroup(foldSquad, "Escuadra (opcional)");
        if (foldSquad) {
            assignToSquad = EditorGUILayout.ToggleLeft("Asignar a SquadGroup", assignToSquad);
            using (new EditorGUI.DisabledScope(!assignToSquad)) {
                squadToUse = (SquadGroup)EditorGUILayout.ObjectField(
                    new GUIContent("SquadGroup destino"),
                    squadToUse, typeof(SquadGroup), true);

                if (GUILayout.Button("Crear SquadGroup")) {
                    squadToUse = CreateSquadGroup(enemyRootParent ? enemyRootParent.transform : null);
                    Ping(squadToUse ? squadToUse.gameObject : null);
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    void DrawExtras() {
        EditorGUILayout.Space(4);
        foldExtras = EditorGUILayout.BeginFoldoutHeaderGroup(foldExtras, "Extras al crear enemigo");
        if (foldExtras) {
            EditorGUILayout.LabelField("Animator", h2);
            animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField(
                new GUIContent("Animator Controller", "Se asigna al componente Animator del enemigo."),
                animatorController, typeof(RuntimeAnimatorController), false);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Salud", h2);
            autoAddHealth = EditorGUILayout.ToggleLeft("Añadir Health si no existe", autoAddHealth);
            autoAddHealthBar = EditorGUILayout.ToggleLeft("Añadir HealthBarWorld si no existe", autoAddHealthBar);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    void DrawActions() {
        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Acciones", h2);

        selectAfterCreate = EditorGUILayout.ToggleLeft("Seleccionar creados", selectAfterCreate);
        pingSelection = EditorGUILayout.ToggleLeft("Ping selección", pingSelection);

        EditorGUILayout.Space(6);
        using (new EditorGUI.DisabledScope(enemyPrefab == null)) {
            if (GUILayout.Button($"Crear {spawnCount} Enemigos", GUILayout.Height(28)))
                CreateEnemies();
        }
        EditorGUILayout.EndVertical();
    }

    void DrawTips() {
        EditorGUILayout.Space(4);
        foldTips = EditorGUILayout.BeginFoldoutHeaderGroup(foldTips, "Tips & Atajos");
        if (foldTips) {
            EditorGUILayout.LabelField(
                "• Usa un prefab base con colliders, Rigidbody y Canvas de vida.\n" +
                "• El Designer añade EnemyManager, Shooter, Animator script, etc.\n" +
                "• El Animator Controller se asigna automáticamente si lo eliges.\n" +
                "• Puedes crear un SquadGroup desde aquí mismo.", help);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    void OnSceneGUI(SceneView sv) {
        if (!showScenePreview) return;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        Vector3 c = GetSceneCenterOnGroundOrZero();
        Handles.color = previewColor;
        Handles.DrawSolidDisc(c, Vector3.up, spawnRadius);
        Handles.color = new Color(previewColor.r, previewColor.g, previewColor.b, 1f);
        Handles.DrawWireDisc(c, Vector3.up, spawnRadius);

        Handles.BeginGUI();
        var r = new Rect(12, 12, 260, 38);
        GUI.Box(r, GUIContent.none);
        GUI.Label(r, $"Enemy Designer Preview\ncenter: {c}  radius: {spawnRadius:0.0}");
        Handles.EndGUI();
    }

    void CreateEnemies() {
        Transform parent = enemyRootParent ? enemyRootParent.transform : null;
        if (!parent) {
            var container = new GameObject("Enemies");
            Undo.RegisterCreatedObjectUndo(container, "Create Enemies Root");
            parent = container.transform;
        }

        SquadGroup squad = null;
        if (assignToSquad)
            squad = squadToUse ? squadToUse : CreateSquadGroup(parent);

        Vector3 center = GetSceneCenterOnGroundOrZero();
        var list = new List<GameObject>();

        for (int i = 0; i < spawnCount; i++) {
            Vector3 pos = center + Random.insideUnitSphere * spawnRadius;
            pos.y = center.y + groundRayHeight;
            if (snapToGround) pos = SnapDown(pos, center.y, out _);

            var go = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
            if (!go) go = Instantiate(enemyPrefab);

            Undo.RegisterCreatedObjectUndo(go, "Create Enemy");
            go.name = $"{enemyPrefab.name}_{i + 1:00}";
            go.transform.SetParent(parent, true);
            go.transform.position = pos;
            if (randomYaw) go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            var mgr = EnsureEnemyMinimalSetup(go);

            if (mgr) {
                if (forceRole != ForceRole.AutoFromArchetype)
                    mgr.role = (forceRole == ForceRole.Elite) ? EnemyRole.Elite : EnemyRole.Grunt;

                if (defaultArchetype) mgr.ApplyArchetype(defaultArchetype);
                if (assignToSquad && squad) mgr.squadGroup = squad;
            }

            list.Add(go);
        }

        if (selectAfterCreate) Selection.objects = list.ToArray();
        if (pingSelection && list.Count > 0) EditorGUIUtility.PingObject(list[0]);
        Debug.Log($"[Enemy Designer] Creados {list.Count} enemigos.");
    }

    EnemyManager EnsureEnemyMinimalSetup(GameObject go) {
        var mgr = go.GetComponent<EnemyManager>();
        if (!mgr) mgr = Undo.AddComponent<EnemyManager>(go);

        // Animator base
        var animator = go.GetComponent<Animator>();
        if (!animator) animator = Undo.AddComponent<Animator>(go);
        if (animatorController) animator.runtimeAnimatorController = animatorController;

        if (!go.GetComponent<EnemyShooter>()) Undo.AddComponent<EnemyShooter>(go);
        if (!go.GetComponent<EnemyAnimator>()) Undo.AddComponent<EnemyAnimator>(go);
        if (!go.GetComponent<Unit>()) Undo.AddComponent<Unit>(go);

        if (autoAddHealth && !go.GetComponent<Health>()) Undo.AddComponent<Health>(go);
        if (autoAddHealthBar && !go.GetComponent<HealthBarWorld>()) Undo.AddComponent<HealthBarWorld>(go);

        var cap = go.GetComponent<CapsuleCollider>();
        if (!cap) {
            cap = Undo.AddComponent<CapsuleCollider>(go);
            cap.center = new Vector3(0, 1f, 0);
            cap.height = 2f;
            cap.radius = 0.35f;
        }

        var rb = go.GetComponent<Rigidbody>();
        if (!rb) {
            rb = Undo.AddComponent<Rigidbody>(go);
            rb.useGravity = true;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        EditorUtility.SetDirty(go);
        mgr.ForceValidateForDesigner();
        return mgr;
    }

    SquadGroup CreateSquadGroup(Transform parent = null) {
        var go = new GameObject("SquadGroup");
        Undo.RegisterCreatedObjectUndo(go, "Create SquadGroup");
        if (parent) go.transform.SetParent(parent, true);
        var sg = go.AddComponent<SquadGroup>();
        return sg;
    }

    Vector3 GetSceneCenterOnGroundOrZero() {
        var sv = SceneView.lastActiveSceneView;
        if (sv && sv.camera) {
            var cam = sv.camera;
            var ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out var hit, 1000f, groundMask))
                return hit.point;
            return cam.transform.position + cam.transform.forward * 10f;
        }
        return Vector3.zero;
    }

    Vector3 SnapDown(Vector3 start, float minY, out bool ok) {
        var from = start;
        if (Physics.Raycast(from, Vector3.down, out var hit, groundRayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore)) {
            ok = true; return hit.point;
        }
        ok = false; start.y = Mathf.Max(minY, start.y - groundRayHeight);
        return start;
    }

    static LayerMask LayerMaskField(string label, LayerMask selected) {
        var names = GetLayerNames();
        int maskNoEmpty = 0;
        for (int i = 0; i < names.Length; i++) {
            int layer = LayerMask.NameToLayer(names[i]);
            if (((selected.value >> layer) & 1) == 1) maskNoEmpty |= (1 << i);
        }
        maskNoEmpty = EditorGUILayout.MaskField(label, maskNoEmpty, names);
        int mask = 0;
        for (int i = 0; i < names.Length; i++) {
            if ((maskNoEmpty & (1 << i)) != 0) {
                int layer = LayerMask.NameToLayer(names[i]);
                mask |= (1 << layer);
            }
        }
        selected.value = mask; return selected;
    }

    static string[] GetLayerNames() {
        var list = new List<string>();
        for (int i = 0; i < 32; i++) {
            var n = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(n)) list.Add(n);
        }
        if (list.Count == 0) list.Add("Default");
        return list.ToArray();
    }

    void Ping(Object o) {
        if (!o) return;
        EditorGUIUtility.PingObject(o);
        Selection.activeObject = o;
    }
}
