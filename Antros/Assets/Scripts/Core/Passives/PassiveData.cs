using ATCG.Capacities;
using ATCG.Databases;
using ATCG.Enums;
using Helteix.Tools.DataMapping;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Passives.Datas
{
    public abstract class PassiveData : GameDatabaseObject, IData, IAbility
    {
        [field: SerializeField, BoxGroup("Base")]
        public string Name { get; private set; }

        [field: SerializeField, BoxGroup("Base")]
        public Element Element { get; private set; }

        [field: SerializeField, TextArea, BoxGroup("Base")]
        public string Description { get; private set; }
        
        [field: SerializeField, BoxGroup("Base")]
        public bool ActiveOnSpawn { get; private set; } = true;
        
        [field: SerializeField, BoxGroup("Runtime")]
        public GameObject RuntimePassive { get; private set; }

    }
}