using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.Utilities;

namespace Helteix.BindingDisplay.Defaults.Interactions
{
    public class MultiTapDefaultInteractionUI : DefaultInteractionUI<MultiTapInteraction>
    {
        [SerializeField]
        private TMP_Text countText;

        protected internal override void Activate(NameAndParameters nameAndParameters)
        {
            base.Activate(nameAndParameters);
            if(countText == null)
                return;

            for (int i = 0; i < nameAndParameters.parameters.Count; i++)
            {
                NamedValue parameter = nameAndParameters.parameters[i];
                if (parameter.name == nameof(MultiTapInteraction.tapCount))
                    countText.text = parameter.value.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}