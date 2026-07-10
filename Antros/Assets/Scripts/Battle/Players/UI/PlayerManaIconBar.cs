using System;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using Helteix.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.UI
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/PlayerManaIconBar")]
    public class PlayerManaIconBar : MonoBehaviour, IPlayerStatUI, ICommandListener<ModifyPlayerManaCommand>
    {
        [SerializeField]
        private TMP_Text valueText;

        [SerializeField]
        private PlayerManaIcon iconPrefab;

        [SerializeField]
        private Transform container;

        private PlayerManaIcon[] manaIcons;

        private void OnEnable()
        {
            this.RegisterListener();
        }

        private void OnDisable()
        {
            this.UnregisterListener();
        }


        private void EnsureMaxMana(int max)
        {
            if (manaIcons == null)
            {
                manaIcons = new PlayerManaIcon[max];

                for (int i = 0; i < max; i++)
                    manaIcons[i] = iconPrefab.InstantiatePrefab(container);
            }

            if (manaIcons.Length == max)
                return;

            using (ListPool<PlayerManaIcon>.Get(out var list))
            {
                list.AddRange(manaIcons);
                if (manaIcons.Length > max)
                {
                    for (int i = max; i < manaIcons.Length; i++)
                    {
                        var icon = manaIcons[i];
                        Destroy(icon.gameObject);
                        list.Remove(icon);
                    }
                }
                else
                {
                    for (int i = manaIcons.Length; i < max; i++)
                    {
                        var icon = iconPrefab.InstantiatePrefab(container);
                        list.Add(icon);
                    }
                }

                manaIcons = list.ToArray();
            }
        }

        public void Connect(IBattlePlayer player)
        {
            container.ClearChildren();

            EnsureMaxMana(player.MaxMana);
            Refresh(player.MaxMana, player.CurrentMana, player.CurrentMana);
        }

        public void Disconnect(IBattlePlayer player)
        {
            EnsureMaxMana(player.MaxMana);
            Refresh(player.MaxMana, player.CurrentMana, player.CurrentMana);
        }

        public async Awaitable Play(CommandContext context, ModifyPlayerManaCommand command)
        {
            await Awaitable.MainThreadAsync();
            DeltaInRangeInfos<int> infos = command.GetInfos();

            Refresh(infos.max, infos.to, infos.from);
        }

        private void Refresh(int max, int current, int last)
        {
            EnsureMaxMana(max);

            for (int i = 0; i < max; i++)
            {
                PlayerManaIcon icon = manaIcons[i];
                if (i < current)
                    icon.Activate();
                else
                    icon.Deactivate();
            }

            valueText.text = $"{current}/{max}";
        }

        public async Awaitable Play(CommandListenerState state, CommandContext context, ModifyPlayerManaCommand command)
        {
            state.CompleteWindUp(this);
            var infos = command.GetInfos();
            Refresh(infos.max, infos.to, infos.from);
            state.CompleteFollowThrough(this);

            await Awaitable.MainThreadAsync();
        }

    }
}