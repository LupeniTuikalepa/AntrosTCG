using System;

namespace ATCG.Battle.Commands.Core.Exceptions
{
    public class BreakCommandException : Exception
    {
        public override string Message => Cause;
        
        public string Cause { get; }

        public BreakCommandException(string cause)
        {
            Cause = cause;
        }
    }
}