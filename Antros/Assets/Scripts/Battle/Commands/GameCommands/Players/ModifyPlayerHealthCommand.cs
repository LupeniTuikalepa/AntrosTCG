using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands.Players
{
    [Serializable]
    public class ModifyPlayerHealthCommand : GameCommand
    {
        [field: SerializeField]
        public int PlayerId { get; private set; }

        [field: SerializeField]
        public int Amount { get; private set; }

        public ModifyPlayerHealthCommand(int playerId, int amount)
        {
            PlayerId = playerId;
            Amount = amount;
        }

        protected override void Execute(in GameCommandContext context)
        {
            IBattlePlayer player = context.GetPlayer(PlayerId);
            player.AddOrRemoveHealth(Amount);
        }
    }
}