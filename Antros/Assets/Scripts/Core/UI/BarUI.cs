using Helteix.Tools;
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

        private float lastValue;

        public void Refresh() => RefreshAsync().ListenForExceptions();

        public async Awaitable RefreshAsync()
        {
            float target = CurrentValue / MaxValue;
            
            
            Tween.StopAll(fill);

            await Sequence.Create()
                .Insert(0f, Tween.UIFillAmount(fill, target, fillDuration, Ease.OutCubic))
                .Insert(0f, Tween.Custom(lastValue, CurrentValue, fillDuration, ctx =>
                {
                    valueText.text = $"{(int)ctx}/{MaxValue}";
                }, Ease.OutCubic));
            
            valueText.text = $"{CurrentValue}/{MaxValue}";
            lastValue = CurrentValue;
        }
    }
}