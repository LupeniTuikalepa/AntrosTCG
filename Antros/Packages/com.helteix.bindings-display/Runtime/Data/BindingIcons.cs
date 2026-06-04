using System;
using UnityEngine;

namespace Helteix.ControlDisplay.Data
{
    [Serializable]
    public struct BindingIcons
    {
        public int Count => icons.Length;

        [SerializeField]
        private Sprite[] icons;

        public Sprite GetIcon(int index) => icons[index];
    }
}