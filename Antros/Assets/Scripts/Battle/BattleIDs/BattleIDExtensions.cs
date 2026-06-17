using System;
using ATCG.Battle.Entities.Lookups;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
    public static class BattleIDExtensions
    {
        public static bool TryGetEntityWithBattleID(this BattleID battleID, World world, out EntityAddress address)
        {
            foreach (ComponentRef<BattleIDOwner> componentRef in world.Query<BattleIDOwner>())
            {
                BattleID id = componentRef.GetValue().id;

                if (id == battleID)
                {
                    address = componentRef.EntityAddress;
                    return true;
                }
            }
            address = EntityAddress.None;
            return false;
        }
    }
}