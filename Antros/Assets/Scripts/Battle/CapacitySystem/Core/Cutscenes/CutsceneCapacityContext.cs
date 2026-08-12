using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.Entities;
using ATCG.Cutscenes;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.HexGrids;

namespace ATCG.Battle.CapacitySystem.Core.Properties
{
    /// <summary>
    /// Per-screen cutscene context. Two layers:
    ///  - LOCAL: screen-specific built-ins (caster actor, screen player, cast point,
    ///    coordinate solver) that differ per screen.
    ///  - GLOBAL: the CastCapacityPhase, shared across screens. Steps inject the real
    ///    capacity values into the phase over time (e.g. Targets after a QTE); reads
    ///    delegate to it LIVE so those injections are visible whenever they happen.
    /// Local wins on key collision; everything else falls through to the phase.
    /// </summary>
    public sealed class CutsceneCapacityContext : ICapacityContext
    {
        private readonly CastCapacityPhase phase;
        private readonly CapacityPropertyBag local = new();

        public CutsceneCapacityContext(CastCapacityPhase phase, RuntimeLocalBattlePlayer screenPlayer)
        {
            this.phase = phase;

            // Declare + fill the screen-local built-ins.
            local.Allow<RuntimeLocalBattlePlayer>(CutsceneContextKeys.SCREEN_PLAYER);
            local.Allow<EntityAddress>(CutsceneContextKeys.CASTER_ADDRESS);
            local.Allow<HexCoordinates>(CutsceneContextKeys.CAST_POINT);
            local.Allow<ICutsceneCoordinateSolver>(CutsceneContextKeys.COORDINATE_SOLVER);
            local.Allow<ICutsceneActor>(CutsceneContextKeys.CASTER);

            local.Set(CutsceneContextKeys.SCREEN_PLAYER, screenPlayer);
            local.Set(CutsceneContextKeys.CASTER_ADDRESS, phase.caster);
            local.Set(CutsceneContextKeys.CAST_POINT, phase.castPoint);
            local.Set<ICutsceneCoordinateSolver>(CutsceneContextKeys.COORDINATE_SOLVER, new GridCoordinateSolver(screenPlayer));

            if (phase.TryGetRuntimeCaster(screenPlayer, out IRuntimeEntity caster) && caster is ICutsceneActor actor)
                local.Set(CutsceneContextKeys.CASTER, actor);
        }

        // Local (screen) first, then live-delegate to the phase (global) so values the
        // steps inject during the cutscene are picked up.
        public bool TryGetProperty<T>(string name, out T value)
        {
            if (local.IsDeclared(name) && local.TryGet(name, out value))
                return true;
            return phase.TryGetProperty(name, out value);
        }

        // Writing a built-in key updates the screen-local layer; anything else is a
        // capacity property and goes to the shared phase.
        public void InjectProperty<T>(string name, T value)
        {
            if (local.IsDeclared(name))
                local.Set(name, value);
            else
                phase.InjectProperty(name, value);
        }
    }
}
