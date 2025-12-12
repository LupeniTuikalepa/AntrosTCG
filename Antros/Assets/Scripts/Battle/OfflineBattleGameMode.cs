using ATCG.Battle.Players;
using ATCG.Metrics;
using ATCG.Players.InputAssigning;

using Eflatun.SceneReference;

using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle
{
    public class OfflineBattleGameMode : BattleGameMode
    {
        public override string ID => "OfflineBattleGameMode";
        public OfflineBattleGameMode(int seed, params IBattlePlayerProfile[] playerProfiles) : base(seed, playerProfiles)
        {

        }

        protected override async Awaitable Initialize()
        {
            PhaseResult<InputUser[]> result = await new PlayerInputsPairingPhase(PlayerCount, GameAssets.Current.PlayerControls).Run();
            int userIndex = 0;
            
            for (int i = 0; i < PlayerCount; i++)
            {
                if (playerProfiles[i] is LocalPlayerProfile localPlayerProfile)
                {
                    localPlayerProfile.InputUser = result.result[userIndex++];
                    playerProfiles[i] = localPlayerProfile;
                }
            }

            await base.Initialize();
        }


        protected override SceneReference GetGameScene() => GameScenes.Current.Game;
    }
}