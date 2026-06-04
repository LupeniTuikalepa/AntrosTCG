using System;
using UnityEngine;

namespace Helteix.ControlDisplay.Data
{
    [Serializable]
    public struct CompositeEntry
    {
        [field: SerializeField]
        public string CompositeName { get; private set; }
        [field: SerializeField]
        public GameObject Prefab { get; private set; }
    }
}