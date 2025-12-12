using System.Collections.Generic;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Players.Runtime.UI
{
    public class OpponentPlayersStats : PlayerHUDElement, IPhaseListener<BattleGameMode>
    {
        public BattleGameMode CurrentPhase { get; private set; }

        [SerializeField]
        private SimplePlayerStats simplePlayerStatsPrefab;
        [SerializeField]
        private Transform container;

        private Dictionary<IBattlePlayer, SimplePlayerStats> simplePlayerStats;

        private void Awake()
        {
            simplePlayerStats = new Dictionary<IBattlePlayer, SimplePlayerStats>();
            Disconnect();
        }

        private void OnEnable()
        {
            this.Register(-10);
        }

        private void OnDisable()
        {
            this.Unregister();
        }

        protected override void OnConnect()
        {
            Disconnect();

            if(CurrentPhase == null)
                return;

            for (int i = 0; i < CurrentPhase.PlayerCount; i++)
            {
                IBattlePlayer player = CurrentPhase.Players[i];
                if(player == Player)
                    continue;

                SimplePlayerStats instance = simplePlayerStatsPrefab.InstantiatePrefab(container);
                simplePlayerStats.Add(player, instance);

                instance.Connect(player);
            }
        }

        protected override void OnDisconnect()
        {
            foreach ((IBattlePlayer battlePlayer, SimplePlayerStats stats) in simplePlayerStats)
                stats.Disconnect(battlePlayer);

            container.ClearChildren();
            simplePlayerStats.Clear();
        }

        void IPhaseListener<BattleGameMode>.OnPhaseBegin(BattleGameMode phase)
        {
            CurrentPhase = phase;
            Debug.Log("Phase Begin");
        }


        void IPhaseListener<BattleGameMode>.OnPhaseEnd(BattleGameMode phase)
        {
        }
    }
}