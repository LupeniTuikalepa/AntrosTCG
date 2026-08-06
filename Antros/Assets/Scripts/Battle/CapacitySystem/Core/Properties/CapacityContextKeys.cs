using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.Entities;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.HexGrids;

namespace ATCG.Battle.CapacitySystem.Core.Properties
{
    /// <summary>
    /// Well-known property keys every context provides by default, so recurring refs
    /// (the caster, the screen player) travel through the same property system as
    /// capacity-specific data — no parallel channel. Capacity-specific properties use
    /// their own keys alongside these.
    /// </summary>
    public static class CapacityContextKeys
    {
        ///<summary>
        /// ATCG.Battle.Entities.Runtime.ICutsceneActor — the caster's transform + renderers.
        ///</summary>
        public const string CASTER = "CASTER";

        /// <summary>
        /// ATCG.Battle.Entities.EntityAddress — the caster's ECS address (game only).
        /// </summary>
        public const string CASTER_ADDRESS = "CASTER_ADDRESS";

        /// <summary>
        /// ATCG.Battle.Players.Local.Runtime.RuntimeLocalBattlePlayer — the screen player (game only).
        /// </summary>
        public const string SCREEN_PLAYER = "SCREEN_PLAYER";

        /// <summary>
        /// ATCG.HexGrids.HexCoordinates — the cast target coordinate.
        /// </summary>
        public const string CAST_POINT = "CAST_POINT";

        /// <summary>
        /// ICutsceneCoordinateSolver — hex coordinate -> world position.
        /// </summary>
        public const string COORDINATE_SOLVER = "COORDINATE_SOLVER";

        public static ICutsceneActor GetCaster(this ICapacityContext ctx) =>
            ctx.TryGetProperty(CASTER, out ICutsceneActor actor) ? actor : null;

        public static EntityAddress GetCasterAddress(this ICapacityContext ctx) =>
            ctx.TryGetProperty(CASTER_ADDRESS, out EntityAddress address) ? address : EntityAddress.None;

        public static RuntimeLocalBattlePlayer GetScreenBattlePlayer(this ICapacityContext ctx) =>
            ctx.TryGetProperty(SCREEN_PLAYER, out RuntimeLocalBattlePlayer player) ? player : null;

        public static HexCoordinates GetCastPoint(this ICapacityContext ctx) =>
            ctx.TryGetProperty(CAST_POINT, out HexCoordinates castPoint) ? castPoint : HexCoordinates.None;

        public static ICutsceneCoordinateSolver GetCoordinateSolver(this ICapacityContext ctx) =>
            ctx.TryGetProperty(COORDINATE_SOLVER, out ICutsceneCoordinateSolver solver) ? solver : null;
    }
}