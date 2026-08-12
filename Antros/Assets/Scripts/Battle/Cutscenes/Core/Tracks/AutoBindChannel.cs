using System;
using UnityEngine.Timeline;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Identity of an auto-bindable timeline track: the track name authored on the timeline plus the
    /// track type it must be. This is the generic half of the binding system — how a channel is
    /// *resolved* to an object at runtime is a consumer concern (capacities map each channel to a
    /// caster/camera), kept out of here so the channel vocabulary and the editor rig stay generic.
    /// </summary>
    public sealed class AutoBindChannel
    {
        public readonly string trackName;
        public readonly Type trackType;
        public readonly string displayName;

        public static AutoBindChannel Create<T>(string trackName) where T : TrackAsset
            => Create<T>(trackName, trackName);

        public static AutoBindChannel Create<T>(string trackName, string displayName) where T : TrackAsset
            => new AutoBindChannel(trackName, typeof(T), displayName);

        private AutoBindChannel(string trackName, Type trackType, string displayName)
        {
            this.trackName = trackName;
            this.trackType = trackType;
            this.displayName = displayName;
        }
    }
}
