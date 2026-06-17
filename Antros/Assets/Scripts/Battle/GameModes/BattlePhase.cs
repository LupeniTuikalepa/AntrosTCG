using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
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
        public IEnumerable<IBattlePlayer> Players => players.Values;
        public IBattlePlayer CurrentPlayer => players[CurrentPlayerID];

        public BattleGrid BattleGrid { get; private set; }

        public BattleID CurrentPlayerID { get; private set; }
        public int Round { get; private set; }
        public int Turn { get; private set; }

        private Dictionary<BattleID, IBattlePlayer> players;

        public readonly IBattlePlayerProfile[] playerProfiles;
        public readonly World world;
        public readonly int seed;


        public BattlePhase(int seed, params IBattlePlayerProfile[] playerProfiles)
        {
            world = new World(maxComponentStores: 128, maxEntities: 128);

            this.seed = seed;
            this.playerProfiles = playerProfiles;
            players = new Dictionary<BattleID, IBattlePlayer>();
        }

        protected override async Awaitable Initialize(CancellationToken token)
        {
            Random.InitState(seed);
            SceneReference gameScene = GameScenes.Current.Game;
            await GameController.GameSceneController.LoadScenesWithLoadingScreen(gameScene);

            players = DictionaryPool<BattleID, IBattlePlayer>.Get();
            for (int i = 0; i < playerProfiles.Length; i++)
            {
                IBattlePlayerProfile playerProfile = playerProfiles[i];

                IBattlePlayer battlePlayer = playerProfile.Convert(this);
                battlePlayer.OnBattleBegins(this);
                players.Add(battlePlayer.GetBattleID(), battlePlayer);
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
                foreach ((BattleID battleID, IBattlePlayer battlePlayer) in players)
                {
                    CurrentPlayerID = battleID;

                    BattleTurn turn = await battlePlayer.PlayTurn(Round, Turn);
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
            DictionaryPool<BattleID, IBattlePlayer>.Release(players);

            //"reset" of seed
            // (BattleGrid as IDisposable).Dispose();
            await Task.CompletedTask;
        }

        public int GetPlayerNumber(IBattlePlayer player) => GetPlayerNumber(player.GetBattleID());

        public int GetPlayerNumber(BattleID playerID)
        {
            int number = 0;
            foreach ((BattleID battleID, _) in players)
            {
                if (playerID == battleID)
                    return number;

                number++;
            }

            return -1;
        }
        public IBattlePlayer GetPlayer(BattleID playerID) => players[playerID];

        public bool TryGetPlayer(BattleID playerID, out IBattlePlayer player) => players.TryGetValue(playerID, out player);

        protected virtual bool IsGameDone(ref BattleHistory history)
        {
            using (ListPool<IBattlePlayer>.Get(out List<IBattlePlayer> winningPlayers))
            {
                winningPlayers.AddRange(players.Values);

                foreach ((BattleID battleID, IBattlePlayer player) in players)
                {
                    if (player.IsDefeated())
                        winningPlayers.Remove(player);
                }

                if (winningPlayers.Count == 1)
                {
                    history.SetWinningPlayer(winningPlayers[0].GetBattleID());
                    return true;
                }


                history.SetWinningPlayer(BattleID.None);
                return false;
            }
        }

    }
}