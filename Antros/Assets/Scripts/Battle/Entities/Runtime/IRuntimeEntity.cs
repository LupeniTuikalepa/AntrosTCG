using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Runtime;
using ATCG.HexGrids;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace ATCG.Battle.Entities.Runtime
{
    public interface IRuntimeEntity
    {
        GameObject gameObject { get; }
        Transform transform { get; }

        EntityAddress Address { get; }
        RuntimeEntityManager Manager { get; }
        IRuntimeBattlePlayer<LocalBattlePlayer> RuntimeBattlePlayer { get; }
        IBattlePlayer BattlePlayer { get; }
        BattlePhase BattlePhase { get; }
        Transform actionUIRoot { get; }
        Transform statusRoot { get; }
        Renderer[] Models { get; }

        void OnHovered();
        void OnUnhovered();
        void OnSelected();
        void OnDeselected();

        Awaitable LookAtCoord(HexCoordinates coordinates, float duration = 0.3f);
    }
}