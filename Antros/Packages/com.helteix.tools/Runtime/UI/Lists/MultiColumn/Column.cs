using System;
using UnityEngine;

namespace ATCG
{
    [Serializable]
    internal class Column
    {
        [field: SerializeField, HideInInspector]
        public string PropertyName { get; internal set; }

        [field: SerializeField, Space]
        public bool Show { get; internal set; }

        [field: SerializeField]
        public string Title { get; internal set; }
        [field: SerializeField]
        public int Width { get; internal set; }

        [field: SerializeField]
        public GameObject ValueUIPrefab { get; internal set; }
    }
}