using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Players.Runtime;
using ATCG.Cards;
using ATCG.Players;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Random = UnityEngine.Random;

namespace ATCG.Battle.Players
{
    public class RuntimeBattlePlayerManager : MonoPhaseListener<BattleGameMode>
    {
        [Header("References")]
        [SerializeField, ChildGameObjectsOnly]
        private Transform container;
        [SerializeReference]
        private IRuntimePlayerFactory[] factories;

        private Dictionary<IBattlePlayer, RuntimeBattlePlayer> runtimeBattlePlayers;

        private void Awake()
        {
            runtimeBattlePlayers = new Dictionary<IBattlePlayer, RuntimeBattlePlayer>();
        }

        protected override void OnPhaseBegin(BattleGameMode phase)
        {
            base.OnPhaseBegin(phase);

            runtimeBattlePlayers.Clear();
            container.ClearChildren();

            for (int i = 0; i < phase.Players.Length; i++)
            {
                IBattlePlayer player = phase.Players[i];
                for (int j = 0; j < factories.Length; j++)
                {
                    var factory = factories[j];
                    if (factory.TryCreateRuntimeFor(player, out RuntimeBattlePlayer runtimeBattlePlayer))
                    {
                        runtimeBattlePlayers.Add(player, runtimeBattlePlayer);
                        runtimeBattlePlayer.transform.SetParent(container);
                        break;
                    }
                }
            }
        }

        protected override void OnPhaseEnd(BattleGameMode phase)
        {
            foreach ((IBattlePlayer battlePlayer, RuntimeBattlePlayer runtimeBattlePlayer) in runtimeBattlePlayers)
                runtimeBattlePlayer.Disconnect();

            runtimeBattlePlayers.Clear();

            container.ClearChildren();
            base.OnPhaseEnd(phase);
        }
    }
}