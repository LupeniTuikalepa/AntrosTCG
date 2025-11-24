using System.Collections.Generic;
using Unity.Services.Multiplayer;

namespace ATCG.Multiplayer
{
    public interface IGameSession
    {
        public bool IsLocked { get; }
        public bool HasPassword { get; }
        public string ID { get; }
        public string HostID { get; }
        public IReadOnlyDictionary<string, SessionProperty> Properties { get; }

        public int MaxPlayers { get; }
        public int CurrentPlayers { get; }
    }
}