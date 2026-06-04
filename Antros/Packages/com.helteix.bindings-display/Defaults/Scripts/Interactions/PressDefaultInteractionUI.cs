using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.Utilities;

namespace Helteix.BindingDisplay.Defaults.Interactions
{
    public sealed class PressDefaultInteractionUI : DefaultInteractionUI<PressInteraction>
    {
        [SerializeField]
        private GameObject press;
        [SerializeField]
        private GameObject release;
        [SerializeField]
        private GameObject pressAndRelease;

        protected internal override void Activate(NameAndParameters nameAndParameters)
        {
            for (int i = 0; i < nameAndParameters.parameters.Count; i++)
            {
                NamedValue parameter = nameAndParameters.parameters[i];
                if (parameter.name == nameof(PressInteraction.behavior))
                {
                    string s = parameter.value.ToString(CultureInfo.InvariantCulture);
                    if (press)
                        press.SetActive(s == nameof(PressBehavior.PressOnly));
                    if (release)
                        release.SetActive(s == nameof(PressBehavior.ReleaseOnly));
                    if (pressAndRelease)
                        pressAndRelease.SetActive(s == nameof(PressBehavior.PressAndRelease));
                }
            }
            base.Activate(nameAndParameters);
        }
    }
}