using System;
using System.Threading;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status;
using ATCG.Battle.CapacitySystem.Status.Berserk;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Implementations;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using ATCG.Battle.Turns;
using ATCG.Metrics;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases
{
    public class LocalPlayerTurnPhase : PlayerTurnPhase
    {
        private static readonly ChannelKey ChannelKey = ChannelKey.GetUniqueChannelKey("Turn phase");

        public readonly LocalBattlePlayer localPlayerTurn;
        public readonly string turnID;

        public LocalPlayerTurnPhase(int turnNumber, LocalBattlePlayer localPlayerTurn) : base(turnNumber,
            localPlayerTurn)
        {
            this.localPlayerTurn = localPlayerTurn;
            turnID = Guid.NewGuid().ToString();
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            using (CommandManager.BeginGroup("Begin turn"))
            {
                ModifyPlayerManaCommand modifyPlayerManaCommand = new ModifyPlayerManaCommand(localPlayerTurn, GameMetrics.Current.RecoveredManaOnTurnStart);
                modifyPlayerManaCommand.Run(localPlayerTurn.BattlePhase);

                FillPlayerHandCommand fillPlayerHandCommand = new FillPlayerHandCommand(localPlayerTurn);
                fillPlayerHandCommand.Run(localPlayerTurn.BattlePhase);

                localPlayerTurn.canDeployHeroes.AddCondition(ChannelKey);
                localPlayerTurn.canMoveHeroes.AddCondition(ChannelKey);
                localPlayerTurn.canUseHeroesAbilities.AddCondition(ChannelKey);
                localPlayerTurn.canDoBasicAttack.AddCondition(ChannelKey);

                BeginTurnCommand beginTurnCommand = new BeginTurnCommand(localPlayerTurn);
                beginTurnCommand.Run(localPlayerTurn.BattlePhase);
            }
            return base.Initialize(token);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            using (CommandManager.BeginGroup("End turn"))
            {
                EndTurnCommand endTurnCommand = new EndTurnCommand(localPlayerTurn);
                endTurnCommand.Run(localPlayerTurn.BattlePhase);

                localPlayerTurn.canDoBasicAttack.RemoveCondition(ChannelKey);
                localPlayerTurn.canDeployHeroes.RemoveCondition(ChannelKey);
                localPlayerTurn.canMoveHeroes.RemoveCondition(ChannelKey);
                localPlayerTurn.canUseHeroesAbilities.RemoveCondition(ChannelKey);
            }

            return base.Dispose(token);
        }

        public void EndTurn()
        {
            BattleTurn infos = new(turnID, localPlayerTurn.ID);
            SetResult(infos);
        }

        public void GiveUp()
        {
            Debug.Log("Giving up is not implemented yet");
        }
    }
}