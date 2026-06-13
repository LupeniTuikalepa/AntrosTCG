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
                if (address.IsBattleCellAspect(out BattleCellAspect aspect))
                {
                    return !aspect.GetMembers().Any();
                }

                return false;
            }
        }

        private readonly IBattleCard battleCard;
        private readonly CardDragPhase<IBattleCard> phase;

        public DeployCardPhase(LocalBattlePlayer localBattlePlayer, IBattleCard battleCard, CardDragPhase<IBattleCard> phase) : base(localBattlePlayer)
        {
            this.battleCard = battleCard;
            this.phase = phase;
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            SelectEntityPhase<DeployableCellFilter> selectEntityPhase = new SelectEntityPhase<DeployableCellFilter>(LocalBattlePlayer, new DeployableCellFilter(), phase);

            PhaseResult<EntityAddress[]> result = await selectEntityPhase;


            if (result.type == PhaseResultType.Cancel)
                throw new OperationCanceledException(token);

            if (result.type != PhaseResultType.Success)
                return;

            if(result.value.Length == 0)
                return;

            var address = result.value[0];
            if (!address.IsGridMemberAspect(out GridMemberAspect aspect))
                return ;

            int cardID = LocalBattlePlayer.Hand.GetCardIndex(battleCard);
            if (cardID == -1)
                return;

            DeployCardCommand deployCardCommand = new(cardID, aspect.Coordinates, LocalBattlePlayer.ID);
            deployCardCommand.RunAndForget(LocalBattlePlayer.BattlePhase);
        }
    }
}