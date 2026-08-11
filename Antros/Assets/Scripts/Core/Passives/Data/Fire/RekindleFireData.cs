using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Passives.Datas.Datas.Fire
{
    [CreateAssetMenu(menuName = "ATCG/Passive/RekindleFire")]
    public class RekindleFireData : PassiveData
    {
        [field: SerializeField]
        public StatusData Status { get; private set; }
        
        [field: SerializeField]
        public int AdditionalStack { get; private set; }
    }
}