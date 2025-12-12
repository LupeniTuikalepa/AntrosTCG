using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.UI
{
    public class BarUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text valueText;
        [SerializeField]
        private Image fill;

        [SerializeField]
        private float fillDuration = .2f;

        public float MaxValue { get; protected set; }
        public float CurrentValue { get; protected set; }

        public void Refresh()
        {
            float target = CurrentValue / MaxValue;

            Tween.StopAll(fill);
            Tween.UIFillAmount(fill, target, fillDuration, Ease.OutCubic);
            valueText.text = $"{CurrentValue}/{MaxValue}";
        }
    }
}