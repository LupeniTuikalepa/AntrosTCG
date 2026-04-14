using ATCG.Battle.Cards;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Turns;
using Helteix.Cards.Collections;
using UnityEngine;

namespace ATCG.Battle.Players
{
    public interface IBattlePlayer
    {
        public delegate void PlayerStatChange(LocalBattlePlayer player, int current, int last);

        IBattlePlayerProfile Profile { get; }
        int CurrentHealth { get; }
        int MaxHealth { get; }
        int CurrentMana { get; }
        int MaxMana { get; }
        Hand<IBattleCard> Hand { get; }
        Deck<IBattleCard> Deck { get; }
        DefaultCardCollection<IBattleCard> DeadCards { get; }
        BattlePhase BattlePhase { get; }

        event PlayerStatChange OnPlayerHealthChanges;
        event PlayerStatChange OnPlayerManaChanges;
        bool IsDefeated();
        Awaitable<BattleTurn> PlayTurn(int round, int turnNumber);

        void OnBattleBegins(BattlePhase battlePhase);
        void OnBattleEnds(BattlePhase battlePhase);
        void AddOrRemoveMana(int mana);
        void AddOrRemoveHealth(int health);
    }
}