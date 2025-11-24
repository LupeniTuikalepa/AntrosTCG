using System;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.GameModes
{
    public class GameModeController : IPhaseListener<IGameMode>, IDisposable
    {
        public static GameModeController Global => GameController.GameModeController;
        public IGameMode Current { get; private set; }

        public event Action<IGameMode> OnGameModeBegins;

        public event Action<IGameMode> OnGameModeEnds;

        public GameModeController()
        {
            this.Register();
        }

        void IPhaseListener<IGameMode>.OnPhaseBegin(IGameMode phase)
        {
            if (Current != null)
            {
                Debug.LogError("Beginning a new game mode while one is not done yet. Cancelling the running one");
                Current.Cancel();
            }

            OnGameModeBegins?.Invoke(Current);
            Current = phase;
        }

        void IPhaseListener<IGameMode>.OnPhaseEnd(IGameMode phase)
        {
            if (Current == phase)
            {
                OnGameModeEnds?.Invoke(Current);
                Current = null;
            }
        }

        public bool CanStartGameMode() => Current == null;

        public void Dispose()
        {
            this.Unregister();
        }
    }
}