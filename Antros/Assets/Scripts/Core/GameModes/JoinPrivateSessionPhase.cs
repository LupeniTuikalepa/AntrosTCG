using System;
using System.Threading;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace ATCG.GameModes
{
    public class JoinPrivateSessionPhase : ISessionSetupPhase
    {
        private string SessionID { get; }
        public string GameMode { get; }
        public int MaxPlayers { get; }
        public string Password { get; }

        public JoinPrivateSessionPhase(string sessionID, string gameMode, int maxPlayers = 2, string password = null)
        {
            SessionID = sessionID;
            GameMode = gameMode;
            MaxPlayers = maxPlayers;
            Password = password;
        }

        async Awaitable<ISession> IPhase<ISession>.Execute(CancellationToken token)
        {
            SessionOptions sessionOptions = new()
            {
                MaxPlayers = MaxPlayers,
                IsLocked = false,
                Password = Password,
                Name = "Private session",
                IsPrivate = true,
                SessionProperties =
                {
                    { IGameMode.SESSION_GAME_MODE, new SessionProperty(GameMode) },
                }
            };

            return await MultiplayerService.Instance.CreateOrJoinSessionAsync(SessionID, sessionOptions);
        }

        Awaitable IBasePhase.Initialize(CancellationToken token)
        {
            return Awaitables.CompletedAwaitable;
        }

        Awaitable IBasePhase.Dispose(CancellationToken token)
        {
            return Awaitables.CompletedAwaitable;
        }
    }
}