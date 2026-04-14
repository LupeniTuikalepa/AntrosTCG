using System.Collections.Generic;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Entities.Heroes.Runtime
{
    public partial class RuntimeHero : IPhaseListener<SelectCellPhase>
    {
        private readonly List<SelectCellPhase> selectCellPhases = new();

        /*
        async Awaitable IBasicAttackCommandListener.BeginBasicAttack(BasicAttackCommand basicAttackCommand)
        {
            try
            {
                Tween.CompleteAll(transform);

                if (RuntimeBattleGrid.TryGetBattleCellAt(HeroCard.AllCellsCoordinates, out var cell))
                    await Tween.PunchScale(transform, Vector3.one * 1.5f, .25f);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        async Awaitable IMoveCommandListener.MoveTo(HexCoordinates coordinates)
        {
            try
            {
                Tween.CompleteAll(transform);
                Manager.Selectable.AddCondition(transform, false);

                if (RuntimeBattleGrid.TryGetBattleCellAt(coordinates, out var cell))
                {
                    transform.localScale = RuntimeBattleGrid.GetTargetScale();
                    await Tween.Position(transform, cell.transform.position, .25f, endDelay:.2f);
                }

                Manager.Selectable.RemoveCondition(transform);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        */

        void IPhaseListener<SelectCellPhase>.OnPhaseBegin(SelectCellPhase phase)
        {
            if (phase.Player == Manager.RuntimeBattleGrid.LocalBattlePlayer)
            {
                selectCellPhases.Add(phase);
                RefreshSelectCellPhaseState();
            }
        }

        void IPhaseListener<SelectCellPhase>.OnPhaseEnd(SelectCellPhase phase)
        {
            if (phase.Player == Manager.RuntimeBattleGrid.LocalBattlePlayer)
            {
                selectCellPhases.Remove(phase);
                RefreshSelectCellPhaseState();
            }
        }

        private void RefreshSelectCellPhaseState()
        {
        }
    }
}