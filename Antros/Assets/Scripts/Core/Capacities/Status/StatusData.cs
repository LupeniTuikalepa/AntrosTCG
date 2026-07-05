using ATCG.Databases;
using ATCG.Enums;
using Helteix.Tools.DataMapping;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
    public abstract class StatusData : GameDatabaseObject, IData
    {
        [field: SerializeField, BoxGroup("Base")]
        public string Name { get; private set; }
        [field: SerializeField, ColorUsage(false), BoxGroup("Base")]
        public Color Color { get; private set; }
        [field: SerializeField, BoxGroup("Base")]
        public Element Element { get; private set; }

        [PropertySpace]
        [field: SerializeField, BoxGroup("Runtime")]
        public GameObject RuntimeStatus { get; private set; }

        [field: SerializeField, BoxGroup("Runtime"), PreviewField]
        public Sprite Icon { get; private set; }

    }
}