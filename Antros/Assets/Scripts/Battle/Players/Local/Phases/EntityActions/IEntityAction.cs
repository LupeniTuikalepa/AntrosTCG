using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using UnityEngine;

namespace ATCG.Battle
{
    public interface IEntityAction
    {
        int ManaCost { get; }
        Awaitable Execute(EntityAddress address, BattlePhase battlePhase);
    }
}