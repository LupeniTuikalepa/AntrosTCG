// Assets/Scripts/Battle/CapacitySystem/Core/Cutscenes/AutoBindChannel.cs

using System;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    public sealed class AutoBindChannel
    {
        public readonly string trackName;
        public readonly Type trackType;
        public readonly string displayName;

        // Resolves the object to bind from the runtime context, or null if not
        // bindable (e.g. HeroAnimator for a spell with no caster).
        public readonly Func<CutsceneBindContext, UnityEngine.Object> resolve;

        public static AutoBindChannel Create<T>(
            string trackName,
            Func<CutsceneBindContext, UnityEngine.Object> resolve)
            where T : TrackAsset
        {
            return Create<T>(trackName, trackName, resolve);
        }

        public static AutoBindChannel Create<T>(
            string trackName, string displayName,
            Func<CutsceneBindContext, UnityEngine.Object> resolve)
            where T : TrackAsset
        {
            return new AutoBindChannel(trackName, typeof(T), displayName, resolve);
        }

        private AutoBindChannel(
            string trackName, Type trackType, string displayName,
            Func<CutsceneBindContext, UnityEngine.Object> resolve)
        {
            this.trackName = trackName;
            this.trackType = trackType;
            this.displayName = displayName;
            this.resolve = resolve;
        }
    }
}