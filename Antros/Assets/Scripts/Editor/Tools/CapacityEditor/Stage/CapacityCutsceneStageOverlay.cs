using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Scene-view overlay shown while a CapacityCutsceneStage is open: an Auto Save
    /// toggle (on by default) and a Save Now button. Only visible inside the stage.
    /// PreviewSceneStage has no save hook, so this overlay is how the custom save is
    /// surfaced to the user.
    /// </summary>
    [Overlay(typeof(SceneView), OverlayId, "Cutscene")]
    [Icon("d_UnityEditor.Timeline.TimelineWindow")]
    public sealed class CapacityCutsceneStageOverlay : Overlay
    {
        private const string OverlayId = "atcg-cutscene-stage-overlay";

        private ToolbarToggle autoSaveToggle;

        public override VisualElement CreatePanelContent()
        {
            VisualElement root = new() { style = { flexDirection = FlexDirection.Row } };

            autoSaveToggle = new ToolbarToggle { text = "Auto Save", value = CapacityCutsceneStage.AutoSave };
            autoSaveToggle.RegisterValueChangedCallback(evt => CapacityCutsceneStage.AutoSave = evt.newValue);
            root.Add(autoSaveToggle);

            ToolbarButton saveNow = new(() =>
            {
                CapacityCutsceneStage stage = CapacityCutsceneStage.Current;
                if (stage != null)
                    stage.Save();
            }) { text = "Save Now" };
            root.Add(saveNow);

            return root;
        }

        [SuppressMessage("Domain reload", "UDR0004:Domain Reload Analyzer")]
        public override void OnCreated()
        {
            base.OnCreated();
            UpdateVisibility();
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.update += UpdateVisibility;
        }

        public override void OnWillBeDestroyed()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorApplication.update -= UpdateVisibility;
            base.OnWillBeDestroyed();
        }

        private void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
            => UpdateVisibility();

        // Only display the overlay while our stage is the active one.
        private void UpdateVisibility()
        {
            bool inStage = CapacityCutsceneStage.Current != null;
            if (displayed != inStage)
                displayed = inStage;

            if (inStage && autoSaveToggle != null)
                autoSaveToggle.SetValueWithoutNotify(CapacityCutsceneStage.AutoSave);
        }
    }
}