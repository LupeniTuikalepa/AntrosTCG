using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace ATCG.Multiplayer
{
    public class JoinedGameSession : IGameSession
    {
        public readonly ISession session;

        public string Code => session.Code;
        public bool IsLocked => session.IsLocked;
        public bool HasPassword => session.HasPassword;

        public string ID => session.Id;
        public string HostID => session.Host;

        public int MaxPlayers => session.MaxPlayers;
        public int CurrentPlayers => session.PlayerCount;

        public IReadOnlyDictionary<string, SessionProperty> Properties => session.Properties;

        public JoinedGameSession(ISession session)
        {
            this.session = session;
        }

        public async Awaitable Leave()
        {
            await session.LeaveAsync();
        }
    }
}