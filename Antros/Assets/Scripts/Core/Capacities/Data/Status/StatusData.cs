using ATCG.Databases;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
    public abstract class StatusData : GameDatabaseObject, IData
    {
        [field: SerializeField]
        public GameObject StatusVFX { get; private set; }
    }
}