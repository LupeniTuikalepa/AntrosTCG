using UnityEngine;

namespace ATCG.Battle.Commands.Directors
{
    public interface ICommandDirector { }
    public interface ICommandDirector<in T> : ICommandDirector where T : ICommand
    {
        bool CanPlay(T command) => true;

        Awaitable Play(CommandDirectorState state, CommandContext context, T command);
        void OnBegin(in CommandDirectorState state, CommandContext context, T command) { }
        void OnHit(in CommandDirectorState state, CommandContext context, T command) { }
        void OnEnd(in CommandDirectorState state, CommandContext context, T command) { }
    }
}