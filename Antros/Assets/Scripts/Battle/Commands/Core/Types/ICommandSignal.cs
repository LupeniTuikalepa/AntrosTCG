using System;
using ATCG.Databases;

namespace ATCG.Battle.Commands
{
    public interface ICommandSignal : ICommand
    {
        Guid Channel { get; }
    }
}