using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.HexGrids;

namespace ATCG.Battle.CapacitySystem.Core.Properties
{
    /// <summary>
    /// The per-screen context handed to cutscene elements at Connect. Its property
    /// schema is pre-declared from the capacity's PropertyDefinitions, then the
    /// screen-specific defaults (caster actor, screen player, cast point, solver) are
    /// written into it. Only declared properties can be written — same closed schema
    /// as the game phase and the editor preview.
    /// </summary>
    public sealed class CutsceneCapacityContext : ICapacityContext
    {
        private readonly CapacityPropertyBag bag = new();

        public CutsceneCapacityContext(CastCapacityPhase phase, RuntimeLocalBattlePlayer screenPlayer)
        {
            bag.Declare(phase.data.PropertyDefinitions);

            bag.Allow<RuntimeLocalBattlePlayer>(CapacityContextKeys.SCREEN_PLAYER);
            bag.Allow<EntityAddress>(CapacityContextKeys.CASTER_ADDRESS);
            bag.Allow<HexCoordinates>(CapacityContextKeys.CAST_POINT);
            bag.Allow<ICutsceneCoordinateSolver>(CapacityContextKeys.COORDINATE_SOLVER);
            bag.Allow<ICutsceneActor>(CapacityContextKeys.CASTER);

            InjectProperty(CapacityContextKeys.SCREEN_PLAYER, screenPlayer);
            InjectProperty(CapacityContextKeys.CASTER_ADDRESS, phase.caster);
            InjectProperty(CapacityContextKeys.CAST_POINT, phase.castPoint);
            InjectProperty<ICutsceneCoordinateSolver>(
                CapacityContextKeys.COORDINATE_SOLVER, new GridCoordinateSolver(screenPlayer));

            if (phase.TryGetRuntimeCaster(screenPlayer, out IRuntimeEntity caster) && caster is ICutsceneActor actor)
                InjectProperty(CapacityContextKeys.CASTER, actor);
        }

        public bool TryGetProperty<T>(string name, out T value) => bag.TryGet(name, out value);
        public void InjectProperty<T>(string name, T value) => bag.Set(name, value);
    }
}