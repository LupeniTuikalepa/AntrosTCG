using System.Collections.Generic;
using ATCG.Cutscenes;
using System.Linq;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using UnityEngine.Timeline;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Scans a capacity's TimelineAsset for step boundaries (StepMarker positions)
    /// and counts the QTE clips each step will consume at runtime.
    ///
    /// Runtime model (CastCapacityPhase): QTE clips push results onto a stack as they
    /// resolve; when the playhead hits a StepMarker the step runs and ReadQtes() drains
    /// EVERYTHING pushed so far. The next QTE after the marker starts a fresh stack.
    /// So a step's QTE count = clips that RESOLVE in ]previousMarker, thisMarker], i.e.
    /// clips whose window has ENDED by the marker (clip.end &lt;= marker.time) and after
    /// the previous marker. Counting is by clip.end, not clip.start, and the window is
    /// [previous marker, this marker] — not [this marker, next marker].
    /// </summary>
    public static class CapacityTimelineScanner
    {
        public readonly struct Result
        {
            public readonly Dictionary<string, int> QteCountByStep;
            public readonly List<string> Warnings;

            public Result(Dictionary<string, int> qteCountByStep, List<string> warnings)
            {
                QteCountByStep = qteCountByStep;
                Warnings = warnings;
            }
        }

        public static Result Scan(TimelineAsset timeline, IReadOnlyList<string> declaredSteps)
        {
            Dictionary<string, int> counts = new();
            List<string> warnings = new();

            foreach (string step in declaredSteps)
                counts[step] = 0;

            if (timeline == null)
            {
                warnings.Add("No timeline loaded.");
                return new Result(counts, warnings);
            }

            List<(double time, string stepName)> markers = CollectStepMarkers(timeline);
            if (markers.Count == 0)
            {
                warnings.Add("No StepMarker found on the timeline.");
                return new Result(counts, warnings);
            }

            markers.Sort((a, b) => a.time.CompareTo(b.time));
            List<double> clipEnds = CollectQteClipEnds(timeline);

            double previousMarkerTime = double.NegativeInfinity;

            for (int i = 0; i < markers.Count; i++)
            {
                (double time, string stepName) marker = markers[i];

                if (string.IsNullOrEmpty(marker.stepName))
                {
                    warnings.Add($"Unnamed StepMarker at t={marker.time:0.00}s — assign a step in the dropdown.");
                    // A marker still consumes the stack at runtime even if unnamed;
                    // advance the window so later steps aren't over-counted.
                    previousMarkerTime = marker.time;
                    continue;
                }

                if (!counts.ContainsKey(marker.stepName))
                {
                    warnings.Add($"Marker references step '{marker.stepName}' which isn't declared on this capacity.");
                    previousMarkerTime = marker.time;
                    continue;
                }

                // Clips whose window ends within ]previousMarker, thisMarker].
                double windowStart = previousMarkerTime;
                double windowEnd = marker.time;
                counts[marker.stepName] += clipEnds.Count(end => end > windowStart && end <= windowEnd);

                previousMarkerTime = marker.time;
            }

            // Clips resolving AFTER the last marker never get consumed by any step.
            int orphanCount = clipEnds.Count(end => end > previousMarkerTime);
            if (orphanCount > 0)
                warnings.Add($"{orphanCount} QTE clip(s) end after the last StepMarker and will never be consumed.");

            foreach (string step in declaredSteps)
            {
                if (markers.All(m => m.stepName != step))
                    warnings.Add($"Declared step '{step}' has no marker on the timeline (count forced to 0).");
            }

            return new Result(counts, warnings);
        }

        private static List<(double, string)> CollectStepMarkers(TimelineAsset timeline)
        {
            List<(double, string)> result = new();

            void CollectFrom(IEnumerable<UnityEngine.Timeline.IMarker> source)
            {
                foreach (var marker in source)
                {
                    if (marker is StepMarker stepMarker)
                        result.Add((stepMarker.time, stepMarker.StepName));
                }
            }

            if (timeline.markerTrack != null)
                CollectFrom(timeline.markerTrack.GetMarkers());

            foreach (TrackAsset track in timeline.GetOutputTracks())
                CollectFrom(track.GetMarkers());

            return result;
        }

        // QTE windows are consumed when they END, so we key counting on clip.end.
        private static List<double> CollectQteClipEnds(TimelineAsset timeline)
        {
            List<double> result = new();
            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (track is not QteTrack)
                    continue;
                foreach (TimelineClip clip in track.GetClips())
                    result.Add(clip.end);
            }
            return result;
        }
    }
}
