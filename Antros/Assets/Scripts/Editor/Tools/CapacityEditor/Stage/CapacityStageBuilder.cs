using ATCG.Capacities;
using ATCG.Editor.Tools.CutsceneEditor;
using UnityEditor;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Thin capacity wrapper over the shared <see cref="CutsceneAssetBuilder"/>: it resolves the
    /// capacity's canonical folder, then delegates the timeline + director-prefab-variant scaffolding
    /// to the shared builder (which also assigns the built director back onto the data). Keeping the
    /// build logic in one place is what lets capacities and every other cutscene kind stay in sync.
    /// </summary>
    public static class CapacityStageBuilder
    {
        // Teach the generic builder where a capacity's stage assets belong. A capacity's DATA lives in
        // Resources/Database, but its stage (director prefab + timeline) must sit under
        // Project/Cutscenes — so a generic Fix that rebuilds a missing director still lands correctly
        // instead of dropping assets into Resources.
        [InitializeOnLoadMethod]
        private static void RegisterStageFolderResolver()
            => CutsceneAssetBuilder.StageFolderResolver = definition =>
                definition is CapacityData capacity ? CapacityAssetLayout.EnsureCapacityFolder(capacity) : null;

        public static bool TryBuild(CapacityData capacity, out string message)
        {
            if (capacity == null)
            {
                message = "No capacity selected.";
                return false;
            }

            string folder = CapacityAssetLayout.EnsureCapacityFolder(capacity);
            return CutsceneAssetBuilder.TryBuild(capacity, folder, capacity.name, out message);
        }
    }
}
