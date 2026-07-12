// Assets/Scripts/Core/Cutscenes/OrbitalRadiusTrack.cs
using System.ComponentModel;
using Unity.Cinemachine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Core.Cutscenes
{
    [DisplayName("ATCG/Cinemachine/Orbital Radius Track")]
    [TrackColor(0.3f, 0.7f, 0.5f)]
    [TrackClipType(typeof(OrbitalRadiusClip))]
    [TrackBindingType(typeof(CinemachineOrbitalFollow))]
    public class OrbitalRadiusTrack : TrackAsset { }
}