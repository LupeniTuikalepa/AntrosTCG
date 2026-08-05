using System;
using Object = UnityEngine.Object;

namespace ATCG.Battle.Entities.Commands
{
    public readonly struct CLCKey : IEquatable<CLCKey>
    {
        public bool IsValid => key != null;
        
        private readonly string key;
        
        public CLCKey(string key)
        {
            this.key = key;
        }

        public static implicit operator CLCKey(string key) => new (key);
        
        public static implicit operator string(CLCKey key) => key.key;
        
        public static implicit operator CLCKey(EntityAddress address) => new ($"Entity_{address.entity.ToString()}");

        public static implicit operator CLCKey(Object obj) => new($"Object_{obj.name}_{obj.GetEntityId()}");
        
        public bool Equals(CLCKey other)
        {
            return key == other.key;
        }

        public override bool Equals(object obj)
        {
            return obj is CLCKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (key != null ? key.GetHashCode() : 0);
        }
    }
}