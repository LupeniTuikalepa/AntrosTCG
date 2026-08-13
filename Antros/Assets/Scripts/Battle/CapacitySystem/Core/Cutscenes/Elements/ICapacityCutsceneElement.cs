using ATCG.Cutscenes;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements
{
    /// <summary>
    /// Kept as the capacity-facing name for a cutscene element; it carries no extra members over
    /// the generic <see cref="ICutsceneElement"/>. New elements can implement either — they are the
    /// same contract (Connect on an <see cref="ICutsceneContext"/> + Disconnect).
    /// </summary>
    public interface ICapacityCutsceneElement : ICutsceneElement
    {
    }
}
