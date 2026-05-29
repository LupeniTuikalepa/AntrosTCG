using System;
using ATCG;
using Unity.Properties;
using UnityEngine;

namespace Helteix.Tools.Samples.MultiColumn
{
    [GeneratePropertyBag, Serializable]
    public class SampleItem
    {

        [property: MultiColumnProperty]
        [field: SerializeField]
        public int Number { get; private set; }

        [property: MultiColumnProperty]
        [field: SerializeField]
        public string Text { get; private set; }

        [property: MultiColumnProperty]
        [field: SerializeField]
        public bool Boolean { get; private set; }


        public SampleItem(int number, string text, bool boolean)
        {
            Number = number;
            Text = text;
            Boolean = boolean;
        }
    }
}