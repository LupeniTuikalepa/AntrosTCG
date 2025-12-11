using ATCG.Battle.Players;
using ATCG.Metrics;
using ATCG.Players.InputAssigning;

using Eflatun.SceneReference;

using Helteix.Tools.Phases;
using UnityEngine;

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
            await new PlayerInputsPairingPhase(PlayerCount, GameAssets.Current.PlayerControls).Run();

            await base.Initialize();
        }


        protected override SceneReference GetGameScene() => GameScenes.Current.Game;
    }
}