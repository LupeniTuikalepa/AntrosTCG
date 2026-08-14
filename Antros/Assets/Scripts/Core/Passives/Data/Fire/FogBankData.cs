using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Passives.Datas.Datas.Fire
{
    [CreateAssetMenu(menuName = "ATCG/Passive/FogBank")]
    public class FogBankData : PassiveData
    {
        [field: SerializeField]
        public StatusData Status { get; private set; }
    }
}