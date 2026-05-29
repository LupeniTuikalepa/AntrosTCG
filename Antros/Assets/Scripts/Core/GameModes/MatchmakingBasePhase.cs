using System;
using System.Threading;
using System.Threading.Tasks;
using Helteix.Tools.Phases;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace ATCG.GameModes
{
    public class MatchmakingPhase : Phase<ISession>
    {
        public string GameModeQueue { get; }
        public int MaxPlayers { get; }

        public MatchmakingPhase(string gameModeQueue, int maxPlayers)
        {
            GameModeQueue = gameModeQueue;
            MaxPlayers = maxPlayers;
        }


        protected override async Awaitable<ISession> Execute(CancellationToken token)
        {
            QuickJoinOptions quickJoinOptions = new QuickJoinOptions()
            {
                CreateSession = true,
                Timeout = TimeSpan.FromSeconds(5),
            };
            var sessionOptions = new SessionOptions()
            {
                MaxPlayers = MaxPlayers,
                IsLocked = false,
                IsPrivate = false,
                SessionProperties =
                {
                    { IGameMode.SESSION_GAME_MODE, new SessionProperty(GameModeQueue) },
                }
            };

            return await MultiplayerService.Instance.MatchmakeSessionAsync(quickJoinOptions, sessionOptions);
        }

        protected override async Awaitable Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        protected override async Awaitable Dispose(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}