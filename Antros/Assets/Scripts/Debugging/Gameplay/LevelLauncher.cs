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
                PlayerInfos[] players =
                {
                    new() { name = "Player 1" },
                    new() { name = "Player 2" }
                };

                GameCardData[] allCards = GameController.GameDatabase.GetAll<GameCardData>().ToArray();
                IBattlePlayerProfile[] localPlayerProfiles = new IBattlePlayerProfile[players.Length];

                for (int i = 0; i < players.Length; i++)
                    localPlayerProfiles[i] = new LocalPlayerProfile(i, players[i], allCards);

                int seed = Random.Range(int.MinValue, int.MaxValue);
                BattlePhase battlePhase = new BattlePhase(seed, localPlayerProfiles);
                battlePhase.Run();
            }
        }
    }
}