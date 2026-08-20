using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CutsceneEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CutsceneEngineEditor
{
    [CustomTimelineEditor(typeof(LookAtClip))]
    public sealed class LookAtClipEditor : ClipEditor
    {
        const float AccentHeight = 3f;

        sealed class BlinkMarkerCache
        {
            public double StartTime;
            public double EndTime;
            public float Frequency;
            public float NoiseOffset;
            public readonly List<double> TriggerTimes =
                new List<double>();
        }

        static readonly ConditionalWeakTable<TimelineClip, BlinkMarkerCache>
            BlinkMarkerCaches =
                new ConditionalWeakTable<TimelineClip, BlinkMarkerCache>();

        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            if (clip.asset is not LookAtClip lookAtClip ||
                track is not LookAtTrack lookAtTrack ||
                clonedFrom != null)
            {
                return;
            }

            lookAtClip.position = LookAtClip.DefaultLocalPosition;

            var director = TimelineEditor.inspectedDirector;
            if (director)
            {
                var animator = director.GetGenericBinding(lookAtTrack) as Animator;
                
                if (LookAtUtility.TryGetEyeCenter(
                        animator,
                        lookAtTrack,
                        out var eyeCenter))
                {
                    lookAtClip.position = LookAtUtility.GetEyeForwardLocalPosition(
                        director.transform,
                        eyeCenter);
                }

                if (animator)
                {
                    LookAtClipInspector.AutoDetectBlendShapeKeys(new []{(lookAtClip, animator)}, LookAtClipInspector.BlendShapeKeyRole.Blink);
                    LookAtClipInspector.AutoDetectBlendShapeKeys(new []{(lookAtClip, animator)}, LookAtClipInspector.BlendShapeKeyRole.UpperEyelidFollow);
                    LookAtClipInspector.AutoDetectBlendShapeKeys(new []{(lookAtClip, animator)}, LookAtClipInspector.BlendShapeKeyRole.LowerEyelidFollow);
                    LookAtClipInspector.AutoDetectBlendShapeKeys(new []{(lookAtClip, animator)}, LookAtClipInspector.BlendShapeKeyRole.HorizontalEyelidFollow);
                }
            }

            EditorUtility.SetDirty(lookAtClip);
        }

        public override void DrawBackground(
            TimelineClip clip,
            ClipBackgroundRegion region)
        {
            if (clip.asset is not LookAtClip lookAtClip) return;

            if (region.position.width > 0f && region.position.height > 0f)
            {
                LookAtTimelineGizmoRegistry.RegisterVisibleClip(clip, lookAtClip);
            }

            var averageWeight = Mathf.Clamp01(
                (lookAtClip.eyesWeight +
                 lookAtClip.headWeight +
                 lookAtClip.neckWeight +
                 lookAtClip.bodyWeight) * 0.25f);
            var color = Color.Lerp(
                new Color(0.16f, 0.10f, 0.24f, 0.35f),
                new Color(0.62f, 0.38f, 0.95f, 0.58f),
                averageWeight);
            EditorGUI.DrawRect(region.position, color);
            DrawClipAccent(region.position, lookAtClip);
            DrawAutomaticBlinkMarkers(clip, lookAtClip, in region);
        }

        static void DrawClipAccent(Rect clipRect, LookAtClip clip)
        {
            var height = Mathf.Min(AccentHeight, clipRect.height);
            if (height <= 0f) return;

            EditorGUI.DrawRect(
                new Rect(
                    clipRect.xMin,
                    clipRect.yMax - height,
                    clipRect.width,
                    height),
                LookAtTimelineGizmoRegistry.ResolveGizmoColor(clip, 1f));
        }

        static void DrawAutomaticBlinkMarkers(
            TimelineClip timelineClip,
            LookAtClip clip,
            in ClipBackgroundRegion region)
        {
            if (clip.blinkMode != LookAtBlinkMode.Automatic ||
                !clip.HasBlinkConfiguration() ||
                region.position.width <= 0f ||
                region.position.height <= 2f ||
                region.endTime <= region.startTime)
            {
                return;
            }

            var triggerTimes = GetAutomaticBlinkTriggerTimes(
                timelineClip,
                clip,
                in region);
            var lineColor = new Color(0.92f, 0.98f, 1f, 0.9f);
            var spanColor = new Color(0.70f, 0.90f, 1f, 0.3f);
            var blinkDuration = Mathf.Clamp(
                clip.blinkDuration,
                LookAtClip.MinimumAutomaticBlinkDuration,
                LookAtClip.MaximumAutomaticBlinkDuration);

            for (var i = 0; i < triggerTimes.Count; i++)
            {
                var triggerTime = triggerTimes[i];
                var normalizedStart = (float)(
                    (triggerTime - region.startTime) /
                    (region.endTime - region.startTime));
                var pulseEndTime = System.Math.Min(
                    triggerTime + blinkDuration,
                    region.endTime);
                var normalizedEnd = (float)(
                    (pulseEndTime - region.startTime) /
                    (region.endTime - region.startTime));
                var markerX = Mathf.Lerp(
                    region.position.xMin,
                    region.position.xMax,
                    normalizedStart);
                var pulseEndX = Mathf.Lerp(
                    region.position.xMin,
                    region.position.xMax,
                    normalizedEnd);

                EditorGUI.DrawRect(
                    new Rect(
                        markerX,
                        region.position.yMax - 3f,
                        Mathf.Max(1f, pulseEndX - markerX),
                        3f),
                    spanColor);
                EditorGUI.DrawRect(
                    new Rect(
                        markerX - 0.75f,
                        region.position.yMin + 1f,
                        1.5f,
                        region.position.height - 2f),
                    lineColor);
            }
        }

        static List<double> GetAutomaticBlinkTriggerTimes(
            TimelineClip timelineClip,
            LookAtClip clip,
            in ClipBackgroundRegion region)
        {
            var cache = BlinkMarkerCaches.GetValue(
                timelineClip, _ => new BlinkMarkerCache());

            var frequency = Mathf.Clamp01(clip.blinkFrequency);
            var noiseOffset =
                LookAtUtility.SanitizeBlinkNoiseOffset(
                    clip.blinkNoiseOffset);
            if (cache.StartTime != region.startTime ||
                cache.EndTime != region.endTime ||
                !Mathf.Approximately(cache.Frequency, frequency) ||
                !Mathf.Approximately(cache.NoiseOffset, noiseOffset))
            {
                cache.StartTime = region.startTime;
                cache.EndTime = region.endTime;
                cache.Frequency = frequency;
                cache.NoiseOffset = noiseOffset;
                LookAtUtility.CollectAutomaticBlinkTriggerTimes(
                    region.startTime,
                    region.endTime,
                    frequency,
                    noiseOffset,
                    cache.TriggerTimes);
            }

            return cache.TriggerTimes;
        }

    }

    // Scene gizmo visibility follows Timeline clip GUI drawing instead of
    // Inspector selection. DrawBackground refreshes visible entries, which
    // expire after the Timeline closes or the clip scrolls out of view.
    [InitializeOnLoad]
    internal static class LookAtTimelineGizmoRegistry
    {
        internal const double VisibilityTimeout = 0.75d;
        internal const float UnselectedOpacityMultiplier = 0.35f;
        const double TimelineRefreshInterval = 0.2d;

        sealed class VisibleClip
        {
            public TimelineClip TimelineClip;
            public LookAtClip Clip;
            public LookAtTrack Track;
            public PlayableDirector Director;
            public double LastSeen;
        }

        static readonly Dictionary<TimelineClip, VisibleClip> VisibleClips =
            new Dictionary<TimelineClip, VisibleClip>();
        static readonly List<TimelineClip> StaleClips = new List<TimelineClip>();
        static readonly List<VisibleClip> DrawClips = new List<VisibleClip>();
        static readonly List<TimelineClip> PreviousSelection = new List<TimelineClip>();

        static PlayableDirector _activeDirector;
        static double _nextTimelineRefresh;
        static bool _isShutdown;

        static LookAtTimelineGizmoRegistry()
        {
            SceneView.duringSceneGui += DuringSceneGUI;
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
        }

        internal static void RegisterVisibleClip(TimelineClip timelineClip, LookAtClip clip)
        {
            var timelineWindow = TimelineEditor.GetWindow();
            var director = TimelineEditor.inspectedDirector;
            var track = timelineClip?.GetParentTrack() as LookAtTrack;
            if (!timelineWindow || !director || timelineClip == null || !clip || !track)
            {
                return;
            }

            if (_activeDirector != director)
            {
                ClearEntries();

                _activeDirector = director;
            }

            LookAtTimelinePreviewUpdater.RequestPreviewUpdate();

            var now = EditorApplication.timeSinceStartup;
            if (VisibleClips.TryGetValue(timelineClip, out var entry))
            {
                entry.Clip = clip;
                entry.Track = track;
                entry.Director = director;
                entry.LastSeen = now;
                return;
            }

            VisibleClips.Add(
                timelineClip,
                new VisibleClip
                {
                    TimelineClip = timelineClip,
                    Clip = clip,
                    Track = track,
                    Director = director,
                    LastSeen = now
                });
            SceneView.RepaintAll();
        }

        internal static float GetOpacityMultiplier(bool isSelected)
        {
            return isSelected ? 1f : UnselectedOpacityMultiplier;
        }

        internal static Color ResolveGizmoColor(
            LookAtClip clip,
            float opacityMultiplier)
        {
            var color = clip ? clip.gizmoColor : LookAtClip.DefaultGizmoColor;
            color.a *= Mathf.Clamp01(opacityMultiplier);
            return color;
        }

        internal static bool IsWithinVisibilityWindow(double now, double lastSeen)
        {
            return now - lastSeen <= VisibilityTimeout;
        }

        static void Update()
        {
            var timelineWindow = TimelineEditor.GetWindow();
            var director = TimelineEditor.inspectedDirector;
            if (!timelineWindow || !director)
            {
                if (VisibleClips.Count > 0 || _activeDirector)
                {
                    ClearEntries();
                    SceneView.RepaintAll();
                }

                return;
            }

            if (_activeDirector != director)
            {
                ClearEntries();
                _activeDirector = director;
                SceneView.RepaintAll();
            }

            var now = EditorApplication.timeSinceStartup;
            var changed = RemoveStaleEntries(now, director);
            if (VisibleClips.Count > 0)
            {
                if (now >= _nextTimelineRefresh)
                {
                    _nextTimelineRefresh = now + TimelineRefreshInterval;
                    timelineWindow.Repaint();
                }

                changed |= UpdateSelectionCache(TimelineEditor.selectedClips);
            }
            else if (PreviousSelection.Count > 0)
            {
                PreviousSelection.Clear();
                changed = true;
            }

            if (changed)
            {
                SceneView.RepaintAll();
            }
        }

        static bool RemoveStaleEntries(double now, PlayableDirector director)
        {
            StaleClips.Clear();
            foreach (var pair in VisibleClips)
            {
                var entry = pair.Value;
                if (!entry.Clip ||
                    !entry.Track ||
                    entry.Director != director ||
                    !IsWithinVisibilityWindow(now, entry.LastSeen))
                {
                    StaleClips.Add(pair.Key);
                }
            }

            for (var i = 0; i < StaleClips.Count; i++)
            {
                VisibleClips.Remove(StaleClips[i]);
            }

            var removedAny = StaleClips.Count > 0;
            StaleClips.Clear();
            return removedAny;
        }

        static bool UpdateSelectionCache(TimelineClip[] selectedClips)
        {
            selectedClips ??= System.Array.Empty<TimelineClip>();
            if (PreviousSelection.Count == selectedClips.Length)
            {
                var unchanged = true;
                for (var i = 0; i < selectedClips.Length; i++)
                {
                    if (!ReferenceEquals(PreviousSelection[i], selectedClips[i]))
                    {
                        unchanged = false;
                        break;
                    }
                }

                if (unchanged) return false;
            }

            PreviousSelection.Clear();
            PreviousSelection.AddRange(selectedClips);
            return true;
        }

        static void DuringSceneGUI(SceneView sceneView)
        {
            if (!_activeDirector ||
                TimelineEditor.inspectedDirector != _activeDirector)
            {
                return;
            }

            DrawClips.Clear();
            var now = EditorApplication.timeSinceStartup;
            foreach (var entry in VisibleClips.Values)
            {
                if (entry.Director == _activeDirector &&
                    entry.Clip &&
                    entry.Track &&
                    IsWithinVisibilityWindow(now, entry.LastSeen))
                {
                    DrawClips.Add(entry);
                }
            }

            if (DrawClips.Count == 0) return;

            var selectedClips = TimelineEditor.selectedClips;
            try
            {
                DrawGizmoPass(selectedClips, drawSelected: false);
                DrawGizmoPass(selectedClips, drawSelected: true);
            }
            finally
            {
                DrawClips.Clear();
            }
        }

        static void DrawGizmoPass(TimelineClip[] selectedClips, bool drawSelected)
        {
            for (var i = 0; i < DrawClips.Count; i++)
            {
                var entry = DrawClips[i];
                var isSelected = ContainsReference(selectedClips, entry.TimelineClip);
                if (isSelected != drawSelected) continue;

                LookAtClipInspector.DrawTargetGizmo(
                    entry.TimelineClip,
                    entry.Clip,
                    entry.Director,
                    entry.Track,
                    GetOpacityMultiplier(isSelected),
                    drawPositionHandle: isSelected);
            }
        }

        static bool ContainsReference(TimelineClip[] selectedClips, TimelineClip candidate)
        {
            if (selectedClips == null) return false;
            for (var i = 0; i < selectedClips.Length; i++)
            {
                if (ReferenceEquals(selectedClips[i], candidate)) return true;
            }

            return false;
        }

        static void ClearEntries()
        {
            VisibleClips.Clear();
            StaleClips.Clear();
            DrawClips.Clear();
            PreviousSelection.Clear();
            _activeDirector = null;
            _nextTimelineRefresh = 0d;
        }

        static void Shutdown()
        {
            if (_isShutdown) return;
            _isShutdown = true;
            SceneView.duringSceneGui -= DuringSceneGUI;
            EditorApplication.update -= Update;
            AssemblyReloadEvents.beforeAssemblyReload -= Shutdown;
            EditorApplication.quitting -= Shutdown;
            ClearEntries();
        }
    }
}
