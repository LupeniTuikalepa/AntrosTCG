using ATCG.Capacities;
using ATCG.Cutscenes;
using ATCG.Editor.Tools.CapacityEditor;

namespace ATCG.Editor.Tools.CutsceneEditor
{
    /// <summary>
    /// Single entry point for opening the authoring stage of any cutscene definition. Both the
    /// per-asset inspector button and the dedicated Cutscenes window route through here, so there is
    /// exactly one place that decides which stage flavour a definition needs.
    ///
    /// Capacities still open their property-aware <see cref="CapacityCutsceneStage"/> (so authored
    /// property values seed the VFX preview); every other kind opens the plain shared
    /// <see cref="CutsceneStage"/>. This is the only spot where the generic side knows about the
    /// capacity subclass — kept deliberately narrow.
    /// </summary>
    public static class CutsceneAuthoring
    {
        public static void Open(CutsceneDefinition definition)
        {
            if (definition == null)
                return;

            if (definition is CapacityData capacity)
                CapacityCutsceneStage.Open(capacity);
            else
                CutsceneStage.Open(definition);
        }
    }
}
