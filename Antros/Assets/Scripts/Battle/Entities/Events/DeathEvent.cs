using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Core;
using ATCG.Battle.Players;
using ATCG.Battle.Timelines;

namespace ATCG.Battle.Entities.Events
{
    public class DeathEvent : EntityEvent
    {
        public DeathEvent(EntityEvent parent = null, params EntityAddress[] entities) : base(parent, entities)
        {
            
        }
        
        public override void Apply()
        {
            for (int i = 0; i < TargetedEntities.Length; i++)
            {
                EntityAddress address = TargetedEntities[i];
                Entity entity = address.entity;
                World world = address.world;

                if (entity.TryGetComponent<HeroComponent>(world, out var componentRef))
                {
                    ref HeroComponent hero = ref componentRef.GetValue();
                    IBattlePlayer player = hero.HeroBattleCard.Player;
                    
                    player.AddOrRemoveHealth(hero.HeroBattleCard.DeathCost);
                }
                
                world.DestroyEntity(entity);
            }
        }
    }
}