using UnityEngine;

namespace ATCG.Battle.Cards.Capacities
{
    public interface IBasicAttackEventRunner : ICardEventRunner
    {
        Awaitable BeginBasicAttack(BasicAttackEvent basicAttackEvent);
    }
}