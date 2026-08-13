using ATCG.Cutscenes;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Editor.Tools.CutsceneEditor
{
    /// <summary>
    /// Editor-preview implementation of <see cref="ICutsceneContext"/> for any cutscene that
    /// isn't a capacity: it provides the well-known built-ins every cutscene element expects — the
    /// source actor (taken from the test-environment hero) and a flat coordinate solver — through an
    /// open property bag. Capacities use the richer <c>DebugCapacityContext</c> instead, which adds
    /// their authored property schema on top of these same built-ins.
    /// </summary>
    public class DebugCutsceneContext : ICutsceneContext
    {
        private readonly CutscenePropertyBag bag = new();

        public DebugCutsceneContext(Transform sourceRoot, Animator sourceAnimator)
        {
            if (sourceRoot != null)
                bag.Set<ICutsceneActor>(CutsceneContextKeys.CASTER, new DebugCutsceneActor(sourceRoot, sourceAnimator));

            bag.Set<ICutsceneCoordinateSolver>(CutsceneContextKeys.COORDINATE_SOLVER, new PreviewCoordinateSolver());
            bag.Set(CutsceneContextKeys.CAST_POINT, default(HexCoordinates));
        }

        public bool TryGetProperty<T>(string name, out T value) => bag.TryGet(name, out value);
        public void InjectProperty<T>(string name, T value) => bag.Set(name, value);
    }
}
