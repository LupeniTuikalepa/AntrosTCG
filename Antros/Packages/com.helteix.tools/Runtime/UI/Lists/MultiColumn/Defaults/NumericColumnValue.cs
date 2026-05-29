using System;
using System.Globalization;
using ATCG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Helteix.Tools
{
    [RequireComponent(typeof(LayoutElement))]
    public class NumericColumnValue : MonoBehaviour,
        IMultiColumnRowValueUI<int>,
        IMultiColumnRowValueUI<float>,
        IMultiColumnRowValueUI<long>,
        IMultiColumnRowValueUI<uint>,
        IMultiColumnRowValueUI<ulong>,
        IMultiColumnRowValueUI<double>
    {

        [field: SerializeField]
        public LayoutElement LayoutElement { get; private set; }

        [SerializeField]
        private string format;

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

        void IMultiColumnRowValueUI<double>.SetValue(double value)
        {
            if(string.IsNullOrEmpty(format))
                Text.SetText(value.ToString(CultureInfo.InvariantCulture));
            else
                Text.SetText(value.ToString(format));
        }

        void IMultiColumnRowValueUI<ulong>.SetValue(ulong value)
        {
            if(string.IsNullOrEmpty(format))
                Text.SetText(value.ToString());
            else
                Text.SetText(value.ToString(format));
        }

        void IMultiColumnRowValueUI<uint>.SetValue(uint value)
        {
            if(string.IsNullOrEmpty(format))
                Text.SetText(value.ToString());
            else
                Text.SetText(value.ToString(format));
        }

        void IMultiColumnRowValueUI<long>.SetValue(long value)
        {
            if(string.IsNullOrEmpty(format))
                Text.SetText(value.ToString());
            else
                Text.SetText(value.ToString(format));
        }

        void IMultiColumnRowValueUI<float>.SetValue(float value)
        {
            if(string.IsNullOrEmpty(format))
                Text.SetText(value.ToString(CultureInfo.InvariantCulture));
            else
                Text.SetText(value.ToString(format));
        }

        void IMultiColumnRowValueUI<int>.SetValue(int value)
        {
            if(string.IsNullOrEmpty(format))
                Text.SetText(value.ToString());
            else
                Text.SetText(value.ToString(format));
        }

    }
}