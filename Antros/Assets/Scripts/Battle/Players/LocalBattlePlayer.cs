using System;
using ATCG.Battle.Actions;
using ATCG.Battle.Cards;
using ATCG.Battle.HexGrids;
using ATCG.Battle.Metrics;
using ATCG.HexGrids;
using Helteix.Cards.Collections;
using Helteix.ChanneledProperties.Conditions;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Players
{
    public class LocalBattlePlayer : IBattlePlayer
    {
        public event Action<LocalBattlePlayer, int, int> OnPlayerHealthChanges;
        public event Action<LocalBattlePlayer, int, int> OnPlayerManaChanges;

        public BattlePlayerProfile Profile { get; }
        public int CurrentMana { get; private set; }
        public Hand<IBattleCard> Hand { get; private set; }
        public Deck<IBattleCard> Deck { get; private set; }

        public DefaultCardCollection<IBattleCard> DeadCards { get; private set; }

        private readonly BattleGameMode gameMode;

        public readonly Condition canDeployHeroes;
        public readonly Condition canDiscardCards;
        public readonly Condition canMoveHeroes;
        public readonly Condition canUseHeroesAbilities;

        public int CurrentHealth { get; private set; }

        public LocalBattlePlayer(BattleGameMode gameMode, BattlePlayerProfile profile)
        {
            this.gameMode = gameMode;
            Profile = profile;
            canDeployHeroes = new Condition(false);
            canDiscardCards = new Condition(false);
            canMoveHeroes = new Condition(false);
            canUseHeroesAbilities = new Condition(false);

            CurrentHealth = GameplayMetrics.Current.MaxHealth;
            CurrentMana = GameplayMetrics.Current.MaxMana;
        }

        public void AddOrRemoveMana(int mana)
        {
            int last = mana;
            CurrentMana += mana;
            int maxMana = GameplayMetrics.Current.MaxMana;
            if (CurrentMana > maxMana)
                CurrentMana = maxMana;
            if(CurrentMana < 0)
                CurrentMana = 0;

            if (last != CurrentMana)
                OnPlayerManaChanges?.Invoke(this, CurrentMana, last);
        }

        public void AddOrRemoveHealth(int health)
        {
            int last = health;
            CurrentHealth += health;
            int maxHealth = GameplayMetrics.Current.MaxHealth;
            if (CurrentHealth > maxHealth)
                CurrentHealth = maxHealth;
            if(CurrentHealth < 0)
                CurrentHealth = 0;

            if (last != CurrentHealth)
                OnPlayerHealthChanges?.Invoke(this, CurrentMana, last);

        }

        public bool IsDefeated() => CurrentHealth <= 0;

        public void DeployBattleCard(IBattleCard card, HexCoordinates coordinates)
        {
            BattleGrid battleGrid = gameMode.BattleGrid;

            if (!battleGrid.HasMember(coordinates) && Hand.TryRemoveCard(card))
                battleGrid.DeployCard(card, coordinates);
        }

        public void FillHand()
        {

        }

        public async Awaitable<BattleTurn> PlayTurn(int round, int turnNumber)
        {
            PlayTurnPhase playTurnPhase = new PlayTurnPhase(turnNumber, this);

            PhaseResult<BattleTurn> result = await playTurnPhase.Run();
            return result;
        }
    }
}