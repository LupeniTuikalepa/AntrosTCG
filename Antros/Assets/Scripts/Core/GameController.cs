using ATCG.GameModes;
using ATCG.Multiplayer;
using ATCG.Scenes;
using Helteix.Tools.Phases;
using PrimeTween;
using Unity.Services.Core;
using UnityEngine;

namespace ATCG
{
    public static class GameController
    {
        public static MultiplayerManager MultiplayerManager { get; private set; }
        public static GameSceneController GameSceneController { get; private set; }
        public static GameModeController GameModeController { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialise()
        {
            MultiplayerManager = new MultiplayerManager();
            GameSceneController = new GameSceneController();
            GameModeController = new GameModeController();

            UnityServices.Instance.InitializeAsync();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void PostInitialise()
        {
            PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        }
    }
}