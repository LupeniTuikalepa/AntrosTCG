using System;
using ATCG.HexGrids;
using Helteix.Tools.DataMapping;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data
{
    [Serializable]
    public abstract class CapacityPatternData : IData
    {
        [field: SerializeField, BoxGroup("General")]
        public bool IsAdditive { get; private set; } = true;

        [field: SerializeField, BoxGroup("General")]
        public bool OverridePatternOrigin { get; private set; }

        [PropertySpace(SpaceAfter = 15)]
        [field: SerializeField, ShowIf(nameof(OverridePatternOrigin)), BoxGroup("General")]
        public HexCoordinates Offset { get; private set; }

    }
}