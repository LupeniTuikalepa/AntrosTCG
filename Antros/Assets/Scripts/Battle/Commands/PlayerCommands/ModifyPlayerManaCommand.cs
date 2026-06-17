using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands.Players
{
    [Serializable]
    public class ModifyPlayerManaCommand : PlayerCommand<DeltaInRangeInfos<int>>
    {

        [field: SerializeField]
        public int Amount { get; private set; }

        public ModifyPlayerManaCommand(IBattlePlayer battlePlayer, int amount) : base(battlePlayer)
        {
            Amount = amount;
        }

        protected override void Process(in CommandContext context)
        {
            IBattlePlayer player = GetPlayer(in context);
            infos.max = player.MaxMana;
            infos.from = player.CurrentMana;

            player.AddOrRemoveMana(Amount);

            infos.to = player.CurrentMana;
        }
    }
}