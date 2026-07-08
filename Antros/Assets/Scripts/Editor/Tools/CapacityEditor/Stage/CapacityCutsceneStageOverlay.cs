using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Scene-view overlay shown while a CapacityCutsceneStage is open: an Auto Save
    /// toggle (on by default), a Save Now button, and a live camera preview. The
    /// preview renders the stage's Cinemachine-driven camera (a PreviewSceneStage
    /// camera isn't drawn by the normal pipeline), updating as the Timeline window
    /// scrubs the director.
    /// </summary>
    [Overlay(typeof(SceneView), OverlayId, "Cutscene")]
    [Icon("d_UnityEditor.Timeline.TimelineWindow")]
    public sealed class CapacityCutsceneStageOverlay : Overlay
    {
        private const string OverlayId = "atcg-cutscene-stage-overlay";
        private const int PreviewWidth = 320;
        private const int PreviewHeight = 180;

        private ToolbarToggle autoSaveToggle;
        private Image previewImage;
        private CapacityCameraPreview preview;
        private CapacityCutsceneStage boundStage;

        public override VisualElement CreatePanelContent()
        {
            VisualElement root = new();

            VisualElement toolbar = new() { style = { flexDirection = FlexDirection.Row } };
            autoSaveToggle = new ToolbarToggle { text = "Auto Save", value = CapacityCutsceneStage.AutoSave };
            autoSaveToggle.RegisterValueChangedCallback(evt => CapacityCutsceneStage.AutoSave = evt.newValue);
            toolbar.Add(autoSaveToggle);

            toolbar.Add(new ToolbarButton(() =>
            {
                CapacityCutsceneStage stage = CapacityCutsceneStage.Current;
                if (stage != null)
                    stage.Save();
            }) { text = "Save Now" });
            root.Add(toolbar);

            previewImage = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = PreviewWidth,
                    height = PreviewHeight,
                    marginTop = 4,
                    backgroundColor = new Color(0f, 0f, 0f, 1f)
                }
            };
            root.Add(previewImage);

            return root;
        }

        public override void OnCreated()
        {
            base.OnCreated();
            EditorApplication.update += Tick;
        }

        public override void OnWillBeDestroyed()
        {
            EditorApplication.update -= Tick;
            preview?.Dispose();
            preview = null;
            base.OnWillBeDestroyed();
        }

        private void Tick()
        {
            // NOTE: never drive `displayed` here — forcing it every frame overrides the
            // user toggling the overlay on/off. Visibility stays user-controlled; only
            // the preview content reacts to whether a stage is open.
            CapacityCutsceneStage stage = CapacityCutsceneStage.Current;

            if (stage == null)
            {
                if (preview != null)
                {
                    preview.Dispose();
                    preview = null;
                    boundStage = null;
                }
                if (previewImage != null)
                    previewImage.image = null;
                return;
            }

            if (autoSaveToggle != null)
                autoSaveToggle.SetValueWithoutNotify(CapacityCutsceneStage.AutoSave);

            try
            {
                // Rebuild the preview when the stage changes OR when the resolved camera
                // differs from the one the preview is holding. Relying on boundStage alone
                // kept a stale camera (e.g. after a recompile or a rig/scene change), which
                // is why the preview stayed black — the new Brain was never re-queried.
                Camera currentBrainCam = stage.Brain != null ? stage.Brain.GetComponent<Camera>() : null;
                bool needsRebuild = boundStage != stage || preview == null || preview.Camera != currentBrainCam;

                if (needsRebuild)
                {
                    preview?.Dispose();
                    preview = new CapacityCameraPreview(stage.Brain, stage.Director);
                    boundStage = stage;
                }

                if (preview != null && preview.IsValid)
                {
                    preview.Render(PreviewWidth, PreviewHeight);
                    if (previewImage != null)
                        previewImage.image = preview.Texture;
                }
            }
            catch (System.Exception e)
            {
                // A transient stage/camera state must not throw every frame and take the
                // overlay down; log once-ish and keep the overlay alive.
                Debug.LogWarning($"[CapacityTimelineEditor] Camera preview skipped: {e.Message}");
                preview?.Dispose();
                preview = null;
                boundStage = null;
            }
        }
    }
}