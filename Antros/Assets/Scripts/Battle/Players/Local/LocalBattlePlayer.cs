using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Turns;
using ATCG.Cards;
using ATCG.HexGrids;
using ATCG.Metrics;
using Helteix.Cards.Collections;
using Helteix.ChanneledProperties.Conditions;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local
{
    public class LocalBattlePlayer : IBattlePlayer
    {
        public readonly Condition canDeployHeroes;
        public readonly Condition canDoBasicAttack;
        public readonly Condition canMoveHeroes;

        public readonly Condition canUseHeroesAbilities;

        public LocalBattlePlayer(BattlePhase battle, LocalPlayerProfile profile)
        {
            BattlePhase = battle;

            Profile = profile;
            canDeployHeroes = new Condition(false);
            canMoveHeroes = new Condition(false);
            canUseHeroesAbilities = new Condition(false);
            canDoBasicAttack = new Condition(false);

            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;

            Hand = new Hand<IBattleCard>();
            Deck = new Deck<IBattleCard>();
            DeadCards = new DefaultCardCollection<IBattleCard>();

            GameCardData[] deckCards = profile.Cards;
            for (int i = 0; i < deckCards.Length; i++)
            {
                IBattleCard card = GameplayCardManager.CreateCardFor(deckCards[i], this);
                Deck.TryAddCard(card);
            }
        }


        public int ID => Profile.ID;
        public LocalPlayerProfile Profile { get; }
        public event IBattlePlayer.PlayerStatChange OnPlayerHealthChanges;
        public event IBattlePlayer.PlayerStatChange OnPlayerManaChanges;


        IBattlePlayerProfile IBattlePlayer.Profile => Profile;
        public Hand<IBattleCard> Hand { get; }
        public Deck<IBattleCard> Deck { get; }
        public DefaultCardCollection<IBattleCard> DeadCards { get; }
        public int CurrentHealth { get; private set; }
        public int MaxHealth => GameMetrics.Current.MaxHealth;
        public int CurrentMana { get; private set; }
        public int MaxMana => GameMetrics.Current.MaxMana;

        public BattlePhase BattlePhase { get; }

        [Button]
        public void AddOrRemoveMana(int mana)
        {
            int last = mana;
            CurrentMana += mana;
            if (CurrentMana > MaxMana)
                CurrentMana = MaxMana;
            if (CurrentMana < 0)
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
            if (CurrentHealth < 0)
                CurrentHealth = 0;

            if (last != CurrentHealth)
                OnPlayerHealthChanges?.Invoke(this, CurrentMana, last);
        }

        public bool IsDefeated()
        {
            return CurrentHealth <= 0;
        }

        public async Awaitable<BattleTurn> PlayTurn(int round, int turnNumber)
        {
            LocalPlayerTurnPhase localPlayerTurnPhase = new(turnNumber, this);

            PhaseResult<BattleTurn> result = await localPlayerTurnPhase.Run();
            return result;
        }

        void IBattlePlayer.OnBattleBegins(BattlePhase battlePhase)
        {
            FillHand();
        }

        void IBattlePlayer.OnBattleEnds(BattlePhase battlePhase)
        {
        }


        public void FillHand()
        {
            Debug.Log($"[Player Turn] Filling {Profile.Infos.name} player's hand.");
            int missingCards = GameMetrics.Current.MinPlayerHandSize - Hand.CurrentSize;
            if (missingCards <= 0)
                return;

            for (int i = 0; i < missingCards; i++)
            {
                if (!TryDrawCardFromDeck(out IBattleCard card))
                    break;
                if (!Hand.TryAddCard(card))
                    break;
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
                if (cards.Count <= 0)
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
    }
}