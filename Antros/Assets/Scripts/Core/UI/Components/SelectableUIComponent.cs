using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.UI
{
    [RequireComponent(typeof(CustomSelectableForUIComponent))]
    public class SelectableUIComponent : UIComponent
    {
        [field: SerializeField, ReadOnly]
        protected CustomSelectableForUIComponent Selectable { get; private set; }

        private void Reset()
        {
            Selectable = GetComponent<CustomSelectableForUIComponent>();
        }

    }
}