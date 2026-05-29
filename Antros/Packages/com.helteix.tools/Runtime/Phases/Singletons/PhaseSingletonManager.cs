using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Helteix.Tools.Phases
{
    internal static class PhaseSingletonManager
    {
        private static Dictionary<string, SinglePhaseChannel> channels = new();

        public static async Task WaitForSingleInstance(ISinglePhase phaseSingleton, CancellationToken sourceToken)
        {
            SinglePhaseChannel channel = GetOrCreateChannel(phaseSingleton.Channel);

            bool isAlreadyCurrent = channel.EnqueueAndWait(phaseSingleton);

            if (!isAlreadyCurrent)
            {
                while (channel.CurrentPhase != phaseSingleton)
                {
                    channel.Refresh();
                    await Awaitable.NextFrameAsync(sourceToken);
                    sourceToken.ThrowIfCancellationRequested();
                }
            }
        }

        private static SinglePhaseChannel GetOrCreateChannel(string key)
        {
            if (!channels.TryGetValue(key, out SinglePhaseChannel value))
            {
                value = new();
                value.Register();
                channels.Add(key, value);
            }

            return value;
        }
    }
}