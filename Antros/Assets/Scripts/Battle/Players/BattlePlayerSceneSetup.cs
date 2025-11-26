using System;
using System.Collections.Generic;
using ATCG.Battle.Players;
using Helteix.Tools;
using Helteix.Tools.Phases.Listeners;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle
{
    public class BattlePlayerSceneSetup : MonoPhaseListener<BattleGameMode>
    {
        [SerializeField, AssetsOnly]
        private RuntimeBattlePlayer[] playersPrefab;

        [SerializeField, ChildGameObjectsOnly]
        private Transform container;

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
                var player = phase.Players[i];
                for (int j = 0; j < playersPrefab.Length; j++)
                {
                    RuntimeBattlePlayer prefab = playersPrefab[j];

                    if (!prefab.IsCompatibleWith(player))
                        continue;

                    RuntimeBattlePlayer instance = Instantiate(prefab, container);
                    instance.Connect(player);

                    runtimeBattlePlayers.Add(player, instance);
                    break;
                }
            }
        }

        protected override void OnPhaseEnd(BattleGameMode phase)
        {
            foreach ((IBattlePlayer battlePlayer, RuntimeBattlePlayer runtimeBattlePlayer) in runtimeBattlePlayers)
            {
                runtimeBattlePlayer.Disconnect(battlePlayer);
            }

            runtimeBattlePlayers.Clear();

            container.ClearChildren();
            base.OnPhaseEnd(phase);
        }
    }
}