using UnityEngine;

namespace ATCG.Capacities.Frost
{
    [CreateAssetMenu(menuName = "ATCG/Deployable/Frost/IceWall")]
    public class IceWallData : DeployableData
    {
        [field: SerializeField]
        public int Health { get; private set; }
    }
}