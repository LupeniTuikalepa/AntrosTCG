using Helteix.ChanneledProperties.Formulas;

namespace ATCG.Battle.Entities.Components
{
    public struct HealthComponent : IEntityComponent
    {
        public int CurrentHealth { get; private set; }
        public Formula<int> maxHealth;

        public bool canBeHealed;
        public bool canBeDamaged;

        public HealthComponent(int maxHealth)
        {
            this.maxHealth = new Formula<int>(maxHealth);
            CurrentHealth = maxHealth;
            canBeDamaged = true;
            canBeHealed = true;
        }

        public void AddOrRemoveHealth(int qtt)
        {
            if (qtt < 0 && CurrentHealth <= 0)
                return;

            if(qtt > 0 && !canBeHealed || qtt < 0 && !canBeDamaged)
                return;

            CurrentHealth += qtt;
            if (CurrentHealth < 0)
                CurrentHealth = 0;
            else if (CurrentHealth >= maxHealth)
                CurrentHealth = maxHealth;
        }
    }
}