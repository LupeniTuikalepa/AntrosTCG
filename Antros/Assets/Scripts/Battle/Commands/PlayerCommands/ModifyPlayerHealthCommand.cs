using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands.Players
{
    [Serializable]
    public class ModifyPlayerHealthCommand : PlayerCommand<DeltaInRangeInfos<int>>
    {
        [field: SerializeField]
        public int Amount { get; private set; }

        public ModifyPlayerHealthCommand(IBattlePlayer player, int amount) : base(player)
        {
            Amount = amount;
        }

        protected override void Process(in CommandContext context)
        {
            IBattlePlayer player = GetPlayer(in context);
            infos.max = player.MaxHealth;
            infos.from = player.CurrentHealth;

            player.AddOrRemoveHealth(Amount);

            infos.to = player.CurrentHealth;
        }
    }
}