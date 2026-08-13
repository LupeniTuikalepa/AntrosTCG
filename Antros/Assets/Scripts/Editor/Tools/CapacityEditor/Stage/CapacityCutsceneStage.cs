using ATCG.Capacities;
using ATCG.Cutscenes;
using ATCG.Editor.Tools.CutsceneEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// The capacity flavour of the shared <see cref="CutsceneStage"/>: it reuses the entire
    /// scene/timeline/save machinery and only swaps in a property-aware preview context
    /// (<see cref="DebugCapacityContext"/>) so authored capacity properties carry their tweaked test
    /// values into the VFX preview. Everything else — opening the isolated scene, locking the
    /// Timeline, rebinding to the rig, saving back to the director prefab, surviving domain reloads —
    /// lives in the base and is shared with every other cutscene kind.
    /// </summary>
    public sealed class CapacityCutsceneStage : CutsceneStage
    {
        /// <summary>The open capacity stage, or null when none / when a non-capacity stage is open.</summary>
        public static new CapacityCutsceneStage Current => CutsceneStage.Current as CapacityCutsceneStage;

        /// <summary>The capacity being edited (the definition, typed).</summary>
        public CapacityData Capacity => definition as CapacityData;

        /// <summary>The preview context, typed so the tweak panel can push edited property values.</summary>
        public new DebugCapacityContext PreviewContext => base.PreviewContext as DebugCapacityContext;

        public static void Open(CapacityData capacity)
        {
            if (capacity == null || capacity.Director == null)
            {
                Debug.LogWarning("[CapacityTimelineEditor] Capacity has no cutscene director to edit.");
                return;
            }

            CapacityCutsceneStage stage = CreateInstance<CapacityCutsceneStage>();
            stage.definition = capacity;
            StageUtility.GoToStage(stage, true);
        }

        // Capacity preview needs the authored property schema seeded with saved debug values, so it
        // uses the richer DebugCapacityContext instead of the generic one.
        protected override ICutsceneContext BuildPreviewContext(Transform sourceRoot, Animator sourceAnimator)
            => new DebugCapacityContext(Capacity, sourceRoot, sourceAnimator);
    }
}
