using System;
using System.Threading;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace ATCG.GameModes
{
    public class MatchmakingPhase : ISessionSetupPhase
    {
        public string GameModeQueue { get; }
        public int MaxPlayers { get; }

        public MatchmakingPhase(string gameModeQueue, int maxPlayers)
        {
            GameModeQueue = gameModeQueue;
            MaxPlayers = maxPlayers;
        }

        async Awaitable<ISession> IPhase<ISession>.Execute(CancellationToken token)
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

            return await MultiplayerService.Instance.MatchmakeSessionAsync(quickJoinOptions, sessionOptions);;
        }

        Awaitable IPhase<ISession>.Initialize(CancellationToken token)
        {
            return Awaitables.CompletedAwaitable;
        }

        Awaitable IPhase<ISession>.Dispose(CancellationToken token)
        {
            return Awaitables.CompletedAwaitable;
        }
    }
}