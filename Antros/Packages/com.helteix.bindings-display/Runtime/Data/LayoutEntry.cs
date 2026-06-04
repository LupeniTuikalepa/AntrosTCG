using System;
using UnityEngine;

namespace Helteix.ControlDisplay.Data
{
    [Serializable]
    public struct LayoutEntry
    {
        [field: SerializeField]
        public string Layout { get; private set; }

        [field: SerializeField]
        public BindingEntry[] BindingEntry { get; private set; }

        [field: SerializeField]
        public GameObject BindingPrefab { get; private set; }

        public bool TryGetEntryFor(BindingDescription description, out BindingEntry entry)
        {
            for (int i = 0; i < BindingEntry.Length; i++)
            {
                entry  = BindingEntry[i];
                if (entry.Matches(description))
                    return true;
            }

            entry = default;
            return false;
        }

    }
}