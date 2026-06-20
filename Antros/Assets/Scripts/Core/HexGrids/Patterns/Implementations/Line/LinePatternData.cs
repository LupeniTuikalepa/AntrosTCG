using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.HexGrids.Patterns
{
    [Serializable]
    public class LinePatternData : PatternData
    {
        [field: SerializeField, Toggle(nameof(useA))]
        public HexCoordinates A { get; private set; }

        [field: SerializeField]
        public HexCoordinates B { get; private set; }

        [field: SerializeField]
        public bool IsAbsolute { get; private set; }

        [SerializeField, HideInInspector]
        private bool useA;

        public bool UseA => useA;
    }
}