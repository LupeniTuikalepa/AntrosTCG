using ATCG.Battle.Entities.Runtime.Animations;
using Unity.Cinemachine;
using UnityEngine.Timeline;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// The shared vocabulary of auto-bindable cutscene channels — their names and the track types
    /// they map to. Generic on purpose: the editor rig fills these by name, and the authoring stage
    /// reads them, without knowing how any consumer resolves a channel to a live object at runtime
    /// (that lives consumer-side, e.g. the capacity binding resolver).
    /// </summary>
    public static class CutsceneChannels
    {
        public static readonly AutoBindChannel HeroAnimator =
            AutoBindChannel.Create<AnimationTrack>("HeroAnimator");

        public static readonly AutoBindChannel MainCamera =
            AutoBindChannel.Create<CinemachineTrack>("MainCamera");

        public static readonly AutoBindChannel[] All = { HeroAnimator, MainCamera };

        public static bool IsAutoBindableTrack(TrackAsset track)
        {
            for (int i = 0; i < All.Length; i++)
            {
                AutoBindChannel channel = All[i];
                if (track.name != channel.trackName)
                    continue;

                if (track.GetType() == channel.trackType)
                    return true;

                UnityEngine.Debug.LogWarning(
                    $"The track {track.name} is not of type {channel.trackType} but {track.GetType()}");
                return false;
            }
            return false;
        }
    }
}
