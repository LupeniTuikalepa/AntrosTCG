using System;
using UnityEngine;

namespace ATCG.Utilities
{
    public static class AwaitablesUtilities
    {
        public static void RunAsync(this Func<Awaitable> awaitable)
        {
            try
            {
                _ = awaitable();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}