using ATCG.Battle.Metrics;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Metrics;
using Eflatun.SceneReference;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle
{
    public class OfflineBattleGameMode : BattleGameMode
    {
        public override string ID => "OfflineBattleGameMode";
        public OfflineBattleGameMode(int seed, params BattlePlayerProfile[] playerProfiles) : base(seed, playerProfiles)
        {

        }

        protected override async Awaitable Initialize()
        {
            await base.Initialize();
            ActivateDisplays();
        }

        private void ActivateDisplays()
        {
            int displayIndex = 0;
            Display[] displays = Display.displays;
            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i] is LocalBattlePlayer localBattlePlayer)
                {
                    if (displays.Length > displayIndex)
                    {
                        Display display = displays[displayIndex];
                        if(!display.active)
                            display.Activate();

                        localBattlePlayer.SetDisplay(displayIndex);
                        displayIndex++;
                    }
                    else
                        Debug.LogError("Couldn't found an available display to the local player");
                }
            }
        }


        protected override IBattlePlayer CreatePlayer(BattlePlayerProfile battlePlayerProfile)
            => new LocalBattlePlayer(this, battlePlayerProfile);

        protected override SceneReference GetGameScene() => GameScenes.Current.Game;
    }
}