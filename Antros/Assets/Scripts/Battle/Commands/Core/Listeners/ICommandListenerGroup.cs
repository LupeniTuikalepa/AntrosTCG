using System;
using UnityEngine;

namespace ATCG.Battle.Commands.Listeners
{
    public interface ICommandListenerGroup : IDisposable
    {
        /// <summary>
        /// Start command player execution with the given context
        /// </summary>
        /// <param name="context">Execution context for the command</param>
        Awaitable Run(CommandContext context);
    }
}