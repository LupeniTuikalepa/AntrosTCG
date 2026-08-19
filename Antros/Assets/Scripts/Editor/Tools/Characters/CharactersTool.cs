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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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

        private enum SubTab { Browse, Settings }

        private VisualElement content;
        private readonly Dictionary<SubTab, VisualElement> views = new();
        private readonly Dictionary<SubTab, Button> tabButtons = new();
        private SubTab activeTab = SubTab.Browse;
        private bool toolActive;

        // ---- Edit / preview state ------------------------------------------
        private PreviewRenderUtility preview;
        private GameObject previewClone;
        private Vector3 framePivot;       // character center (F reset target)
        private float framingDistance = 4f; // distance that fits the whole character
        private float distance = 4f;
        private Vector2 orbit = new(130f, -12f); // camera yaw, pitch (degrees)
        private Vector2 pan;              // arrow-key pan offset, in camera right/up
        private float maxPan = 2f;        // clamp so panning can't drift off the character
        private Vector2 light = new(35f, 35f); // light yaw, pitch (degrees)
        private bool dragging;
        private int activeButton; // 0 = left (orbit), 2 = middle (pan), 1 = right (light)
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
        private string selectedSkPath;

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

            views[SubTab.Browse] = BuildBrowseView();
            views[SubTab.Settings] = BuildSettingsView();
            foreach (VisualElement v in views.Values)
                content.Add(v);

            // Poller lives on the preview element; the hub resumes it only while Browse is showing.
            pollItem = previewImage.schedule.Execute(Poll).Every(200);
            pollItem.Pause();

            ShowTab(SubTab.Browse);
            return root;
        }

        public void OnActivated()
        {
            toolActive = true;
            if (activeTab == SubTab.Browse)
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

            if (tab == SubTab.Browse)
            {
                ReloadSkList();
                RefreshPreview();
                if (toolActive)
                    pollItem?.Resume();
            }
            else
            {
                pollItem?.Pause();
            }
        }

        // ====================================================================
        // Browse sub-tab: explorer (left) + inspector (right)
        // ====================================================================

        private VisualElement BuildBrowseView()
        {
            TwoPaneSplitView split = new(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            split.AddToClassList("characters-view");
            split.Add(BuildExplorer());
            split.Add(BuildInspector());
            return split;
        }

        // Right pane: live preview + edition buttons for the currently loaded character.
        private VisualElement BuildInspector()
        {
            VisualElement view = new();
            view.AddToClassList("characters-view");

            Toolbar bar = new();
            bar.Add(new ToolbarButton(() => OpenSyntyWindow()) { text = "Open Sidekick" });
            bar.Add(new ToolbarButton(RefreshPreview) { text = "Refresh" });
            bar.Add(new ToolbarButton(() => InvokeSyntyAction("SaveCharacter", "sauvegarde")) { text = "Save Character" });
            bar.Add(new ToolbarButton(ExportWithFlatKit) { text = "Export (FlatKit)" });

            ToolbarToggle auto = new() { text = "Auto", value = autoRefresh };
            auto.RegisterValueChangedCallback(e => autoRefresh = e.newValue);
            bar.Add(auto);
            view.Add(bar);

            previewImage = new Image { scaleMode = ScaleMode.ScaleToFit, focusable = true };
            previewImage.AddToClassList("characters-preview");
            previewImage.RegisterCallback<GeometryChangedEvent>(_ => RenderPreview());
            previewImage.RegisterCallback<PointerDownEvent>(OnPointerDown);
            previewImage.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            previewImage.RegisterCallback<PointerUpEvent>(OnPointerUp);
            previewImage.RegisterCallback<WheelEvent>(OnWheel);
            previewImage.RegisterCallback<KeyDownEvent>(OnKeyDown);
            previewImage.RegisterCallback<ContextClickEvent>(e => e.StopPropagation()); // no menu on right-drag
            view.Add(previewImage);

            Label hint = new("Gauche : tourner • molette : zoom • clic molette : déplacer • clic droit : lumière • F : recentrer");
            hint.AddToClassList("characters-status");
            view.Add(hint);

            statusLabel = new Label();
            statusLabel.AddToClassList("characters-status");
            view.Add(statusLabel);

            return view;
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
            framePivot = bounds.center;
            float radius = Mathf.Max(bounds.extents.magnitude, 0.1f);
            // Distance that fits the whole bounding sphere in the camera's FOV, with margin — so the
            // initial view is wide enough to see the entire character.
            framingDistance = radius / Mathf.Sin(Mathf.Deg2Rad * preview.camera.fieldOfView * 0.5f) * 1.15f;
            maxPan = radius * 0.9f;
            distance = framingDistance;
            pan = Vector2.zero;

            SetStatus($"Aperçu de « {source.name} ».");
            RenderPreview();
        }

        private void EnsurePreview()
        {
            if (preview != null)
                return;

            preview = new PreviewRenderUtility();
            Camera cam = preview.camera;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.14f, 0.15f, 0.17f, 1f);
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 1000f;
            cam.fieldOfView = 30f;
            preview.ambientColor = new Color(0.32f, 0.33f, 0.35f, 1f);

            preview.lights[0].intensity = 1.2f; // key light — rotated live by the Light slider
            if (preview.lights.Length > 1)
            {
                preview.lights[1].intensity = 0.55f; // fixed fill
                preview.lights[1].transform.rotation = Quaternion.Euler(-20f, -140f, 0f);
            }

            EnablePostProcessing(cam);
            InjectGlobalVolume();
        }

        // Turns on URP post-processing on the preview camera and lets it see all volume layers, so the
        // character previews with the game's global post FX.
        private static void EnablePostProcessing(Camera cam)
        {
            UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
            if (data == null)
                return;

            data.renderPostProcessing = true;
            data.volumeLayerMask = ~0;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        }

        // Copies the project's highest-priority global Volume into the preview scene so its post FX
        // apply here too. No-op if the open scenes have no global volume.
        private void InjectGlobalVolume()
        {
            Volume global = FindGlobalVolume();
            if (global == null || global.sharedProfile == null)
                return;

            GameObject go = new("PreviewGlobalVolume") { hideFlags = HideFlags.HideAndDontSave };
            Volume v = go.AddComponent<Volume>();
            v.isGlobal = true;
            v.priority = global.priority;
            v.sharedProfile = global.sharedProfile;
            preview.AddSingleGO(go);
        }

        private static Volume FindGlobalVolume()
        {
            Volume best = null;
            foreach (Volume v in UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                if (!v.isGlobal || v.sharedProfile == null)
                    continue;
                if (best == null || v.priority > best.priority)
                    best = v;
            }
            return best;
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
            Vector3 center = framePivot + rotation * Vector3.right * pan.x + rotation * Vector3.up * pan.y;

            preview.camera.transform.position = center - direction * distance;
            preview.camera.transform.rotation = rotation;
            preview.lights[0].transform.rotation = Quaternion.Euler(light.y, light.x, 0f);

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
            activeButton = e.button;
            lastPointer = e.localPosition;
            previewImage.CapturePointer(e.pointerId);
            previewImage.Focus(); // grab keyboard focus so F works
            e.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (!dragging)
                return;

            Vector2 delta = (Vector2)e.localPosition - lastPointer;
            lastPointer = e.localPosition;

            switch (activeButton)
            {
                case 0: // left → orbit camera
                    orbit.x += delta.x * 0.4f;
                    orbit.y = Mathf.Clamp(orbit.y + delta.y * 0.4f, -89f, 89f);
                    break;

                case 2: // middle → pan (clamped near the character)
                    float k = distance * 0.0018f;
                    pan.x = Mathf.Clamp(pan.x - delta.x * k, -maxPan, maxPan);
                    pan.y = Mathf.Clamp(pan.y + delta.y * k, -maxPan, maxPan);
                    break;

                case 1: // right → rotate the light
                    light.x += delta.x * 0.5f;
                    light.y = Mathf.Clamp(light.y + delta.y * 0.5f, -89f, 89f);
                    break;
            }

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

        // F recenters on the character and zooms back out to the framing distance.
        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode != KeyCode.F)
                return;

            pan = Vector2.zero;
            distance = framingDistance;
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
        // Explorer (left pane)
        // ====================================================================

        private VisualElement BuildExplorer()
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

        // Selecting a character in the list is what launches its edition: highlight it, load it into
        // Synty, and the live poll picks up the regenerated character for the inspector preview.
        private void SelectAndEdit(string path)
        {
            selectedSkPath = path;
            RebuildSkRows();
            EditSk(path);
        }

        // Loads a .sk into Synty — as if it had been loaded from their own window.
        private void EditSk(string path)
        {
            ModularCharacterWindow window = OpenSyntyWindow();
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
            row.AddToClassList("characters-row--clickable");
            row.EnableInClassList("characters-row--selected", path == selectedSkPath);

            Label name = new(Path.GetFileNameWithoutExtension(path)) { tooltip = path };
            name.AddToClassList("cutscene-row-name");
            row.Add(name);

            // Clicking the row loads the character; a small Ping stays for locating the asset.
            row.RegisterCallback<ClickEvent>(_ => SelectAndEdit(path));

            Button ping = new(() => PingSk(path)) { text = "Ping" };
            ping.AddToClassList("cutscene-row-ping");
            ping.RegisterCallback<ClickEvent>(e => e.StopPropagation()); // don't trigger the row load
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
