using System.Collections.Generic;
using ATCG.Battle.Actions;
using ATCG.Battle.Cards;
using ATCG.Battle.Grids;
using ATCG.Battle.Metrics;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Cards;
using ATCG.HexGrids;
using Helteix.Cards.Collections;
using Helteix.ChanneledProperties.Conditions;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players
{
    public class LocalBattlePlayer : IBattlePlayer
    {

        IBattlePlayerProfile IBattlePlayer.Profile => Profile;

        public event IBattlePlayer.PlayerStatChange OnPlayerHealthChanges;
        public event IBattlePlayer.PlayerStatChange OnPlayerManaChanges;

        public int ID => Profile.ID;
        public LocalPlayerProfile Profile { get; private set; }
        public Hand<IBattleCard> Hand { get; private set; }
        public Deck<IBattleCard> Deck { get; private set; }
        public DefaultCardCollection<IBattleCard> DeadCards { get; private set; }
        public int CurrentHealth { get; private set; }
        public int MaxHealth => GameplayMetrics.Current.MaxHealth;
        public int CurrentMana { get; private set; }
        public int MaxMana => GameplayMetrics.Current.MaxMana;

        public BattleGameMode BattleGameMode { get; }

        public readonly Condition canDeployHeroes;
        public readonly Condition canMoveHeroes;
        public readonly Condition canUseHeroesAbilities;

        public LocalBattlePlayer(BattleGameMode gameMode, LocalPlayerProfile profile)
        {
            BattleGameMode = gameMode;

            Profile = profile;
            canDeployHeroes = new Condition(false);
            canMoveHeroes = new Condition(false);
            canUseHeroesAbilities = new Condition(false);

            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;

            Hand = new Hand<IBattleCard>();
            Deck = new Deck<IBattleCard>();
            DeadCards = new DefaultCardCollection<IBattleCard>();

            string[] deckCards = profile.Deck.cards;
            for (int i = 0; i < deckCards.Length; i++)
            {
                if (GameController.GameDatabase.TryGetObject(deckCards[i], out GameCardData data))
                {
                    IBattleCard card = GameplayCardManager.CreateCardFor(data, ID);
                    Deck.TryAddCard(card);
                }
            }
        }

        [Button]
        public void AddOrRemoveMana(int mana)
        {
            int last = mana;
            CurrentMana += mana;
            if (CurrentMana > MaxMana)
                CurrentMana = MaxMana;
            if(CurrentMana < 0)
                CurrentMana = 0;

            if (last != CurrentMana)
                OnPlayerManaChanges?.Invoke(this, CurrentMana, last);
        }

        [Button]
        public void AddOrRemoveHealth(int health)
        {
            int last = health;
            CurrentHealth += health;
            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;
            if(CurrentHealth < 0)
                CurrentHealth = 0;

            if (last != CurrentHealth)
                OnPlayerHealthChanges?.Invoke(this, CurrentMana, last);
        }

        public bool IsDefeated() => CurrentHealth <= 0;

        public void DeployBattleCard(IBattleCard card, HexCoordinates coordinates)
        {
            BattleGrid battleGrid = BattleGameMode.BattleGrid;

            if (Hand.TryRemoveCard(card))
            {
                battleGrid.DeployCard(card, coordinates);
            }
        }

        public void FillHand()
        {
            Debug.Log($"[Player Turn] Filling {Profile.Infos.name} player's hand.");
            int missingCards = GameplayMetrics.Current.MinPlayerHandSize - Hand.CurrentSize;
            if (missingCards > 0)
            {
                for (int i = 0; i < missingCards; i++)
                {
                    if (!TryDrawCardFromDeck(out IBattleCard card))
                        break;
                    if(!Hand.TryAddCard(card))
                        break;
                }
            }
        }

        private bool TryDrawCardFromDeck(out IBattleCard card)
        {
            if (Deck.TryGet(out card))
                return true;

            if (TryRefillDeck())
                return TryDrawCardFromDeck(out card);

            return false;
        }

        private bool TryRefillDeck()
        {
            using (ListPool<IBattleCard>.Get(out List<IBattleCard> cards))
            {
                cards.AddRange(DeadCards.Cards);
                if(cards.Count <= 0)
                    return false;

                foreach (IBattleCard card in cards)
                {
                    if (!DeadCards.TryRemoveCard(card))
                        return false;

                    if (!Deck.TryAddCard(card))
                        return false;
                }
            }

            return true;
        }

        public async Awaitable<BattleTurn> PlayTurn(int round, int turnNumber)
        {
            LocalPlayerTurnPhase localPlayerTurnPhase = new LocalPlayerTurnPhase(turnNumber, this);

            PhaseResult<BattleTurn> result = await localPlayerTurnPhase.Run();
            return result;
        }

        public void OnBattleBegins(BattleGameMode battleGameMode)
        {
            FillHand();
        }

        public void OnBattleEnds(BattleGameMode battleGameMode)
        {

        }
    }
}