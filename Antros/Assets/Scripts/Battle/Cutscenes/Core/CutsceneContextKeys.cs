using ATCG.Battle.Entities;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.HexGrids;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Well-known property keys every cutscene context provides by default, so recurring roles
    /// (the source actor, the screen player, the coordinate solver) travel through the same
    /// property system as consumer-specific data — no parallel channel. Any cutscene type
    /// (attacks, capacities, arrivals) shares this vocabulary; consumer-specific properties use
    /// their own keys alongside these.
    /// </summary>
    public static class CutsceneContextKeys
    {
        /// <summary>
        /// ATCG.Cutscenes.ICutsceneActor — the source actor's transform + renderers + animator.
        /// </summary>
        public const string CASTER = "CASTER";

        /// <summary>
        /// ATCG.Battle.Entities.EntityAddress — the source's ECS address (game only).
        /// </summary>
        public const string CASTER_ADDRESS = "CASTER_ADDRESS";

        /// <summary>
        /// ATCG.Battle.Players.Local.Runtime.RuntimeLocalBattlePlayer — the screen player (game only).
        /// </summary>
        public const string SCREEN_PLAYER = "SCREEN_PLAYER";

        /// <summary>
        /// ATCG.HexGrids.HexCoordinates — the cast / focus target coordinate.
        /// </summary>
        public const string CAST_POINT = "CAST_POINT";

        /// <summary>
        /// ATCG.Cutscenes.ICutsceneActor — the cutscene's target actor. Present only when the event has
        /// one (a basic attack's target, a capacity's primary target); the Target track binds to it.
        /// </summary>
        public const string TARGET = "TARGET";

        /// <summary>
        /// ICutsceneCoordinateSolver — hex coordinate -> world position.
        /// </summary>
        public const string COORDINATE_SOLVER = "COORDINATE_SOLVER";

        /// <summary>
        /// IQteResultReceiver — where a resolved QTE score is submitted (the owner screen turns it
        /// into a networked command). Present only for cutscenes that host QTE.
        /// </summary>
        public const string QTE_RECEIVER = "QTE_RECEIVER";

        public static ICutsceneActor GetCaster(this ICutsceneContext ctx) =>
            ctx.TryGetProperty(CASTER, out ICutsceneActor actor) ? actor : null;

        public static ICutsceneActor GetTarget(this ICutsceneContext ctx) =>
            ctx.TryGetProperty(TARGET, out ICutsceneActor actor) ? actor : null;

        public static EntityAddress GetCasterAddress(this ICutsceneContext ctx) =>
            ctx.TryGetProperty(CASTER_ADDRESS, out EntityAddress address) ? address : EntityAddress.None;

        public static RuntimeLocalBattlePlayer GetScreenBattlePlayer(this ICutsceneContext ctx) =>
            ctx.TryGetProperty(SCREEN_PLAYER, out RuntimeLocalBattlePlayer player) ? player : null;

        public static HexCoordinates GetCastPoint(this ICutsceneContext ctx) =>
            ctx.TryGetProperty(CAST_POINT, out HexCoordinates castPoint) ? castPoint : HexCoordinates.None;

        public static ICutsceneCoordinateSolver GetCoordinateSolver(this ICutsceneContext ctx) =>
            ctx.TryGetProperty(COORDINATE_SOLVER, out ICutsceneCoordinateSolver solver) ? solver : null;

        public static IQteResultReceiver GetQteReceiver(this ICutsceneContext ctx) =>
            ctx.TryGetProperty(QTE_RECEIVER, out IQteResultReceiver receiver) ? receiver : null;
    }
}
