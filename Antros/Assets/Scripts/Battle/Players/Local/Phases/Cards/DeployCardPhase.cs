using System;
using System.Threading;
using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Lookups;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.HexGrids;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases.Cards
{
    public class DeployCardPhase : LocalPlayerPhase
    {
        private struct DeployableCellFilter : IEntityFilter
        {
            public bool Accepts(EntityAddress address)
            {
                if (address.Is(out BattleCellAspect aspect))
                    return !aspect.HasMembers;

                return false;
            }
        }

        private readonly IBattleCard battleCard;
        private readonly CardDragPhase<IBattleCard> dragPhase;

        public DeployCardPhase(LocalBattlePlayer localBattlePlayer, IBattleCard battleCard, CardDragPhase<IBattleCard> dragPhase) : base(localBattlePlayer)
        {
            this.battleCard = battleCard;
            this.dragPhase = dragPhase;
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            //TODO create pattern with deployable cells
            //As of now, all the grid is deployable
            HexPatternBuilder patternBuilder = new HexPatternBuilder(LocalBattlePlayer.BattlePhase.BattleGrid.AllCellsCoordinates);

            SelectEntityPhase<DeployableCellFilter> selectEntityPhase = new SelectEntityPhase<DeployableCellFilter>(
                LocalBattlePlayer,
                new DeployableCellFilter(),
                patternBuilder,
                dragPhase);

            PhaseResult<EntityAddress[]> result = await selectEntityPhase;


            if (result.type == PhaseResultType.Cancel)
                throw new OperationCanceledException(token);

            if (result.type != PhaseResultType.Success)
                return;

            if(result.value.Length == 0)
                return;

            var address = result.value[0];
            if (!address.Is(out GridMemberAspect aspect))
                return ;

            int cardID = LocalBattlePlayer.Hand.GetCardIndex(battleCard);
            if (cardID == -1)
                return;

            DeployCardCommand deployCardCommand = new(cardID, aspect.Coordinates, LocalBattlePlayer.ID);
            await deployCardCommand.RunAsync(LocalBattlePlayer.BattlePhase);
        }
    }
}