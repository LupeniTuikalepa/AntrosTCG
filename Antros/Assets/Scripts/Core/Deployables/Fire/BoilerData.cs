using ATCG.Construction;
using UnityEngine;

namespace ATCG.Capacities.Fire
{
    [CreateAssetMenu(menuName = "ATCG/Deployable/Fire/Boiler")]
    public class BoilerData : ConstructionData
    {
        [field: SerializeField]
        public int StackAdd { get; private set; }
    }
}