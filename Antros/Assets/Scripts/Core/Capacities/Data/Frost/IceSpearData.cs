using ATCG.Capacities.Attributs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Frost
{
    [CreateAssetMenu(menuName = "ATCG/Capacities/Frost/IceSpear")]
    [WithStep("Hit")]
    public partial class IceSpearData : CapacityData
    {
        [field: SerializeField, MinMaxSlider(1, 20), BoxGroup("Specific")]
        public Vector2Int DamageRange { get; private set; } = Vector2Int.one;
        [field: SerializeField, MinMaxSlider(1, 20), BoxGroup("Specific")]
        public Vector2Int DistanceEfficiencyRange { get; private set; } = Vector2Int.one;

        public int MinDamage => DamageRange.x;
        public int MaxDamage => DamageRange.y;
        public int MinDistance => DistanceEfficiencyRange.x;
        public int MaxDistance => DistanceEfficiencyRange.y;
    }
}