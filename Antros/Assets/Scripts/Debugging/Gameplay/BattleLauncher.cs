using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATCG.Battle;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Turns;
using ATCG.Cards;
using ATCG.GameModes;
using ATCG.Players;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Gameplay
{
    public class BattleLauncher : MonoBehaviour
    {
        private class DebugBattleGameMode : GameMode<object>
        {
            protected override async Awaitable<object> Execute(CancellationToken token)
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
                PhaseResult<BattleHistory> result = await battlePhase.Run();

                return result.value.WinningPlayer == 0;
            }

        }
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
                DebugBattleGameMode gameMode = new DebugBattleGameMode();
                GameController.GameModeController.StartGameMode(gameMode);
                gameMode.RunAndForget();
            }
        }
    }
}