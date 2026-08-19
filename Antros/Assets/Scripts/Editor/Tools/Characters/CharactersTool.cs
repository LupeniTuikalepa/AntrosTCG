using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Synty.SidekickCharacters;
using Synty.SidekickCharacters.Serialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.Characters
{
    /// <summary>
    /// Antros TCG Editor tab fronting Synty's Sidekick Character Tool, split into three sub-tabs:
    ///   • Explore  — lists saved .sk characters.
    ///   • Edit     — opens Synty's editor docked beside, shows a live in-tab 3D preview, and exports.
    ///   • Settings — the FlatKit base material used to re-shade exports, and the Explore scan folder.
    ///
    /// On export we drive Synty's own action (their private <c>CreateCharacterPrefab()</c>, reached by
    /// reflection) then re-shade the material it produces with a copy of the base material, moving the
    /// character's albedo into FlatKit's base map. Synty's UI can't be embedded — a VisualElement tree
    /// belongs to one window — so its editor is opened docked beside this hub instead.
    /// </summary>
    public sealed class CharactersTool : IEditorTool, IDisposable
    {
        // The name Synty gives the generated character root in the active scene.
        private const string OutputModelName = "Combined Character";
        private const string DefaultExploreFolder = "Assets/Project/Characters";

        public string DisplayName => "Characters";
        public string Icon => "☻";
        public int Order => 60;

        private enum SubTab { Explore, Edit, Settings }

        private VisualElement content;
        private readonly Dictionary<SubTab, VisualElement> views = new();
        private readonly Dictionary<SubTab, Button> tabButtons = new();
        private SubTab activeTab = SubTab.Edit;
        private bool toolActive;

        // ---- Edit / preview state ------------------------------------------
        private PreviewRenderUtility preview;
        private GameObject previewClone;
        private Vector3 pivot;
        private float distance = 4f;
        private Vector2 orbit = new(130f, -12f); // yaw, pitch (degrees)
        private bool dragging;
        private Vector2 lastPointer;

        private Image previewImage;
        private Label statusLabel;

        private IVisualElementScheduledItem pollItem;
        private bool autoRefresh = true;
        private int lastSignature;

        // ---- Explore state --------------------------------------------------
        private ScrollView exploreList;
        private readonly List<string> skPaths = new();
        private string exploreFilter = string.Empty;

        // ====================================================================
        // Tool lifecycle
        // ====================================================================

        public VisualElement BuildUI()
        {
            VisualElement root = new();
            root.AddToClassList("characters-root");
            EditorStyleLoader.Load(root, "EditorTheme.uss");
            EditorStyleLoader.Load(root, "Characters.uss");
            EditorStyleLoader.Load(root, "Cutscenes.uss"); // reuse the cutscene list/row styling

            root.Add(BuildSubTabBar());

            content = new VisualElement();
            content.AddToClassList("characters-content");
            root.Add(content);

            views[SubTab.Explore] = BuildExploreView();
            views[SubTab.Edit] = BuildEditView();
            views[SubTab.Settings] = BuildSettingsView();
            foreach (VisualElement v in views.Values)
                content.Add(v);

            // Poller lives on the preview element; the hub resumes it only while Edit is showing.
            pollItem = previewImage.schedule.Execute(Poll).Every(200);
            pollItem.Pause();

            ShowTab(SubTab.Edit);
            return root;
        }

        public void OnActivated()
        {
            toolActive = true;
            if (activeTab == SubTab.Edit)
            {
                RefreshPreview();
                pollItem?.Resume();
            }
        }

        public void OnDeactivated()
        {
            toolActive = false;
            pollItem?.Pause();
        }

        public void Dispose()
        {
            pollItem?.Pause();
            CleanupPreview();
        }

        // ====================================================================
        // Sub-tabs
        // ====================================================================

        private VisualElement BuildSubTabBar()
        {
            VisualElement bar = new();
            bar.AddToClassList("characters-subtabs");

            foreach (SubTab tab in Enum.GetValues(typeof(SubTab)).Cast<SubTab>())
            {
                SubTab captured = tab;
                Button b = new(() => ShowTab(captured)) { text = tab.ToString() };
                b.AddToClassList("characters-subtab");
                bar.Add(b);
                tabButtons[tab] = b;
            }
            return bar;
        }

        private void ShowTab(SubTab tab)
        {
            activeTab = tab;
            foreach (KeyValuePair<SubTab, VisualElement> kv in views)
                kv.Value.style.display = kv.Key == tab ? DisplayStyle.Flex : DisplayStyle.None;
            foreach (KeyValuePair<SubTab, Button> kv in tabButtons)
                kv.Value.EnableInClassList("characters-subtab--active", kv.Key == tab);

            if (tab == SubTab.Edit)
            {
                RefreshPreview();
                if (toolActive)
                    pollItem?.Resume();
            }
            else
            {
                pollItem?.Pause();
                if (tab == SubTab.Explore)
                    ReloadSkList();
            }
        }

        // ====================================================================
        // Edit sub-tab
        // ====================================================================

        private VisualElement BuildEditView()
        {
            VisualElement view = new();
            view.AddToClassList("characters-view");

            VisualElement bar = new();
            bar.AddToClassList("characters-bar");
            view.Add(bar);

            bar.Add(MakeButton("Open Sidekick Editor", () => OpenSyntyWindow(), false));
            bar.Add(MakeButton("Refresh Preview", RefreshPreview, false));

            Toggle auto = new("Auto") { value = autoRefresh };
            auto.AddToClassList("characters-auto");
            auto.RegisterValueChangedCallback(e => autoRefresh = e.newValue);
            bar.Add(auto);

            bar.Add(MakeButton("Save Character", () => InvokeSyntyAction("SaveCharacter", "sauvegarde"), false));
            bar.Add(MakeButton("Export (FlatKit)", ExportWithFlatKit, true));

            previewImage = new Image { scaleMode = ScaleMode.ScaleToFit };
            previewImage.AddToClassList("characters-preview");
            previewImage.RegisterCallback<GeometryChangedEvent>(_ => RenderPreview());
            previewImage.RegisterCallback<PointerDownEvent>(OnPointerDown);
            previewImage.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            previewImage.RegisterCallback<PointerUpEvent>(OnPointerUp);
            previewImage.RegisterCallback<WheelEvent>(OnWheel);
            view.Add(previewImage);

            statusLabel = new Label();
            statusLabel.AddToClassList("characters-status");
            view.Add(statusLabel);

            return view;
        }

        private static Button MakeButton(string text, Action action, bool primary)
        {
            Button b = new(action) { text = text };
            b.AddToClassList("characters-btn");
            if (primary)
                b.AddToClassList("characters-btn--primary");
            return b;
        }

        // ---- Synty window plumbing -----------------------------------------

        private static ModularCharacterWindow OpenSyntyWindow()
            => EditorWindow.GetWindow<ModularCharacterWindow>("Sidekick Character Tool", typeof(AntrosEditorWindow));

        // Runs one of Synty's own button actions on the live window instance and reports whether it ran.
        private bool InvokeSyntyAction(string methodName, string label)
        {
            if (GameObject.Find(OutputModelName) == null)
            {
                SetStatus($"Rien à {label} : génère d'abord un personnage dans l'éditeur Sidekick.");
                return false;
            }

            ModularCharacterWindow window = OpenSyntyWindow();
            MethodInfo method = typeof(ModularCharacterWindow).GetMethod(
                methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);

            if (method == null)
            {
                Debug.LogWarning($"[Characters] Action Synty '{methodName}' introuvable — le pack a peut-être changé.");
                SetStatus($"Action Synty '{methodName}' introuvable (pack mis à jour ?).");
                return false;
            }

            method.Invoke(window, null);
            SetStatus($"{label} déclenché via l'éditeur Sidekick.");
            return true;
        }

        // ---- Export + FlatKit re-shade -------------------------------------

        // Drives Synty's export, then swaps the shader on every material it just produced for a copy of
        // the FlatKit base material — carrying the character's albedo across into FlatKit's base map.
        private void ExportWithFlatKit()
        {
            HashSet<string> before = new(AssetDatabase.FindAssets("t:Material"));

            if (!InvokeSyntyAction("CreateCharacterPrefab", "export FBX"))
                return;

            AssetDatabase.Refresh();
            int reshaded = ReshadeNewMaterials(before);

            SetStatus(reshaded > 0
                ? $"Export terminé — {reshaded} matériau(x) repassé(s) sous FlatKit."
                : "Export terminé (aucun nouveau matériau, ou export annulé).");
        }

        private static int ReshadeNewMaterials(HashSet<string> beforeGuids)
        {
            Material baseMaterial = CharacterToolSettings.GetOrCreate().BaseMaterial;
            if (baseMaterial == null || baseMaterial.shader == null)
            {
                Debug.LogWarning("[Characters] Aucun Base Material FlatKit défini (onglet Settings) — export laissé tel quel.");
                return 0;
            }

            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Material"))
            {
                if (beforeGuids.Contains(guid))
                    continue;

                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material generated = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (generated == null)
                    continue;

                Texture albedo = ResolveAlbedo(generated);

                generated.shader = baseMaterial.shader;
                generated.CopyPropertiesFromMaterial(baseMaterial);
                if (albedo != null)
                    generated.mainTexture = albedo; // FlatKit _BaseMap is [MainTexture]

                EditorUtility.SetDirty(generated);
                count++;
            }

            if (count > 0)
                AssetDatabase.SaveAssets();
            return count;
        }

        // The character's albedo, however the source shader exposes it.
        private static Texture ResolveAlbedo(Material material)
        {
            if (material.mainTexture != null)
                return material.mainTexture;
            if (material.HasProperty("_BaseMap"))
                return material.GetTexture("_BaseMap");
            if (material.HasProperty("_MainTex"))
                return material.GetTexture("_MainTex");
            return null;
        }

        // ---- Preview --------------------------------------------------------

        private void RefreshPreview()
        {
            CleanupClone();

            GameObject source = GameObject.Find(OutputModelName);
            lastSignature = ComputeSignature(source);
            if (source == null)
            {
                SetStatus("Aucun personnage généré. Ouvre l'éditeur Sidekick et génère-en un.");
                if (previewImage != null)
                    previewImage.image = null;
                return;
            }

            EnsurePreview();

            previewClone = UnityEngine.Object.Instantiate(source);
            previewClone.hideFlags = HideFlags.HideAndDontSave;
            preview.AddSingleGO(previewClone);

            Bounds bounds = ComputeBounds(previewClone);
            pivot = bounds.center;
            distance = Mathf.Max(bounds.size.magnitude * 1.4f, 1f);

            SetStatus($"Aperçu de « {source.name} ». Glisse pour tourner, molette pour zoomer.");
            RenderPreview();
        }

        private void EnsurePreview()
        {
            if (preview != null)
                return;

            preview = new PreviewRenderUtility();
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.backgroundColor = new Color(0.14f, 0.15f, 0.17f, 1f);
            preview.camera.nearClipPlane = 0.01f;
            preview.camera.farClipPlane = 1000f;
            preview.ambientColor = new Color(0.32f, 0.33f, 0.35f, 1f);

            preview.lights[0].intensity = 1.2f;
            preview.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
            if (preview.lights.Length > 1)
            {
                preview.lights[1].intensity = 0.55f;
                preview.lights[1].transform.rotation = Quaternion.Euler(-20f, -140f, 0f);
            }
        }

        private void RenderPreview()
        {
            if (preview == null || previewClone == null || previewImage == null)
                return;

            Rect rect = previewImage.contentRect;
            if (rect.width < 8f || rect.height < 8f || float.IsNaN(rect.width))
                return;

            Quaternion rotation = Quaternion.Euler(orbit.y, orbit.x, 0f);
            Vector3 direction = rotation * Vector3.forward;
            preview.camera.transform.position = pivot - direction * distance;
            preview.camera.transform.rotation = rotation;

            preview.BeginPreview(rect, GUIStyle.none);
            preview.Render(true);
            Texture rendered = preview.EndPreview();

            previewImage.image = rendered;
            previewImage.MarkDirtyRepaint();
        }

        private void Poll()
        {
            if (!autoRefresh)
                return;

            GameObject source = GameObject.Find(OutputModelName);
            int signature = ComputeSignature(source);

            if (signature != lastSignature)
                RefreshPreview();
            else if (source != null)
                RenderPreview();
        }

        private static int ComputeSignature(GameObject source)
        {
            if (source == null)
                return 0;

            unchecked
            {
                int h = 17;
                h = h * 31 + source.GetHashCode();

                SkinnedMeshRenderer[] renderers = source.GetComponentsInChildren<SkinnedMeshRenderer>();
                h = h * 31 + renderers.Length;
                foreach (SkinnedMeshRenderer smr in renderers)
                {
                    Mesh mesh = smr.sharedMesh;
                    h = h * 31 + (mesh != null ? mesh.GetHashCode() : 0);

                    int blendCount = mesh != null ? mesh.blendShapeCount : 0;
                    for (int i = 0; i < blendCount; i++)
                        h = h * 31 + Mathf.RoundToInt(smr.GetBlendShapeWeight(i) * 10f);

                    foreach (Material mat in smr.sharedMaterials)
                        h = h * 31 + (mat != null ? mat.GetHashCode() : 0);
                }
                return h;
            }
        }

        private static Bounds ComputeBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(go.transform.position, Vector3.one);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private void OnPointerDown(PointerDownEvent e)
        {
            dragging = true;
            lastPointer = e.localPosition;
            previewImage.CapturePointer(e.pointerId);
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (!dragging)
                return;

            Vector2 delta = (Vector2)e.localPosition - lastPointer;
            lastPointer = e.localPosition;

            orbit.x += delta.x * 0.4f;
            orbit.y = Mathf.Clamp(orbit.y + delta.y * 0.4f, -89f, 89f);
            RenderPreview();
        }

        private void OnPointerUp(PointerUpEvent e)
        {
            dragging = false;
            if (previewImage.HasPointerCapture(e.pointerId))
                previewImage.ReleasePointer(e.pointerId);
        }

        private void OnWheel(WheelEvent e)
        {
            distance = Mathf.Clamp(distance * (1f + e.delta.y * 0.05f), 0.5f, 50f);
            RenderPreview();
            e.StopPropagation();
        }

        private void CleanupClone()
        {
            if (previewClone != null)
            {
                UnityEngine.Object.DestroyImmediate(previewClone);
                previewClone = null;
            }
        }

        private void CleanupPreview()
        {
            CleanupClone();
            if (preview != null)
            {
                preview.Cleanup();
                preview = null;
            }
        }

        private void SetStatus(string message)
        {
            if (statusLabel != null)
                statusLabel.text = message;
        }

        // ====================================================================
        // Explore sub-tab
        // ====================================================================

        private VisualElement BuildExploreView()
        {
            VisualElement view = new();
            view.AddToClassList("cutscene-tab"); // same shell as the cutscenes list

            Toolbar bar = new();
            bar.Add(new ToolbarButton(ReloadSkList) { text = "Refresh" });

            ToolbarSearchField search = new();
            search.AddToClassList("cutscene-search");
            search.value = exploreFilter;
            search.RegisterValueChangedCallback(e =>
            {
                exploreFilter = e.newValue ?? string.Empty;
                RebuildSkRows();
            });
            bar.Add(search);
            view.Add(bar);

            exploreList = new ScrollView(ScrollViewMode.Vertical);
            exploreList.AddToClassList("cutscene-list");
            view.Add(exploreList);

            return view;
        }

        // Loads a .sk into Synty and switches to Edit — as if it had been loaded from their own window.
        // The live poll then picks up the regenerated character for the preview.
        private void EditSk(string path)
        {
            ModularCharacterWindow window = OpenSyntyWindow();
            ShowTab(SubTab.Edit);
            LoadSkWhenReady(window, path, 0);
        }

        // Synty's editor initialises asynchronously (async CreateGUI + background data load), so we wait
        // for its runtime + database to exist before applying the character, then reuse its own
        // LoadSerializedCharacter so the load is identical to using their window.
        private void LoadSkWhenReady(ModularCharacterWindow window, string path, int attempt)
        {
            bool ready = GetPrivateField(window, "_sidekickRuntime") != null
                         && GetPrivateField(window, "_dbManager") != null;

            if (!ready)
            {
                if (attempt > 300)
                {
                    SetStatus("L'éditeur Sidekick n'a pas fini de s'initialiser — réessaie.");
                    return;
                }
                EditorApplication.delayCall += () => LoadSkWhenReady(window, path, attempt + 1);
                return;
            }

            try
            {
                SerializedCharacter character = DeserializeSk(path);
                if (character == null)
                {
                    SetStatus("Impossible de désérialiser ce .sk.");
                    return;
                }

                // Mirror the flags LoadCharacter sets around the apply, when those fields still exist.
                SetPrivateField(window, "_loadingCharacter", true);
                SetPrivateField(window, "_showAllColourProperties", true);

                MethodInfo load = typeof(ModularCharacterWindow).GetMethod(
                    "LoadSerializedCharacter", BindingFlags.NonPublic | BindingFlags.Instance);
                if (load == null)
                {
                    SetStatus("Méthode Synty 'LoadSerializedCharacter' introuvable (pack mis à jour ?).");
                    return;
                }

                load.Invoke(window, new object[] { character, true });
                SetPrivateField(window, "_loadingCharacter", false);

                SetStatus($"Chargé « {Path.GetFileNameWithoutExtension(path)} » dans Sidekick — la preview va se mettre à jour.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Characters] Chargement du .sk échoué : {e.Message}");
                SetStatus("Chargement du .sk échoué (voir Console).");
            }
        }

        // Deserializes a .sk (Synty's YAML format) into a SerializedCharacter. The YAML deserializer
        // lives in the VisualScripting package; we reach it by reflection to avoid a compile-time
        // dependency on that assembly.
        private static SerializedCharacter DeserializeSk(string path)
        {
            string data = Encoding.ASCII.GetString(File.ReadAllBytes(path));

            Type deserializerType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Unity.VisualScripting.YamlDotNet.Serialization.Deserializer"))
                .FirstOrDefault(t => t != null);
            if (deserializerType == null)
                throw new InvalidOperationException("Désérialiseur YAML (VisualScripting) introuvable.");

            object deserializer = Activator.CreateInstance(deserializerType);
            MethodInfo deserialize = deserializerType.GetMethods()
                .First(m => m.Name == "Deserialize"
                            && m.IsGenericMethodDefinition
                            && m.GetParameters().Length == 1
                            && m.GetParameters()[0].ParameterType == typeof(string))
                .MakeGenericMethod(typeof(SerializedCharacter));

            return (SerializedCharacter)deserialize.Invoke(deserializer, new object[] { data });
        }

        private static object GetPrivateField(object target, string name)
            => target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(target);

        private static void SetPrivateField(object target, string name, object value)
            => target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(target, value);

        private void ReloadSkList()
        {
            skPaths.Clear();

            string root = ExploreRoot();
            if (AssetDatabase.IsValidFolder(root) && Directory.Exists(root))
            {
                foreach (string path in Directory.GetFiles(root, "*.sk", SearchOption.AllDirectories))
                    skPaths.Add(path.Replace('\\', '/'));
                skPaths.Sort(StringComparer.OrdinalIgnoreCase);
            }

            RebuildSkRows();
        }

        // Rebuilds the rows (filtered + grouped by sub-folder), mirroring the cutscenes list.
        private void RebuildSkRows()
        {
            if (exploreList == null)
                return;

            exploreList.Clear();
            string root = ExploreRoot();

            IEnumerable<string> shown = skPaths;
            if (!string.IsNullOrWhiteSpace(exploreFilter))
            {
                string needle = exploreFilter.Trim();
                shown = skPaths.Where(p =>
                    Path.GetFileNameWithoutExtension(p).IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            List<string> visible = shown.ToList();
            if (visible.Count == 0)
            {
                Label empty = new(skPaths.Count == 0
                    ? $"Aucun .sk sous {root}."
                    : $"Aucun personnage ne correspond à « {exploreFilter} ».");
                empty.AddToClassList("cutscene-empty");
                exploreList.Add(empty);
                return;
            }

            foreach (IGrouping<string, string> group in visible
                         .GroupBy(p => GroupLabel(p, root))
                         .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
            {
                Label header = new(group.Key);
                header.AddToClassList("cutscene-group-header");
                exploreList.Add(header);

                foreach (string path in group.OrderBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase))
                    exploreList.Add(BuildSkRow(path));
            }
        }

        private VisualElement BuildSkRow(string path)
        {
            VisualElement row = new();
            row.AddToClassList("cutscene-row");

            Label name = new(Path.GetFileNameWithoutExtension(path)) { tooltip = path };
            name.AddToClassList("cutscene-row-name");
            row.Add(name);

            Button edit = new(() => EditSk(path)) { text = "Edit" };
            edit.AddToClassList("cutscene-row-edit");
            row.Add(edit);

            Button ping = new(() => PingSk(path)) { text = "Ping" };
            ping.AddToClassList("cutscene-row-ping");
            row.Add(ping);

            return row;
        }

        // First sub-folder under the explore root (e.g. "Chizus"), used as the group header. Characters
        // deeper down still fall under their top-level folder rather than each getting their own group.
        private static string GroupLabel(string path, string root)
        {
            string dir = (Path.GetDirectoryName(path) ?? string.Empty).Replace('\\', '/');
            if (dir.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                dir = dir[root.Length..].Trim('/');

            if (string.IsNullOrEmpty(dir))
                return "(racine)";

            int slash = dir.IndexOf('/');
            return slash >= 0 ? dir[..slash] : dir;
        }

        private static string ExploreRoot()
        {
            DefaultAsset folder = CharacterToolSettings.GetOrCreate().ExploreFolder;
            string path = folder != null ? AssetDatabase.GetAssetPath(folder) : null;
            if (!string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path))
                return path;
            return DefaultExploreFolder;
        }

        private static void PingSk(string path)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset != null)
                EditorGUIUtility.PingObject(asset);
        }

        // ====================================================================
        // Settings sub-tab
        // ====================================================================

        private VisualElement BuildSettingsView()
        {
            VisualElement view = new();
            view.AddToClassList("characters-view");
            view.AddToClassList("characters-settings");

            CharacterToolSettings settings = CharacterToolSettings.GetOrCreate();

            ObjectField baseMaterial = new("Base Material (FlatKit)")
            {
                objectType = typeof(Material),
                value = settings.BaseMaterial,
                tooltip = "Une copie de ce matériau remplace le matériau généré à l'export ; l'albedo du perso va dans sa base map."
            };
            baseMaterial.AddToClassList("characters-settings-field");
            baseMaterial.RegisterValueChangedCallback(e => settings.BaseMaterial = e.newValue as Material);
            view.Add(baseMaterial);

            ObjectField exploreFolder = new("Explore Folder")
            {
                objectType = typeof(DefaultAsset),
                value = settings.ExploreFolder,
                tooltip = "Dossier scanné par l'onglet Explore pour les .sk (défaut : Assets/Project/Characters)."
            };
            exploreFolder.AddToClassList("characters-settings-field");
            exploreFolder.RegisterValueChangedCallback(e => settings.ExploreFolder = e.newValue as DefaultAsset);
            view.Add(exploreFolder);

            Label hint = new(
                "À l'export, le shader du matériau créé par Synty est remplacé par celui du Base Material, " +
                "en conservant la texture du personnage dans la base map.");
            hint.AddToClassList("characters-status");
            view.Add(hint);

            return view;
        }
    }
}
