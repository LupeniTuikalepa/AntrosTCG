
using System.Linq;
using ATCG.Battle;
using ATCG.Battle.Players;
using ATCG.Cards;
using ATCG.Databases;
using ATCG.Metrics;
using ATCG.Players;
using ATCG.Players.InputAssigning;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.MainMenu.MainMenu.GameModeButtons
{
    public class LaunchOfflineGameMode : LaunchGameMode<OfflineBattleGameMode, BattleGameModeResults>
    {
        protected override async Awaitable<OfflineBattleGameMode> GetGameMode()
        {
            await Awaitables.CompletedAwaitable;
            int seed = Random.Range(int.MinValue, int.MaxValue);

            PlayerProfile[] players = new PlayerProfile[]
            {
                new PlayerProfile()
                {
                    name = "Player 1"
                },
                new PlayerProfile()
                {
                    name = "Player 2"
                }
            };
                string[] allCards =
                    GameController.GameDatabase.GetAll<GameCardData>()
                        .Select(ctx => ctx.ID.ToString())
                        .ToArray();

                //PlayerInputPairing[] pairings = result.result;
                LocalPlayerProfile[] localPlayerProfiles = new LocalPlayerProfile[players.Length];
                for (int i = 0; i < players.Length; i++)
                {
                    localPlayerProfiles[i] = new LocalPlayerProfile()
                    {
                        ID = i,
                        Profile = players[i],
                        Deck = new PlayerDeck()
                        {
                            cards = allCards,
                        },
                    };
                }
                return new OfflineBattleGameMode(seed, localPlayerProfiles);
                /*
            }
            */

            return null;
        }
    }
}