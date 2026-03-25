using ATCG.Battle.Players;
using ATCG.Cards.Implementations;

namespace ATCG.Battle.Cards
{
    public class HeroBattleCard : BattleCard<HeroCardData>, IHeroCard
    {
        public int CurrentHealth { get; private set; }
        public int MaxHealth => Data.Health;

        public int DeathCost => Data.DeathCost;
        public int Speed => Data.Speed;
        public int Strength => Data.Strength;

        public HeroBattleCard(HeroCardData data, IBattlePlayer player) : base(data, player)
        {

        }

        /*
        public bool CanPerformBasicAttack() => Player.CurrentMana >= GameMetrics.Current.BasicAttackCost;


        public async Awaitable PerformBasicAttack()
        {
            BasicAttackCommand basicAttackCommand = new (this, BattleGrid);
            await RunEvent<BasicAttackCommand, IBasicAttackCommandListener>(basicAttackCommand);
        }

        public void GetMovableCoords(List<HexCoordinates> output)
        {
            FloodFillPattern pattern = new(Speed, Coordinates)
            {
                ValidateCell = ctx => BattleGrid.TryGetBattleCell(ctx, out BattleCell battleCell) && battleCell.CanHeroMoveTo()
            };

            pattern.Evaluate(output);
        }

        public async Awaitable MoveCard(HexCoordinates to)
        {
            MoveCommand moveCommand = new MoveCommand(to);
            if(BattleGrid.TryGetBattleCell(Coordinates, out BattleCell battleCell))
                battleCell.cell.RemoveMember(this);

            await RunEvent<MoveCommand, IMoveCommandListener>(moveCommand);

            Coordinates = to;
            if(BattleGrid.TryGetBattleCell(Coordinates, out battleCell))
                battleCell.cell.AddMember(this);
        }


        public async Awaitable AddOrRemoveHealth(int damage)
        {
            int last = CurrentHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth + damage, 0, MaxHealth);

            if(CurrentHealth != last)
                HealthChanged?.Invoke(this, damage, last, CurrentHealth);

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Die();
            }
        }


        private void Die()
        {
            Player.AddOrRemoveHealth(-DeathCost);
        }
        */
    }
}