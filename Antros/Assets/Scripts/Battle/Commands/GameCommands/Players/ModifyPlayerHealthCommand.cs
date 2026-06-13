using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands.Players
{
    [Serializable]
    public class ModifyPlayerHealthCommand : GameCommand<ModifyPlayerHealthCommand.Infos>
    {
        public struct Infos
        {
            public int from;
            public int to;
        }
        
        [field: SerializeField]
        public int PlayerId { get; private set; }

        [field: SerializeField]
        public int Amount { get; private set; }

        public ModifyPlayerHealthCommand(int playerId, int amount)
        {
            PlayerId = playerId;
            Amount = amount;
        }

        protected override void Process(in CommandContext context)
        {
            IBattlePlayer player = context.GetBattlePlayer(PlayerId);
            infos.from = player.CurrentHealth;

            player.AddOrRemoveHealth(Amount);

            infos.to = player.CurrentHealth;
        }
    }
}