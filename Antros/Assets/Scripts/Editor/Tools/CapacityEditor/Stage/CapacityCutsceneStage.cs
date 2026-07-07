using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks;
using ATCG.Capacities;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// An isolated editing stage for a capacity's cutscene, in the spirit of Prefab
    /// Mode: it opens its own in-memory scene (no .unity file) and shows a breadcrumb.
    /// The stage is populated with the reusable test environment (hero + camera +
    /// CinemachineBrain + DebugCutsceneRig) plus the capacity's own director prefab on
    /// top. Saving applies edits to the director prefab and flushes its timeline; the
    /// environment is scenery and is never persisted.
    ///
    /// PreviewSceneStage has no OnSaveOpenedStage hook (that's PrefabStage-only), so
    /// saving is driven from the stage overlay: an "Auto Save" toggle (on by default)
    /// plus a "Save Now" button, and an auto-save on close.
    /// </summary>
    public sealed class CapacityCutsceneStage : PreviewSceneStage
    {
        private const string AutoSavePrefKey = "ATCG.CapacityEditor.AutoSave";

        public static CapacityCutsceneStage Current { get; private set; }

        public static bool AutoSave
        {
            get => EditorPrefs.GetBool(AutoSavePrefKey, true);
            set => EditorPrefs.SetBool(AutoSavePrefKey, value);
        }

        private CapacityData capacity;
        private GameObject stageInstance;   // the instantiated director prefab (edited)
        private DebugCutsceneRig rig;
        private DebugCapacityContext previewContext;

        public CapacityData Capacity => capacity;

        public PlayableDirector Director => stageInstance != null
            ? stageInstance.GetComponentInChildren<PlayableDirector>(true)
            : null;

        public CinemachineBrain Brain=> rig.TryGet(CutsceneChannels.MainCamera, out Object cam) && cam is CinemachineBrain brain ? brain : null;
        public DebugCutsceneRig Rig => rig;
        public DebugCapacityContext PreviewContext => previewContext;


        public static void Open(CapacityData capacity)
        {
            if (capacity == null || capacity.CutsceneDirector == null)
            {
                Debug.LogWarning("[CapacityTimelineEditor] Capacity has no cutscene director to edit.");
                return;
            }

            CapacityCutsceneStage stage = CreateInstance<CapacityCutsceneStage>();
            stage.capacity = capacity;
            StageUtility.GoToStage(stage, true);
        }

        protected override bool OnOpenStage()
        {
            base.OnOpenStage();

            Scene stageScene = scene;
            CapacityEditorSettings settings = CapacityEditorSettings.GetOrCreate();

            // 1. Test environment (hero, camera + CinemachineBrain, rig). Scenery only.
            if (settings.testEnvironmentPrefab != null)
            {
                GameObject env = (GameObject)PrefabUtility.InstantiatePrefab(
                    settings.testEnvironmentPrefab, stageScene);
                rig = env.GetComponentInChildren<DebugCutsceneRig>(true);
                if (rig == null)
                    Debug.LogWarning("[CapacityTimelineEditor] Test environment has no DebugCutsceneRig — " +
                                     "auto-bindable tracks won't bind.");
            }
            else
            {
                Debug.LogWarning("[CapacityTimelineEditor] No test environment prefab set (Settings tab). " +
                                 "Bindings won't preview correctly.");
            }

            // 2. The capacity's director prefab — this is what the user actually edits.
            GameObject prefabRoot = ResolvePrefabRoot(capacity.CutsceneDirector);
            if (prefabRoot == null)
            {
                Debug.LogWarning($"[CapacityTimelineEditor] Couldn't resolve the director prefab for '{capacity.name}'.");
                return false;
            }

            stageInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot, stageScene);

            // The prefab's serialized bindings point at objects outside this stage;
            // reconnect every auto-bindable track to the rig present here.
            CapacityAutoBinder.RebindAll(Director, rig);
            ConnectElements();

            OpenAndLockTimeline();

            Current = this;
            return true;
        }

        protected override void OnCloseStage()
        {
            if (AutoSave)
                Save();

            UnlockTimeline();

            if (Current == this)
                Current = null;

            base.OnCloseStage();
        }

        // Wires the cutscene elements to a preview context so VFX (particles, etc.)
        // resolve their caster from the test hero instead of a running game. The hero
        // is the object bound to the HeroAnimator channel on the rig.
        private void ConnectElements()
        {
            if (stageInstance == null)
                return;

            Transform heroRoot = null;
            Animator heroAnimator = null;
            if (rig != null && rig.TryGet(CutsceneChannels.HeroAnimator.trackName, out Object heroRef))
            {
                heroAnimator = heroRef as Animator;
                if (heroRef is Component heroComponent)
                    heroRoot = heroComponent.transform;
            }

            previewContext = new DebugCapacityContext(capacity, heroRoot, heroAnimator);
            ReconnectElements();
        }

        // Re-runs Connect on every element with the current preview context. Called on
        // open and again whenever the tweak panel changes a property value.
        public void ReconnectElements()
        {
            if (stageInstance == null || previewContext == null)
                return;

            ICapacityCutsceneElement[] elements = stageInstance.GetComponentsInChildren<ICapacityCutsceneElement>(true);
            for (int i = 0; i < elements.Length; i++)
                elements[i].Connect(previewContext);
        }

        // Loads the stage director into the Timeline window and locks it, so editing
        // stays pinned to this cutscene even when selection changes elsewhere.
        private void OpenAndLockTimeline()
        {
            PlayableDirector director = Director;
            if (director == null)
                return;

            TimelineEditorWindow window = TimelineEditor.GetOrCreateWindow();
            window.SetTimeline(director);
            window.locked = true;
            window.Show();
        }

        private static void UnlockTimeline()
        {
            TimelineEditorWindow window = TimelineEditor.GetWindow();
            if (window != null)
                window.locked = false;
        }

        /// <summary>
        /// Applies the stage instance's edits back to the director prefab and flushes
        /// its timeline + the capacity data. Called by the overlay button, on close
        /// when auto-save is on, and after scan writes when auto-save is on.
        /// </summary>
        public bool Save()
        {
            if (stageInstance == null)
                return true;

            string prefabPath = AssetDatabase.GetAssetPath(capacity.CutsceneDirector);
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogWarning("[CapacityTimelineEditor] Director isn't a prefab asset — can't save.");
                return false;
            }

            PrefabUtility.ApplyPrefabInstance(stageInstance, InteractionMode.AutomatedAction);

            if (capacity.CutsceneTimeline != null)
                EditorUtility.SetDirty(capacity.CutsceneTimeline);
            EditorUtility.SetDirty(capacity);

            AssetDatabase.SaveAssets();
            return true;
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
                capacity != null ? $"Cutscene: {capacity.name}" : "Cutscene",
                EditorGUIUtility.IconContent("d_UnityEditor.Timeline.TimelineWindow").image);
        }

        private static GameObject ResolvePrefabRoot(PlayableDirector director)
        {
            GameObject go = director.gameObject;

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (source != null)
                go = source;

            string path = AssetDatabase.GetAssetPath(go);
            if (string.IsNullOrEmpty(path))
                path = AssetDatabase.GetAssetPath(director);

            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }
}