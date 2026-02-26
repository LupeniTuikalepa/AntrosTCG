using System;
using ATCG.Battle.Cards.Capacities;
using ATCG.HexGrids;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Heroes.Runtime
{
    public partial class RuntimeHero : IBasicAttackEventRunner, IMoveEventRunner
    {
        async Awaitable IBasicAttackEventRunner.BeginBasicAttack(BasicAttackEvent basicAttackEvent)
        {
            try
            {
                Tween.CompleteAll(transform);

                if (RuntimeBattleGrid.TryGetBattleCellAt(Card.Coordinates, out var cell))
                    await Tween.PunchScale(transform, cell.GetTargetScale() * 1.5f, .25f);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        async Awaitable IMoveEventRunner.MoveTo(HexCoordinates coordinates)
        {
            try
            {
                Tween.CompleteAll(transform);
                Selectable.AddCondition(transform, false);

                if (RuntimeBattleGrid.TryGetBattleCellAt(coordinates, out var cell))
                {
                    transform.localScale = cell.GetTargetScale();
                    await Tween.Position(transform, cell.transform.position, .25f, endDelay:.2f);
                }

                Selectable.RemoveCondition(transform);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}