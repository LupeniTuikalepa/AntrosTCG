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

        // Resolves the object to bind from the editor's DebugCutsceneRig. Used by
        // the capacity timeline editor tool to preview bindings outside Play Mode.
        public readonly Func<DebugCutsceneRig, UnityEngine.Object> resolveDebug;

        public static AutoBindChannel Create<T>(
            string trackName,
            Func<CutsceneBindContext, UnityEngine.Object> resolve,
            Func<DebugCutsceneRig, UnityEngine.Object> resolveDebug)
            where T : TrackAsset
        {
            return Create<T>(trackName, trackName, resolve, resolveDebug);
        }

        public static AutoBindChannel Create<T>(
            string trackName, string displayName,
            Func<CutsceneBindContext, UnityEngine.Object> resolve,
            Func<DebugCutsceneRig, UnityEngine.Object> resolveDebug)
            where T : TrackAsset
        {
            return new AutoBindChannel(trackName, typeof(T), displayName, resolve, resolveDebug);
        }

        private AutoBindChannel(
            string trackName, Type trackType, string displayName,
            Func<CutsceneBindContext, UnityEngine.Object> resolve,
            Func<DebugCutsceneRig, UnityEngine.Object> resolveDebug)
        {
            this.trackName = trackName;
            this.trackType = trackType;
            this.displayName = displayName;
            this.resolve = resolve;
            this.resolveDebug = resolveDebug;
        }
    }
}
