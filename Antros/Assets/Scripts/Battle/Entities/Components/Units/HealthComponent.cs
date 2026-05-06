using Helteix.ChanneledProperties.Conditions;
using Helteix.ChanneledProperties.Formulas;

namespace ATCG.Battle.Entities.Components
{
    public struct HealthComponent : IEntityComponent
    {
        public int CurrentHealth { get; private set; }
        public Formula<int> MaxHealth { get; private set; }
        public Condition CanBeHealed { get; private set; }
        public Condition CanBeDamaged { get; private set; }

        public HealthComponent(int maxHealth)
        {
            this.MaxHealth = new Formula<int>(maxHealth);
            CurrentHealth = maxHealth;

            CanBeDamaged = new Condition(true);
            CanBeHealed = new Condition(true);
        }

        public void AddOrRemoveHealth(int qtt)
        {
            if (qtt < 0 && CurrentHealth <= 0)
                return;

            if(qtt > 0 && !CanBeHealed || qtt < 0 && !CanBeDamaged)
                return;

            CurrentHealth += qtt;
            if (CurrentHealth < 0)
                CurrentHealth = 0;
            else if (CurrentHealth >= MaxHealth)
                CurrentHealth = MaxHealth;
        }
    }
}