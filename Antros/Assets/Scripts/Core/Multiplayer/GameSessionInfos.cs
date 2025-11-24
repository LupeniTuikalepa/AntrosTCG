using System;
using System.Collections.Generic;
using Unity.Services.Multiplayer;

namespace ATCG.Multiplayer
{
    public class GameSessionInfos : IGameSession
    {
        public readonly ISessionInfo session;

        public bool IsLocked => session.IsLocked;
        public bool HasPassword => session.HasPassword;
        public DateTime LastUpdated => session.LastUpdated;
        public string ID => session.Id;
        public string HostID => session.HostId;

        public IReadOnlyDictionary<string, SessionProperty> Properties => session.Properties;
        public int MaxPlayers => session.MaxPlayers;

        public int CurrentPlayers => session.MaxPlayers - session.AvailableSlots;


        public GameSessionInfos(ISessionInfo session)
        {
            this.session = session;
        }
    }
}