using ATCG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Helteix.Tools
{
    [RequireComponent(typeof(LayoutElement))]
    public class BoolColumnValue: MonoBehaviour, IMultiColumnRowValueUI<bool>
    {
        [field: SerializeField]
        public LayoutElement LayoutElement { get; private set; }

        [field: SerializeField]
        public GameObject Icon { get; private set; }

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

        public void SetValue(bool value) => Icon.SetActive(value);
    }
}