using ATCG.Battle.Cards;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface IBattleCellLookupPhase : IPhaseCompletionSource<BattleCell>
    {
        int PlayerID { get; }
        IBattleCellLookupFilter Filter { get; }
        ChannelKey ChannelKey { get; }
        bool IsCoordValid(HexCoordinates coordinates);

    }
}