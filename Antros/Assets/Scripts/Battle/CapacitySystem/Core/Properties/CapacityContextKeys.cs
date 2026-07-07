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
        // ATCG.Battle.Entities.Runtime.ICutsceneActor — the caster's transform + renderers.
        public const string CASTER = "CASTER";

        // ATCG.Battle.Entities.EntityAddress — the caster's ECS address (game only).
        public const string CASTER_ADDRESS = "CASTER_ADDRESS";

        // ATCG.Battle.Players.Local.Runtime.RuntimeLocalBattlePlayer — the screen player (game only).
        public const string SCREEN_PLAYER = "SCREEN_PLAYER";

        // ATCG.HexGrids.HexCoordinates — the cast target coordinate.
        public const string CAST_POINT = "CAST_POINT";

        // ICutsceneCoordinateSolver — hex coordinate -> world position.
        public const string COORDINATE_SOLVER = "COORDINATE_SOLVER";
    }
}