using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Heroes.Runtime;
using ATCG.Battle.Metrics;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local.Phases.Filters;
using ATCG.Cards.Implementations;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using Helteix.Cards;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Cards
{
    public class HeroBattleCard : BattleCard<HeroCardData>, IHeroCard
    {
        public delegate void CardHealthChangeDelegate(HeroBattleCard card, int amount, int from, int to);

        public event CardHealthChangeDelegate HealthChanged;

        public int MaxHealth => Data.Health;
        public int CurrentHealth { get; private set; }

        public int DeathCost => Data.DeathCost;
        public int Speed => Data.Speed;
        public int Strength => Data.Strength;

        private List<object> cardEventRunners;

        public HeroBattleCard(HeroCardData data, IBattlePlayer player) : base(data, player)
        {

        }

        public bool CanPerformBasicAttack() => Player.CurrentMana >= GameplayMetrics.Current.BasicAttackCost;

        public async Awaitable PerformBasicAttack()
        {
            BasicAttackEvent basicAttackEvent = new (this, Player, BattleGrid);
            await RunEvent<BasicAttackEvent, IBasicAttackEventRunner>(basicAttackEvent);
        }


        public async Awaitable MoveCard(HexCoordinates to)
        {
            MoveEvent moveEvent = new MoveEvent(this, to);
            if(BattleGrid.TryGetBattleCell(Coordinates, out BattleCell cell))
                cell.cell.RemoveMember(this);

            await RunEvent<MoveEvent, IMoveEventRunner>(moveEvent);

            Coordinates = to;
            if(BattleGrid.TryGetBattleCell(Coordinates, out cell))
                cell.cell.AddMember(this);
        }

        public void AddOrRemoveHealth(int damage)
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
    }
}