using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.GameCommands.Capacities;
using ATCG.Cutscenes;
using UnityEngine;

namespace ATCG.Battle.Cutscenes
{
    /// <summary>
    /// A scoped command director that collects QTE results into an accumulator while a QTE-bearing
    /// cutscene plays. Registered on play and unregistered after, it receives every <see cref="QteCommand"/>
    /// on BOTH screens (they're replicated, deterministic), so the accumulator holds the same values
    /// everywhere and a step handler reads the same averaged effectiveness. Mirrors what
    /// CastCapacityPhase does for capacities, generalised for any cutscene.
    /// </summary>
    public sealed class QteResultCollector : ICommandDirector<QteCommand>
    {
        private readonly QteResultAccumulator accumulator;

        public QteResultCollector(QteResultAccumulator accumulator) => this.accumulator = accumulator;

        void ICommandDirector<QteCommand>.OnBegin(in CommandDirectorState state, CommandContext context, QteCommand command)
            => accumulator.Add(command.Result);

        async Awaitable ICommandDirector<QteCommand>.Play(CommandDirectorState state, CommandContext context, QteCommand command)
        {
            state.CompleteAll(this);
            await Awaitable.MainThreadAsync();
        }
    }
}
