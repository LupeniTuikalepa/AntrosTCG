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

        // Optional channels don't warn when their track is absent, and their rig object is activated
        // only while the track IS on the timeline (e.g. Target — a target stand-in shown only when used).
        public readonly bool optional;

        public static AutoBindChannel Create<T>(string trackName, bool optional = false) where T : TrackAsset
            => Create<T>(trackName, trackName, optional);

        public static AutoBindChannel Create<T>(string trackName, string displayName, bool optional = false) where T : TrackAsset
            => new AutoBindChannel(trackName, typeof(T), displayName, optional);

        private AutoBindChannel(string trackName, Type trackType, string displayName, bool optional)
        {
            this.trackName = trackName;
            this.trackType = trackType;
            this.displayName = displayName;
            this.optional = optional;
        }
    }
}
