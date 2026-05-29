using System.Threading;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.GameModes
{
    public interface IGameMode : IPhase
    {
        public const string SESSION_GAME_MODE = "GAME_MODE";
    }

    public abstract class GameMode<T> : Phase<T>, IGameMode
    {
        
    }
}