using System;
using ATCG.Databases;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Capacities
{
    [Serializable]
    public abstract class DeployableData : GameDatabaseObject, IData
    {
        [field: SerializeField]
        public GameObject Prefab { get; private set; }
    }
}