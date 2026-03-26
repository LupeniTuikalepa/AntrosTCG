using ATCG.Battle.Entities.Core.Components;

namespace ATCG.Battle.Entities.Components
{
    public struct HealthComponent : IEntityComponent
    {
        public int maxHealth;
        public int currentHealth;

        public void AddOrRemoveHealth(int qtt)
        {
            if(qtt < 0 && currentHealth <= 0)
                return;
            
            currentHealth -= qtt;
            if (currentHealth < 0)
                currentHealth = 0;
            else if (currentHealth >= maxHealth)
                currentHealth = maxHealth;
        }
    }
}