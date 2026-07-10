using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Commands.Players;
using ATCG.UI;
using UnityEngine;

namespace ATCG.Battle.Players.UI
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/PlayerManaBar")]
    public class PlayerManaBar : BarUI, IPlayerStatUI, IPlayerCommandListener<ModifyPlayerManaCommand>
    {
        public IBattlePlayer BattlePlayer { get; private set; }


        public void Connect(IBattlePlayer player)
        {
            CurrentValue = player.CurrentHealth;
            MaxValue = player.MaxHealth;

            Refresh();
        }

        public void Disconnect(IBattlePlayer player)
        {
            BattlePlayer = null;
        }

        public async Awaitable Play(CommandListenerState state, CommandContext context, ModifyPlayerManaCommand command)
        {
            //No need to wait what follows
            state.CompleteAll(this);

            DeltaInRangeInfos<int> infos = command.GetInfos();
            MaxValue = infos.from;
            CurrentValue = infos.to;

            await RefreshAsync();
        }
    }
}