using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Capacities.Fire
{
    [CreateAssetMenu(menuName = "ATCG/Deployable/Fire/WillOWisp")]
    public class WillOWispData : DeployableData
    {
        [field: SerializeField]
        public int Health { get; private set; }
        
        [field: SerializeField]
        public int AttackRange { get; private set; }
        
        [field: SerializeField]
        public int MoveSpeed { get; private set; }
        
        [field: SerializeField]
        public StatusData Status { get; private set; }
    }
}