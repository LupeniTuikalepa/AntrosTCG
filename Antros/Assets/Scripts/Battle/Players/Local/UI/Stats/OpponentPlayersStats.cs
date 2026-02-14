using System.Collections.Generic;
using ATCG.GameModes;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.Players.Runtime.UI
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/OpponentPlayersStats")]
    public class OpponentPlayersStats : PlayerHUDElement
    {
        public BattleGameMode CurrentPhase { get; private set; }

        [SerializeField]
        private SimplePlayerStats simplePlayerStatsPrefab;
        [SerializeField]
        private RectTransform container;

        private Dictionary<IBattlePlayer, SimplePlayerStats> simplePlayerStats;

        private void Awake()
        {
            simplePlayerStats = new Dictionary<IBattlePlayer, SimplePlayerStats>();
            Clear();
        }


        protected override void OnConnect()
        {/*
            if (GameModeController.Global.Current is BattleGameMode bgm)
                CurrentPhase = bgm;
            else
            {
            */
                CurrentPhase = null;
                return;
            /*}
            for (int i = 0; i < CurrentPhase.PlayerCount; i++)
            {
                IBattlePlayer player = CurrentPhase.Players[i];
                if(player == Player)
                    continue;

                SimplePlayerStats instance = simplePlayerStatsPrefab.InstantiatePrefab(container);
                simplePlayerStats.Add(player, instance);

                instance.Connect(player);

                LayoutRebuilder.ForceRebuildLayoutImmediate(container);
            }*/
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