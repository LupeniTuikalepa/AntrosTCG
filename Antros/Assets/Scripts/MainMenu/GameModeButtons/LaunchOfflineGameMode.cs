using System.Linq;
using ATCG.Battle;
using ATCG.Battle.Players;
using ATCG.Cards;
using ATCG.Players;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.MainMenu.MainMenu.GameModeButtons
{
    public class LaunchOfflineGameMode : LaunchGameMode<OfflineBattleGameMode, BattleGameModeResults>
    {
        [SerializeField]
        private GameCardData[] cards;
        protected override async Awaitable<OfflineBattleGameMode> GetGameMode()
        {
            await Awaitables.CompletedAwaitable;
            string[] deck = cards
                .Select(ctx => ctx.ID.ToString())
                .ToArray();

            int seed = Random.Range(int.MinValue, int.MaxValue);
            return new OfflineBattleGameMode(seed,
                new BattlePlayerProfile()
                {
                    id = 0,
                    playerDeck = deck,
                    playerProfile = new PlayerProfile()
                    {
                        name = "Player 1"
                    }
                },
                new BattlePlayerProfile()
                {
                    id = 1,
                    playerDeck = deck,
                    playerProfile = new PlayerProfile()
                    {
                        name = "Player 2"
                    }
                });

        }
    }
}