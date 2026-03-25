using System;
using System.Threading;
using System.Threading.Tasks;
using ATCG.Battle.Actions;
using ATCG.Battle.Entities.Core;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.HexGrids.Grids;
using ATCG.Metrics;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace ATCG.Battle
{
    public class BattlePhase : IPhase<BattleHistory>
    {
        public readonly int seed;

        public uint CellRadius => GameMetrics.Current.CellRadius;
        public uint GridRadius => GameMetrics.Current.GridRadius;


        public readonly IBattlePlayerProfile[] playerProfiles;

        public BattleGrid BattleGrid { get; private set; }

        public IBattlePlayer[] Players { get; private set; }

        public int Round { get; private set; }
        public int Turn { get; private set; }

        public int CurrentPlayerID { get; private set; }

        public IBattlePlayer CurrentPlayer { get; private set; }

        public HexGrid HexGrid => BattleGrid.Grid;
        public int PlayerCount => playerProfiles.Length;

        public World World { get; }

        public BattlePhase(int seed, params IBattlePlayerProfile[] playerProfiles)
        {
            World = new World(maxComponentStores: 128, maxEntities: 128);

            this.seed = seed;
            this.playerProfiles = playerProfiles;
        }

        async Awaitable IPhase<BattleHistory>.Initialize(CancellationToken token)
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

            BattleGrid = new BattleGrid(CellRadius, GridRadius);
        }


        async Awaitable<BattleHistory> IPhase<BattleHistory>.Execute(CancellationToken token)
        {
            await Awaitable.EndOfFrameAsync(token);
            BattleHistory history = new BattleHistory(seed);
            Round = 1;
            Turn = 1;
            while (true)
            {
                bool isGameDone = false;
                for (int i = 0; i < Players.Length; i++)
                {
                    CurrentPlayer = Players[i];

                    BattleTurn turn =  await CurrentPlayer.PlayTurn(Round, Turn);
                    history.RegisterTurn(turn);
                    Turn++;

                    if (!IsGameDone(ref history))
                        continue;

                    isGameDone = true;
                    break;
                }

                if(isGameDone)
                    break;

                Round++;
            }

            return history;
        }

        async Awaitable IPhase<BattleHistory>.Dispose(CancellationToken token)
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
            ((IDisposable)BattleGrid).Dispose();

            await Task.CompletedTask;
        }

        protected virtual bool IsGameDone(ref BattleHistory history)
        {
            using (ListPool<IBattlePlayer>.Get(out var winningPlayers))
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