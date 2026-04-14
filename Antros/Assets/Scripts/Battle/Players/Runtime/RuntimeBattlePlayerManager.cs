using System.Collections.Generic;
using ATCG.Battle.GameModes;
using Helteix.Tools;
using Helteix.Tools.Phases.Listeners;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Players.Runtime
{
    [AddComponentMenu("ATCG/Gameplay/Player/Runtime/PlayerManager")]
    public class RuntimeBattlePlayerManager : MonoPhaseListener<BattlePhase>
    {
        [Header("References"), SerializeField, ChildGameObjectsOnly]
        private Transform container;

        [SerializeReference]
        private IRuntimePlayerFactory[] factories;

        private Dictionary<IBattlePlayer, RuntimeBattlePlayer> runtimeBattlePlayers;

        private void Awake()
        {
            runtimeBattlePlayers = new Dictionary<IBattlePlayer, RuntimeBattlePlayer>();
        }


        protected override void OnPhaseBegin(BattlePhase phase)
        {
            base.OnPhaseBegin(phase);

            runtimeBattlePlayers.Clear();
            container.ClearChildren();

            for (int i = 0; i < phase.Players.Length; i++)
            {
                IBattlePlayer player = phase.Players[i];
                for (int j = 0; j < factories.Length; j++)
                {
                    IRuntimePlayerFactory factory = factories[j];
                    if (factory.TryCreateRuntimeFor(player, out RuntimeBattlePlayer runtimeBattlePlayer))
                    {
                        runtimeBattlePlayers.Add(player, runtimeBattlePlayer);
                        runtimeBattlePlayer.transform.SetParent(container);
                        break;
                    }
                }
            }
        }

        protected override void OnPhaseEnd(BattlePhase phase)
        {
            foreach ((IBattlePlayer battlePlayer, RuntimeBattlePlayer runtimeBattlePlayer) in runtimeBattlePlayers)
                runtimeBattlePlayer.Disconnect();

            runtimeBattlePlayers.Clear();

            container.ClearChildren();
            base.OnPhaseEnd(phase);
        }
    }
}