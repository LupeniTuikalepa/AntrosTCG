using ATCG.Databases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities
{

    public abstract class CapacityData : GameDatabaseObject
    {
        [field: SerializeField, PropertyRange(0, 10)]
        public int Cost { get; private set; }

        [field: SerializeField]
        public string Name { get; private set;}

        [field: SerializeField, TextArea]
        public string Description { get; private set;}
    }
}