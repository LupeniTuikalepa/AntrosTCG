using ATCG.Capacities;
using ATCG.Capacities.UI;
using Helteix.Tools.UI;
using UnityEngine;

namespace ATCG.Cards.UI.Components
{
    public abstract class BaseCapacityProviderUI : MonoBehaviour
    {
        public abstract bool Build(ICapacityDataProvider provider);

        public abstract void Clear();
    }
}