using System;
using UnityEngine;

namespace ATCG.Battle.Commands.Directors
{
    public interface ICommandDirectorGroup : IDisposable
    {
        /// <summary>
        /// Start command player execution with the given context
        /// </summary>
        /// <param name="context">Execution context for the command</param>
        Awaitable Run(CommandContext context);
    }
}