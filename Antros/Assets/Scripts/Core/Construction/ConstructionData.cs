using ATCG.Databases;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Construction
{
    public abstract class ConstructionData : GameDatabaseObject, IData
    {
        [field: SerializeField]
        public GameObject Prefab { get; private set; }
    }
}