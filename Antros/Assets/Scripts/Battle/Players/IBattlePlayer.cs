using ATCG.Battle.Actions;
using ATCG.Battle.Cards;
using Helteix.Cards.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle.Players
{
    public interface IBattlePlayer
    {
        public delegate void PlayerStatChange(LocalBattlePlayer player, int current, int last);
        event PlayerStatChange OnPlayerHealthChanges;
        event PlayerStatChange OnPlayerManaChanges;

        IBattlePlayerProfile Profile { get; }
        int CurrentHealth { get; }
        int MaxHealth { get; }
        int CurrentMana { get; }
        int MaxMana { get; }
        Hand<IBattleCard> Hand { get; }
        Deck<IBattleCard> Deck { get; }
        DefaultCardCollection<IBattleCard> DeadCards { get; }
        BattlePhase BattlePhase { get; }
        bool IsDefeated();
        Awaitable<BattleTurn> PlayTurn(int round, int turnNumber);
        void OnBattleBegins(BattlePhase battlePhase);
        void OnBattleEnds(BattlePhase battlePhase);
        void AddOrRemoveMana(int mana);
        void AddOrRemoveHealth(int health);
    }
}