using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.UI
{
    [RequireComponent(typeof(CustomSelectable))]
    public class SelectableUIComponent : UIComponent
    {
        [field: SerializeField, ReadOnly]
        protected CustomSelectable Selectable { get; private set; }

        private void Reset()
        {
            Selectable = GetComponent<CustomSelectable>();
        }

    }
}