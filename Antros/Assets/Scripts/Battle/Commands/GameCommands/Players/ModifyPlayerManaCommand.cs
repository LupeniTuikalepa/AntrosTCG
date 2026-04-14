using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Players;
using ATCG.Metrics;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands.Players
{
    [Serializable]
    public class ModifyPlayerManaCommand : GameCommand
    {
        [field: SerializeField]
        public int PlayerId { get; private set; }
        [field: SerializeField]
        public int Amount { get; private set; }

        public ModifyPlayerManaCommand(int playerId, int amount)
        {
            PlayerId = playerId;
            Amount = amount;
        }

        protected override void Execute(in GameCommandContext context)
        {
            IBattlePlayer player = context.GetPlayer(PlayerId);
            player.AddOrRemoveMana(Amount);
        }
    }
}