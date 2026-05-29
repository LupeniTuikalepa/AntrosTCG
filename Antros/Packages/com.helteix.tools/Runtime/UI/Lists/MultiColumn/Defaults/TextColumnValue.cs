using System;
using ATCG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Helteix.Tools
{
    [RequireComponent(typeof(LayoutElement))]
    public class TextColumnValue : MonoBehaviour, IMultiColumnRowValueUI<string>
    {
        [field: SerializeField]
        public LayoutElement LayoutElement { get; private set; }
        [field: SerializeField]
        public TMP_Text Text { get; private set; }

        private void Reset()
        {
            GetDependencies();
        }
        private void OnValidate()
        {
            GetDependencies();
        }

        private void Awake()
        {
            if(!LayoutElement)
                GetDependencies();
        }

        private void GetDependencies()
        {
            LayoutElement = GetComponent<LayoutElement>();
        }

        public void SetValue(string value) => Text.SetText(value);
    }
}