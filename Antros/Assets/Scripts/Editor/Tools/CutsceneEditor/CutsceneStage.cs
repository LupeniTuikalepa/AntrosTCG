using ATCG.Battle;
using ATCG.Battle.Entities.Runtime;
using ATCG.Cutscenes;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace ATCG.Editor.Tools.CutsceneEditor
{
    /// <summary>
    /// An isolated editing stage for ANY cutscene definition, in the spirit of Prefab Mode: it opens
    /// its own in-memory scene (no .unity file), populates it with the reusable test environment
    /// (hero + camera + CinemachineBrain + DebugCutsceneRig) plus the definition's own director
    /// prefab on top, locks the Timeline window on it, and connects the cutscene elements to a
    /// preview context so VFX resolve without a running game. Saving applies edits to the director
    /// prefab and flushes its timeline; the environment is scenery and is never persisted.
    ///
    /// This is the single authoring stage shared by every cutscene kind. Capacities extend it
    /// (<c>CapacityCutsceneStage</c>) only to swap in their property-aware preview context; the whole
    /// scene/timeline/save machinery lives here, once.
    ///
    /// PreviewSceneStage has no OnSaveOpenedStage hook (that's PrefabStage-only), so saving is driven
    /// from the stage overlay: an "Auto Save" toggle (on by default) plus a "Save Now" button, and an
    /// auto-save on close.
    /// </summary>
    public class CutsceneStage : PreviewSceneStage
    {
        // Unchanged pref key (was the capacity editor's) so the user's existing Auto Save choice
        // carries over now that the stage is shared.
        private const string AutoSavePrefKey = "ATCG.CapacityEditor.AutoSave";

        public static CutsceneStage Current { get; protected set; }

        public static bool AutoSave
        {
            get => EditorPrefs.GetBool(AutoSavePrefKey, true);
            set => EditorPrefs.SetBool(AutoSavePrefKey, value);
        }

        [SerializeField] protected CutsceneDefinition definition;
        [SerializeField] private GameObject stageInstance;   // survives domain reload
        private DebugCutsceneRig rig;
        protected ICutsceneContext previewContext;

        public CutsceneDefinition Definition => definition;
        public ICutsceneContext PreviewContext => previewContext;

        public PlayableDirector Director => stageInstance != null
            ? stageInstance.GetComponentInChildren<PlayableDirector>(true)
            : null;

        // Resolve the CinemachineBrain from the STAGE SCENE, not the rig table: a rig reference
        // points at the environment PREFAB ASSET (scene invalid), and a camera with no valid scene
        // culls nothing and renders black in the preview. Searching the stage scene returns the live
        // instance whose scene is valid.
        public CinemachineBrain Brain
        {
            get
            {
                if (!scene.IsValid())
                    return null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    CinemachineBrain found = root.GetComponentInChildren<CinemachineBrain>(true);
                    if (found != null)
                        return found;
                }
                return null;
            }
        }

        public DebugCutsceneRig Rig => rig;
        public Scene StageScene => scene;

        public static void Open(CutsceneDefinition definition)
        {
            if (definition == null || definition.Director == null)
            {
                Debug.LogWarning("[CutsceneEditor] Definition has no director to edit.");
                return;
            }

            CutsceneStage stage = CreateInstance<CutsceneStage>();
            stage.definition = definition;
            StageUtility.GoToStage(stage, true);
        }

        // Builds the preview context injected into the cutscene elements. The base provides the
        // generic built-ins (source actor + coordinate solver); capacities override to add their
        // authored property schema.
        protected virtual ICutsceneContext BuildPreviewContext(Transform sourceRoot, Animator sourceAnimator)
            => new DebugCutsceneContext(sourceRoot, sourceAnimator);

        // Safety net for "quitting Unity while the stage is still open": OnCloseStage is the normal
        // save point (back button, switching stage), but it isn't guaranteed to fire on an outright
        // Editor quit. Hooking quitting directly means unsaved edits still land.
        private void HookQuitSave()
        {
            EditorApplication.quitting -= OnEditorQuitting;
            EditorApplication.quitting += OnEditorQuitting;
        }

        private void UnhookQuitSave() => EditorApplication.quitting -= OnEditorQuitting;

        private void OnEditorQuitting()
        {
            if (AutoSave)
                Save();
        }

        // After a domain reload (recompile) the stage SO is reserialized: the [SerializeField] fields
        // survive, but Current and the non-serialized derived state (rig, context, timeline lock) are
        // lost while the visual stage remains. Rebuild the derived state.
        protected new virtual void OnEnable()
        {
            if (stageInstance == null || !scene.IsValid())
                return;

            Current = this;

            if (rig == null)
                rig = FindRigInScene();

            ConnectElements();
            CutsceneAutoBinder.RebindAll(Director, rig);
            OpenAndLockTimeline();
            HookQuitSave();
        }

        private DebugCutsceneRig FindRigInScene()
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                DebugCutsceneRig found = root.GetComponentInChildren<DebugCutsceneRig>(true);
                if (found != null)
                    return found;
            }
            return null;
        }

        protected override bool OnOpenStage()
        {
            base.OnOpenStage();

            Scene stageScene = scene;
            CutsceneEditorSettings settings = CutsceneEditorSettings.GetOrCreate();

            // 1. Test environment (hero, camera + CinemachineBrain, rig). Scenery only.
            if (settings.testEnvironmentPrefab != null)
            {
                GameObject env = (GameObject)PrefabUtility.InstantiatePrefab(
                    settings.testEnvironmentPrefab, stageScene);
                rig = env.GetComponentInChildren<DebugCutsceneRig>(true);
                if (rig == null)
                    Debug.LogWarning("[CutsceneEditor] Test environment has no DebugCutsceneRig — " +
                                     "auto-bindable tracks won't bind.");
            }
            else
            {
                Debug.LogWarning("[CutsceneEditor] No test environment prefab set (Settings tab). " +
                                 "Bindings won't preview correctly.");
            }

            // 2. The definition's director prefab — this is what the user actually edits.
            GameObject prefabRoot = ResolvePrefabRoot(definition.Director);
            if (prefabRoot == null)
            {
                Debug.LogWarning($"[CutsceneEditor] Couldn't resolve the director prefab for '{definition.name}'.");
                return false;
            }

            stageInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot, stageScene);

            // The prefab's serialized bindings point at objects outside this stage; reconnect every
            // auto-bindable track to the rig present here.
            CutsceneAutoBinder.RebindAll(Director, rig);
            ConnectElements();

            OpenAndLockTimeline();

            Current = this;
            HookQuitSave();
            return true;
        }

        protected override void OnCloseStage()
        {
            if (AutoSave)
                Save();

            UnhookQuitSave();
            UnlockTimeline();

            if (Current == this)
                Current = null;

            base.OnCloseStage();
        }

        // Wires the cutscene elements to a preview context so VFX resolve their source from the test
        // hero instead of a running game. The hero is the object bound to the HeroAnimator channel.
        private void ConnectElements()
        {
            if (stageInstance == null)
                return;

            Transform sourceRoot = null;
            Animator sourceAnimator = null;
            if (rig != null && rig.TryGet(CutsceneChannels.HeroAnimator.trackName, out Object heroRef))
            {
                sourceAnimator = heroRef as Animator;
                if (sourceAnimator)
                    sourceRoot = sourceAnimator.GetComponentInParent<IRuntimeEntity>().transform;
            }

            // Body-part LinkedRenderer keys are auto-assigned by LinkedRendererMapper.Awake at
            // runtime; Awake never fires in the edit-mode preview, so map here — otherwise key-based
            // VFX (PropagateVFX) find no renderers and spawn nothing. Guarded: a mapping failure
            // must not abort OnOpenStage (that would leave Current unset and break every custom
            // editor that keys off the active session, e.g. the step-marker dropdown).
            if (sourceAnimator != null)
            {
                try
                {
                    sourceAnimator.GetComponentInParent<LinkedRendererMapper>()?.Map();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[CutsceneStage] LinkedRenderer mapping failed: {e.Message}");
                }
            }

            previewContext = BuildPreviewContext(sourceRoot, sourceAnimator);
            ReconnectElements();
        }

        // Re-runs Connect on every element with the current preview context. Called on open and again
        // whenever a tweak panel changes a property value.
        public void ReconnectElements()
        {
            if (stageInstance == null || previewContext == null)
                return;

            ICutsceneElement[] elements = stageInstance.GetComponentsInChildren<ICutsceneElement>(true);
            for (int i = 0; i < elements.Length; i++)
            {
                // Disconnect first so elements drop any stale bindings before re-pulling the
                // (now-updated) injected values from the preview context.
                elements[i].Disconnect();
                elements[i].Connect(previewContext);
            }
        }

        // Loads the stage director into the Timeline window and locks it, deferred one editor tick to
        // dodge a window-init race that silently dropped the lock on first open / after domain reload.
        private void OpenAndLockTimeline()
        {
            EditorApplication.delayCall += TryLockTimeline;
        }

        private void TryLockTimeline()
        {
            if (Current != this)
                return;

            PlayableDirector director = Director;
            if (director == null)
                return;

            TimelineEditorWindow window = TimelineEditor.GetOrCreateWindow();
            window.Show();
            window.SetTimeline(director);
            window.locked = true;
        }

        private static void UnlockTimeline()
        {
            TimelineEditorWindow window = TimelineEditor.GetWindow();
            if (window != null)
                window.locked = false;
        }

        /// <summary>
        /// Applies the stage instance's edits back to the director prefab and flushes its timeline +
        /// the definition. Called by the overlay button, on close when auto-save is on, and after
        /// scan writes when auto-save is on.
        /// </summary>
        public bool Save()
        {
            if (stageInstance == null)
            {
                Debug.LogWarning("[CutsceneEditor] Save skipped: stageInstance is null (stage already torn down?).");
                return true;
            }

            string prefabPath = AssetDatabase.GetAssetPath(definition.Director);
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogWarning("[CutsceneEditor] Director isn't a prefab asset — can't save.");
                return false;
            }

            // ApplyPrefabInstance can throw (e.g. stageInstance no longer recognized as an outermost
            // prefab instance root). Guard it so clip/timeline data still saves even if the
            // prefab-instance side fails, and the failure is impossible to miss.
            bool prefabApplied = true;
            try
            {
                PrefabUtility.ApplyPrefabInstance(stageInstance, InteractionMode.AutomatedAction);
            }
            catch (System.Exception e)
            {
                prefabApplied = false;
                Debug.LogError($"[CutsceneEditor] ApplyPrefabInstance failed for '{definition.name}': {e}");
            }

            if (definition.Timeline != null)
                EditorUtility.SetDirty(definition.Timeline);
            EditorUtility.SetDirty(definition);

            AssetDatabase.SaveAssets();

            if (prefabApplied)
                Debug.Log($"[CutsceneEditor] Saved '{definition.name}' to '{prefabPath}'.");

            return prefabApplied;
        }

        /// <summary>Saves only when auto-save is enabled (used by the periodic scan).</summary>
        public void AutoSaveIfEnabled()
        {
            if (AutoSave)
                Save();
        }

        protected override GUIContent CreateHeaderContent()
        {
            return new GUIContent(
                definition != null ? $"Cutscene: {definition.name}" : "Cutscene",
                EditorGUIUtility.IconContent("d_UnityEditor.Timeline.TimelineWindow").image);
        }

        // Resolve the prefab asset root the definition references, WITHOUT walking up the variant
        // chain: loading the asset at the referenced object's own path returns the correct root,
        // variant included (GetCorrespondingObjectFromSource would climb toward the base template).
        protected static GameObject ResolvePrefabRoot(PlayableDirector director)
        {
            if (director == null)
                return null;

            string path = AssetDatabase.GetAssetPath(director);
            if (string.IsNullOrEmpty(path))
                path = AssetDatabase.GetAssetPath(director.gameObject);

            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }
}
