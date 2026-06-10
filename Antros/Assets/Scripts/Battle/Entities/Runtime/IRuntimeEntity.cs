using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Runtime;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace ATCG.Battle.Entities.Runtime
{
    public interface IRuntimeEntity
    {
        GameObject gameObject { get; }
        EntityAddress Address { get; }
        RuntimeEntityManager Manager { get; }
        RuntimeBattlePlayer RuntimeBattlePlayer { get; }
        IBattlePlayer BattlePlayer { get; }
        BattlePhase BattlePhase { get; }

        void OnSelected();
        void OnDeselected();
    }
}