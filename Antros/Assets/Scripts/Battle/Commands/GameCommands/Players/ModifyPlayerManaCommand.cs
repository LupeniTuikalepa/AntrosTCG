using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands.Players
{
    [Serializable]
    public class ModifyPlayerManaCommand : GameCommand<ModifyPlayerManaCommand.Infos>
    {
        public struct Infos
        {
            public int fromMana;
            public int toMana;
            public int maxMana;
        }

        [field: SerializeField]
        public int PlayerId { get; private set; }
        [field: SerializeField]
        public int Amount { get; private set; }

        public ModifyPlayerManaCommand(int playerId, int amount)
        {
            PlayerId = playerId;
            Amount = amount;
        }

        protected override void Process(in GameCommandContext context)
        {
            IBattlePlayer player = context.GetBattlePlayer(PlayerId);

            infos.maxMana = player.MaxMana;
            infos.fromMana = player.CurrentMana;

            player.AddOrRemoveMana(Amount);

            infos.toMana = player.CurrentMana;
        }
    }
}