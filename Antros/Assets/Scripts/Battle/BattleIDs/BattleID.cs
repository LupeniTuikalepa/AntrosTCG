using System;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
    [System.Serializable]
    public struct BattleID : IEquatable<BattleID>
    {
        public static BattleID None { get; } = new BattleID(null);

        [SerializeField]
        private string id;

        public bool IsValid => id != null;
        public BattleID(string id)
        {
            this.id = id;
        }

        public static BattleID CreateNew() => new BattleID(Guid.NewGuid().ToString());

        public static bool operator ==(BattleID a, BattleID b)
        {
            return a.id == b.id;
        }

        public static bool operator !=(BattleID a, BattleID b)
        {
            return !(a == b);
        }

        public bool Equals(BattleID other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (id != null ? id.GetHashCode() : 0);
        }


        public override string ToString() => id;
    }
}