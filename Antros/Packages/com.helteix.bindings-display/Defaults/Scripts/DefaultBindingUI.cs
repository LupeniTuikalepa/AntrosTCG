using System;
using Helteix.ControlDisplay;
using Helteix.ControlDisplay.Data;
using Helteix.ControlDisplay.UI.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace Helteix.BindingDisplay.Defaults
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public class DefaultBindingUI : MonoBehaviour, IBindingUI, ILayoutElement
    {
        [SerializeField, HideInInspector]
        private RectTransform rectTransform;
        [SerializeField, HideInInspector]
        private Image image;

        [SerializeField]
        private TMP_Text displayKeyText;
        [SerializeField]
        private DefaultInteractionUI[] interactionUis;

        private float targetHeight;
        private float targetWidth;
        private BindingDisplayStyling styling;

        private void Reset()
        {
            OnValidate();
        }
        private void OnValidate()
        {
            image = GetComponent<Image>();
            rectTransform = transform as RectTransform;
        }

        private void Awake()
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            styling = GetComponentInParent<BindingDisplayStyling>();
        }

        void IBindingUI.Sync(BindingDescription binding, BindingIcons icons)
        {
            if (displayKeyText)
            {
                displayKeyText.text = binding.displayString;
            }

            int variantIndex = styling == null ? 0 : styling.IconVariantIndex;
            image.sprite = icons.GetIcon(variantIndex);

            for (int i = 0; i < interactionUis.Length; i++)
            {
                DefaultInteractionUI ui = interactionUis[i];
                if(ui == null)
                    continue;

                bool isCompatible = false;
                NameAndParameters compatible = default;
                for (int j = 0; j < binding.interactions.Length; j++)
                {
                    compatible = binding.interactions[j];
                    bool matches = ui.Matches(compatible);
                    if (matches)
                    {
                        isCompatible = true;
                        break;
                    }
                }

                if(isCompatible)
                    ui.Activate(compatible);
                else
                    ui.Deactivate();
            }
        }

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
            targetWidth = rectTransform.sizeDelta.y;
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
            targetHeight = rectTransform.sizeDelta.x;
        }

        float ILayoutElement.minWidth => targetWidth * .5f;
        float ILayoutElement.preferredWidth => targetWidth;
        float ILayoutElement.flexibleWidth => 0;

        float ILayoutElement.minHeight => targetHeight * .5f;
        float ILayoutElement.preferredHeight => targetHeight;

        float ILayoutElement.flexibleHeight { get; } = 0;

        int ILayoutElement.layoutPriority => 1;
    }
}