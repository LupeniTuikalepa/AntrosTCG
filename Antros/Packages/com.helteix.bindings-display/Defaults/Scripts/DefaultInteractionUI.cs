using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Helteix.BindingDisplay.Defaults
{

    public abstract class DefaultInteractionUI : MonoBehaviour
    {
        protected internal abstract bool Matches(NameAndParameters nameAndParameters);

        protected internal virtual void Activate(NameAndParameters nameAndParameters)
        {
            gameObject.SetActive(true);
        }

        protected internal virtual void Deactivate()
        {
            gameObject.SetActive(false);
        }
    }
    public abstract class DefaultInteractionUI<T> : DefaultInteractionUI
    {
        protected internal override bool Matches(NameAndParameters nameAndParameters)
        {
            Type interaction = InputSystem.TryGetInteraction(nameAndParameters.name);
            if(interaction is not null)
                return interaction == typeof(T);

            return false;
        }


        public void Sync(NameAndParameters[] bindingInteractions)
        {
            for (int i = 0; i < bindingInteractions.Length; i++)
            {
                if (Matches(bindingInteractions[i]))
                {
                    Activate(bindingInteractions[i]);
                    return;
                }
            }

            Deactivate();
        }
    }
}