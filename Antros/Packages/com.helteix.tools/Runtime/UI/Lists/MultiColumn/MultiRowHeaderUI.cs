using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG
{
    [RequireComponent(typeof(LayoutElement))]
    public class MultiRowHeaderUI : MonoBehaviour
    {
        [field: SerializeField]
        public LayoutElement LayoutElement { get; private set; }

        [field: SerializeField]
        public TMP_Text Text { get; private set; }

        public virtual void SetVisibility(bool show)
        {
            gameObject.SetActive(show);
        }
    }
}