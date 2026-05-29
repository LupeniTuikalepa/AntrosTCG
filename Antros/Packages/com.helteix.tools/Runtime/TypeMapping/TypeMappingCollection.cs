using System;
using System.Collections.Generic;
using UnityEngine;

namespace Helteix.Tools.TypeMapping
{
    [Serializable]
    public struct TypeMapEntry
    {
        [SerializeField]
        public string guid;
        [SerializeField]
        public string typeInfos;
    }

    public sealed class TypeMappingCollection : ScriptableObject
    {
        [SerializeField]
        private List<TypeMapEntry> entries;

        public IReadOnlyList<TypeMapEntry> Entries => entries.AsReadOnly();
    }
}