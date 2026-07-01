using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes
{
    /// <summary>
    /// Placed on the timeline at the instant a step should be processed. Carries
    /// the step NAME (filled by the editor tool's dropdown from the generated
    /// DeclaredSteps, so it can't drift from the code). When the playhead reaches
    /// it, the cutscene relays the name to the director, which relays to the phase;
    /// the phase flushes the QTE stack and runs the matching step once all screens
    /// have reported this marker.
    /// </summary>
    [Serializable]
    public class StepMarker : Marker, INotification
    {
        [SerializeField] private string stepName;
        public string StepName => stepName;

        [SerializeField] private PropertyName id = new PropertyName(Guid.NewGuid().ToString());
        PropertyName INotification.id => id;
    }
}