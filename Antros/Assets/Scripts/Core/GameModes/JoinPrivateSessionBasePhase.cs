using System.Threading;
using System.Threading.Tasks;
using Helteix.Tools.Phases;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace ATCG.GameModes
{
    public class JoinPrivateSessionPhase : Phase<ISession>
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


        protected override async Awaitable<ISession> Execute(CancellationToken token)
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
    }
}