using System;
using System.Threading;
using ATCG.Battle.Actions;
using ATCG.Battle.HexGrids;
using ATCG.Battle.Metrics;
using ATCG.Battle.Players;
using ATCG.GameModes;
using ATCG.HexGrids.Grids;
using Eflatun.SceneReference;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ATCG.Battle
{
    public abstract class BattleGameMode : GameMode<BattleGameModeResults>
    {
        public uint CellRadius => GameplayMetrics.Current.CellRadius;
        public uint GridRadius => GameplayMetrics.Current.GridRadius;

        public readonly int seed;

        public readonly BattlePlayerProfile[] playerProfiles;


        public BattleGameModeResults Results { get; private set; }
        public BattleGrid BattleGrid { get; private set; }
        public IBattlePlayer[] Players { get; private set; }

        public int Round { get; private set; }
        public int Turn { get; private set; }

        public int CurrentPlayerID { get; private set; }
        public IBattlePlayer CurrentPlayer { get; private set; }

        public abstract string ID { get; }

        public HexGrid HexGrid => BattleGrid.Grid;
        protected BattleGameMode(int seed, params BattlePlayerProfile[] playerProfiles)
        {
            this.seed = seed;
            this.playerProfiles = playerProfiles;
        }


        protected override async Awaitable Initialize()
        {
            SceneReference gameScene = GetGameScene();
            Random.InitState(seed);

            Scene mainScene = await GameController.GameSceneController.LoadScenesWithLoadingScreen(gameScene);
            await Awaitable.MainThreadAsync();

            Players = new IBattlePlayer[playerProfiles.Length];
            for (int i = 0; i < playerProfiles.Length; i++)
            {
                IBattlePlayer battlePlayer = CreatePlayer(playerProfiles[i]);

                Players[i] = battlePlayer;
            }

            BattleGrid = new BattleGrid(CellRadius, GridRadius);
        }


        protected override async Awaitable<BattleGameModeResults> Execute(CancellationToken token)
        {
            BattleHistory history = new BattleHistory(seed, ID);
            Round = 1;
            Turn = 1;
            BattleGameModeResults results = default;

            while (true)
            {
                bool isGameDone = false;
                for (int i = 0; i < Players.Length; i++)
                {
                    CurrentPlayer = Players[i];

                    BattleTurn turn =  await CurrentPlayer.PlayTurn(Round, Turn);
                    history.RegisterTurn(turn);
                    Turn++;

                    if (!IsGameDone(out results))
                        continue;

                    isGameDone = true;
                    break;
                }

                if(isGameDone)
                    break;

                Round++;
            }

            results.history = history;
            return results;
        }

        protected override Awaitable Dispose()
        {
            //"reset" of seed
            Random.InitState(DateTime.Today.GetHashCode());
            ((IDisposable)BattleGrid).Dispose();

            return Awaitables.CompletedAwaitable;
        }

        protected virtual bool IsGameDone(out BattleGameModeResults battleGameModeResults)
        {
            using (ListPool<IBattlePlayer>.Get(out var winningPlayers))
            {
                for (int i = 0; i < Players.Length; i++)
                {
                    IBattlePlayer player = Players[i];
                    if (!player.IsDefeated())
                        winningPlayers.Add(player);
                }

                switch (winningPlayers.Count)
                {
                    case 0:
                        battleGameModeResults = new BattleGameModeResults()
                        {
                            winningPlayerID = -1,
                        };
                        return true;
                    case 1:
                        battleGameModeResults = new BattleGameModeResults()
                        {
                            winningPlayerID = winningPlayers[0].Profile.id,
                        };
                        return true;
                    default:
                        battleGameModeResults = default;
                        return false;
                }
            }
        }
        protected abstract IBattlePlayer CreatePlayer(BattlePlayerProfile playerProfile);
        protected abstract SceneReference GetGameScene();
    }
}