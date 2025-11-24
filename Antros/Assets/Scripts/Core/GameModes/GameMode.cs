using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATCG.Multiplayer;
using Helteix.Tools.Phases;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace ATCG.GameModes
{
    public interface IGameMode : IBasePhase
    {
        public const string SESSION_GAME_MODE = "GAME_MODE";
    }

    public abstract class GameMode<T> : IPhase<T>, IGameMode
    {
        public T Result { get; private set; }

        Awaitable<T> IPhase<T>.Execute(CancellationToken token) => Execute(token);
        Awaitable IBasePhase.Initialize(CancellationToken token) => Initialize();
        Awaitable IBasePhase.Dispose(CancellationToken token) => Dispose();


        protected abstract Awaitable Initialize();
        protected abstract Awaitable<T> Execute(CancellationToken token);
        protected abstract Awaitable Dispose();
    }
}