using System.Linq;
using ATCG.Battle;
using ATCG.Battle.Players;
using ATCG.Cards;
using ATCG.GameModes;
using ATCG.Players;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Gameplay
{
    public class LevelLauncher : MonoBehaviour
    {
        [SerializeField]
        private bool launchOnStart = true;

        private void Start()
        {
            if (launchOnStart)
                LaunchDebugBattleMode();
        }

        public void LaunchDebugBattleMode()
        {
            if (GameController.GameModeController.Current == null)
            {
                PlayerInfos[] players = new PlayerInfos[]
                {
                    new PlayerInfos()
                    {
                        name = "Player 1"
                    },
                    new PlayerInfos()
                    {
                        name = "Player 2"
                    }
                };
                string[] allCards =
                    GameController.GameDatabase.GetAll<GameCardData>()
                        .Select(ctx => ctx.ID.ToString())
                        .ToArray();

                //PlayerInputPairing[] pairings = result.result;
                IBattlePlayerProfile[] localPlayerProfiles = new IBattlePlayerProfile[players.Length];
                for (int i = 0; i < players.Length; i++)
                {
                    localPlayerProfiles[i] = new LocalPlayerProfile()
                    {
                        ID = i,
                        Infos = players[i],
                        Deck = new PlayerDeck()
                        {
                            cards = allCards,
                        },
                    };
                }

                int seed = Random.Range(int.MinValue, int.MaxValue);
                _ = new OfflineBattleGameMode(seed, localPlayerProfiles).Run();
            }
        }
    }
}