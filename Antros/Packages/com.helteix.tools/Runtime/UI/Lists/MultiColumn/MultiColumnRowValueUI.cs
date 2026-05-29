using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG
{
    public interface IMultiColumnRowValueUI<in T>
    {
        LayoutElement LayoutElement { get; }
        void SetValue(T value);
    }
}