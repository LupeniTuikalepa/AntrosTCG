using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.Utilities;

namespace Helteix.BindingDisplay.Defaults.Interactions
{
    public class HoldDefaultInteractionUI : DefaultInteractionUI<HoldInteraction>
    {
        [SerializeField]
        private TMP_Text timeText;


        protected internal override void Activate(NameAndParameters nameAndParameters)
        {
            base.Activate(nameAndParameters);
            if(timeText == null)
                return;

            for (int i = 0; i < nameAndParameters.parameters.Count; i++)
            {
                NamedValue parameter = nameAndParameters.parameters[i];
                if (parameter.name == nameof(HoldInteraction.duration))
                    timeText.text = parameter.value.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}