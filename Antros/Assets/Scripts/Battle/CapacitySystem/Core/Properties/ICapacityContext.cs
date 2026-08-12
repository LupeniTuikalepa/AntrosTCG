using ATCG.Cutscenes;

namespace ATCG.Battle.CapacitySystem.Core.Properties
{
    /// <summary>
    /// The capacity flavour of <see cref="ICutsceneContext"/> — implemented by CastCapacityPhase in
    /// game and by a debug context in the editor preview. It carries no extra members for now; it
    /// exists so capacity code can keep referring to a capacity-specific type while every cutscene
    /// element binds to the generic <see cref="ICutsceneContext"/> underneath. Properties are keyed
    /// by string (see CutsceneContextKeys for the well-known ones) and typed at retrieval.
    /// </summary>
    public interface ICapacityContext : ICutsceneContext
    {
    }
}
