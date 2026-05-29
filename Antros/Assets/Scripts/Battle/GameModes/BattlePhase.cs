using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Turns;
using ATCG.HexGrids.Grids;
using ATCG.Metrics;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace ATCG.Battle.GameModes
{
    public class BattlePhase : Phase<BattleHistory>
    {
        public uint CellRadius => GameMetrics.Current.CellRadius;
        public uint GridRadius => GameMetrics.Current.GridRadius;

        public HexGrid HexGrid => BattleGrid.grid;
        public int PlayerCount => playerProfiles.Length;

        public BattleGrid BattleGrid { get; private set; }
        public IBattlePlayer[] Players { get; private set; }
        public IBattlePlayer CurrentPlayer { get; private set; }
        public int CurrentPlayerID => CurrentPlayer.Profile.ID;

        public int Round { get; private set; }
        public int Turn { get; private set; }

        public readonly IBattlePlayerProfile[] playerProfiles;
        public readonly World world;
        public readonly int seed;

        public BattlePhase(int seed, params IBattlePlayerProfile[] playerProfiles)
        {
            world = new World(maxComponentStores: 128, maxEntities: 128);

            this.seed = seed;
            this.playerProfiles = playerProfiles;
        }

        protected override async Awaitable Initialize(CancellationToken token)
        {
            Random.InitState(seed);
            SceneReference gameScene = GameScenes.Current.Game;
            await GameController.GameSceneController.LoadScenesWithLoadingScreen(gameScene);

            Players = new IBattlePlayer[playerProfiles.Length];
            for (int i = 0; i < playerProfiles.Length; i++)
            {
                IBattlePlayerProfile playerProfile = playerProfiles[i];

                IBattlePlayer battlePlayer = playerProfile.Convert(this);
                battlePlayer.OnBattleBegins(this);
                Players[i] = battlePlayer;
            }

            BattleGrid = new BattleGrid(this, CellRadius, GridRadius);
        }


        protected override async Awaitable<BattleHistory> Execute(CancellationToken token)
        {
            await Awaitable.EndOfFrameAsync(token);
            BattleHistory history = new(seed);
            Round = 1;
            Turn = 1;
            while (true)
            {
                bool isGameDone = false;
                for (int i = 0; i < Players.Length; i++)
                {
                    CurrentPlayer = Players[i];

                    BattleTurn turn = await CurrentPlayer.PlayTurn(Round, Turn);
                    history.RegisterTurn(turn);
                    Turn++;

                    if (!IsGameDone(ref history))
                        continue;

                    isGameDone = true;
                    break;
                }

                if (isGameDone)
                    break;

                Round++;
            }

            return history;
        }

        protected override async Awaitable Dispose(CancellationToken token)
        {
            //"reset" of seed
            Random.InitState(DateTime.Today.GetHashCode());
            for (int i = 0; i < playerProfiles.Length; i++)
            {
                IBattlePlayerProfile playerProfile = playerProfiles[i];

                IBattlePlayer battlePlayer = playerProfile.Convert(this);
                battlePlayer.OnBattleEnds(this);
                Players[i] = battlePlayer;
            }

            // (BattleGrid as IDisposable).Dispose();
            await Task.CompletedTask;
        }

        public IBattlePlayer GetPlayer(int playerID) => Players[playerID];

        protected virtual bool IsGameDone(ref BattleHistory history)
        {
            using (ListPool<IBattlePlayer>.Get(out List<IBattlePlayer> winningPlayers))
            {
                winningPlayers.AddRange(Players);
                for (int i = 0; i < Players.Length; i++)
                {
                    IBattlePlayer player = Players[i];
                    if (player.IsDefeated())
                        winningPlayers.Remove(player);
                }

                if (winningPlayers.Count == 1)
                {
                    history.SetWinningPlayer(winningPlayers[0].Profile.ID);
                    return true;
                }

                if (winningPlayers.Count == 0)
                {
                    history.SetWinningPlayer(-1);
                    return true;
                }

                history.SetWinningPlayer(-2);
                return false;
            }
        }
    }
}