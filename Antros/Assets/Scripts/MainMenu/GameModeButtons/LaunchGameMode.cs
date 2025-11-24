using ATCG.GameModes;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.MainMenu.MainMenu.GameModeButtons
{
    public abstract class LaunchGameMode<T, TResult> : MonoBehaviour where T : IGameMode, IPhase<TResult>

    {
        [SerializeField]
        private Button button;

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            GameModeController.Global.OnGameModeBegins += OnGameModeBegins;
            GameModeController.Global.OnGameModeBegins += OnGameModeEnds;
        }


        private void OnDisable()
        {
            GameModeController.Global.OnGameModeBegins -= OnGameModeBegins;
            GameModeController.Global.OnGameModeBegins -= OnGameModeEnds;
        }

        public void OnClicked()
        {
            if(GameModeController.Global.CanStartGameMode())
                _ = Launch();
        }

        private async Awaitable Launch()
        {
            T gameMode = await GetGameMode();
            _ = gameMode.Run();
        }

        protected abstract Awaitable<T> GetGameMode();

        private void OnGameModeBegins(IGameMode gameMode)
        {
            button.interactable = true;
        }

        private void OnGameModeEnds(IGameMode gameMode)
        {
            button.interactable = false;
        }
    }
}