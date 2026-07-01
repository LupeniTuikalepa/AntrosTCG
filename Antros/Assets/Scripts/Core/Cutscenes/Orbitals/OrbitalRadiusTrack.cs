// Assets/Scripts/Core/Cutscenes/OrbitalRadiusTrack.cs
using Unity.Cinemachine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Core.Cutscenes
{
    [TrackColor(0.3f, 0.7f, 0.5f)]
    [TrackClipType(typeof(OrbitalRadiusClip))]
    [TrackBindingType(typeof(CinemachineOrbitalFollow))]
    public class OrbitalRadiusTrack : TrackAsset { }
}