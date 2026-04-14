using System.Collections.Generic;
using ATCG.Battle.GameModes;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.Players.Local.UI.Stats
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/OpponentPlayersStats")]
    public class OpponentPlayersStats : PlayerHUDElement
    {
        [SerializeField]
        private SimplePlayerStats simplePlayerStatsPrefab;

        [SerializeField]
        private RectTransform container;

        private Dictionary<IBattlePlayer, SimplePlayerStats> simplePlayerStats;
        public BattlePhase CurrentPhase { get; private set; }

        private void Awake()
        {
            simplePlayerStats = new Dictionary<IBattlePlayer, SimplePlayerStats>();
            Clear();
        }


        protected override void OnConnect()
        {
            CurrentPhase = Player.BattlePhase;
            for (int i = 0; i < CurrentPhase.PlayerCount; i++)
            {
                IBattlePlayer player = CurrentPhase.Players[i];
                if (player == Player)
                    continue;

                SimplePlayerStats instance = simplePlayerStatsPrefab.InstantiatePrefab(container);
                simplePlayerStats.Add(player, instance);

                instance.Connect(player);

                LayoutRebuilder.ForceRebuildLayoutImmediate(container);
            }
        }

        protected override void OnDisconnect()
        {
            Clear();
        }

        private void Clear()
        {
            foreach ((IBattlePlayer battlePlayer, SimplePlayerStats stats) in simplePlayerStats)
                stats.Disconnect(battlePlayer);

            container.ClearChildren();
            simplePlayerStats.Clear();
        }
    }
}