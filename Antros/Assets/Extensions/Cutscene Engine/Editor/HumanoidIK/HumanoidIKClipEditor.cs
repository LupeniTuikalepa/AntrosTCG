using System.Collections.Generic;
using CutsceneEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

namespace CutsceneEngineEditor
{
    [CustomTimelineEditor(typeof(HumanoidIKClip))]
    public class HumanoidIKClipEditor : ClipEditor
    {
        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            if (clip.asset is not HumanoidIKClip humanoidClip ||
                track is not HumanoidIKTrack humanoidTrack)
            {
                return;
            }

            var director = TimelineEditor.inspectedDirector;
            if (!director) return;

            if (clonedFrom != null)
            {
                HumanoidIKClipSpaceMigration.EnsureDirectorLocal(humanoidClip, director);
                return;
            }

            var directorTransform = director.transform;
            humanoidClip.position = Vector3.zero;
            humanoidClip.SetTargetWorldRotation(directorTransform, directorTransform.rotation);

            var defaultBendTarget = HumanoidIKUtility.IsFoot(humanoidTrack.target)
                ? new Vector3(0f, 0.5f, 1.2f)
                : new Vector3(0f, 1.2f, 1.2f);

            humanoidClip.SetHumanoidPoleWorldVector(directorTransform, directorTransform.TransformPoint(defaultBendTarget));

            var animator = director.GetGenericBinding(humanoidTrack) as Animator;
            if (TryGetCurrentTargetWorldPose(
                    animator,
                    humanoidTrack.target,
                    out var worldPosition,
                    out var worldRotation,
                    out var worldBendTarget,
                    out var hasWorldRotation))
            {
                humanoidClip.position = directorTransform.InverseTransformPoint(worldPosition);
                humanoidClip.SetHumanoidPoleWorldVector(directorTransform, worldBendTarget);
                if (hasWorldRotation)
                {
                    humanoidClip.SetTargetWorldRotation(directorTransform, worldRotation);
                }
            }

            EditorUtility.SetDirty(humanoidClip);
        }

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            var humanoidClip = clip.asset as HumanoidIKClip;
            if (!humanoidClip) return;

            var rect = region.position;
            if (rect.width > 0f && rect.height > 0f)
            {
                HumanoidIKTimelineGizmoRegistry.RegisterVisibleClip(clip, humanoidClip);
            }

            var weight = Mathf.Clamp01((humanoidClip.positionWeight + humanoidClip.rotationWeight) * 0.5f);
            var color = Color.Lerp(new Color(0.08f, 0.2f, 0.25f, 0.35f), new Color(0.2f, 0.75f, 1f, 0.55f), weight);
            EditorGUI.DrawRect(rect, color);
        }

        static bool TryGetCurrentTargetWorldPose(
            Animator animator,
            HumanoidIKTarget target,
            out Vector3 worldPosition,
            out Quaternion worldRotation,
            out Vector3 worldBendTarget,
            out bool hasWorldRotation)
        {
            worldPosition = default;
            worldRotation = Quaternion.identity;
            worldBendTarget = default;
            hasWorldRotation = false;
            if (!HumanoidIKUtility.TryGetLimbBones(animator, target, out var limb)) return false;

            worldPosition = limb.End.position;
            var forwardDir = animator ? animator.transform.forward : Vector3.forward;
            if (HumanoidIKUtility.IsHand(target)) forwardDir *= -1f;
            worldBendTarget = limb.Lower.position + forwardDir * 0.5f;
            if (!HumanoidIKHumanPoseSolver.TryCreate(animator, out var poseSolver)) return true;
            using (poseSolver)
            {
                if (!poseSolver.TryGetBoneToEffectorRotation(target, out var boneToEffectorRotation))
                {
                    return true;
                }

                worldRotation = limb.End.rotation * boneToEffectorRotation;
                hasWorldRotation = true;
                return true;
            }
        }
    }

    internal static class HumanoidIKClipSpaceMigration
    {
        internal static bool EnsureDirectorLocal(
            HumanoidIKClip clip,
            PlayableDirector director)
        {
            if (!clip || !director ||
                !clip.EnsureDirectorLocalDefaultAnchor(director.transform))
            {
                return false;
            }

            EditorUtility.SetDirty(clip);
            return true;
        }

        internal static bool EnsureTimelineDirectorLocal(PlayableDirector director)
        {
            var timeline = director ? director.playableAsset as TimelineAsset : null;
            if (!timeline) return false;

            var changed = false;
            foreach (var rootTrack in timeline.GetRootTracks())
            {
                changed |= EnsureTrackDirectorLocal(rootTrack, director);
            }

            return changed;
        }

        static bool EnsureTrackDirectorLocal(
            TrackAsset track,
            PlayableDirector director)
        {
            var changed = false;
            if (track is HumanoidIKTrack)
            {
                foreach (var timelineClip in track.GetClips())
                {
                    if (timelineClip.asset is HumanoidIKClip clip)
                    {
                        changed |= EnsureDirectorLocal(clip, director);
                    }
                }
            }

            foreach (var childTrack in track.GetChildTracks())
            {
                changed |= EnsureTrackDirectorLocal(childTrack, director);
            }

            return changed;
        }
    }

    // Scene preview visibility follows Timeline clip GUI drawing, not Unity object
    // selection. DrawBackground refreshes each visible clip; stale entries disappear
    // after the Timeline closes or stops drawing that clip.
    [InitializeOnLoad]
    internal static class HumanoidIKTimelineGizmoRegistry
    {
        internal const double VisibilityTimeout = 0.75;
        internal const float UnselectedOpacityMultiplier = 0.5f;
        internal const float TargetFrameBoundsSize = 0.3f;
        const double TimelineRefreshInterval = 0.2;
        const string FrameSelectedCommandName = "FrameSelected";

        sealed class VisibleClip
        {
            public TimelineClip TimelineClip;
            public HumanoidIKClip Clip;
            public HumanoidIKTrack Track;
            public PlayableDirector Director;
            public UnityEngine.Object Binding;
            public double LastSeen;
        }

        static readonly Dictionary<TimelineClip, VisibleClip> VisibleClips =
            new Dictionary<TimelineClip, VisibleClip>();
        static readonly List<TimelineClip> StaleClips = new List<TimelineClip>();
        static readonly List<VisibleClip> DrawClips = new List<VisibleClip>();
        static readonly List<TimelineClip> PreviousSelection = new List<TimelineClip>();
        static readonly HumanoidIKGizmoDrawer GizmoDrawer = new HumanoidIKGizmoDrawer();

        static PlayableDirector _activeDirector;
        static PlayableDirector _migratedDirector;
        static double _nextTimelineRefresh;
        static bool _isShutdown;

        static HumanoidIKTimelineGizmoRegistry()
        {
            // DrawMeshNow submitted from beforeSceneGui is cleared by the Scene
            // camera render. Preview geometry must be submitted duringSceneGui.
            SceneView.duringSceneGui += DuringSceneGUI;
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
        }

        internal static void RegisterVisibleClip(
            TimelineClip timelineClip,
            HumanoidIKClip clip)
        {
            var timelineWindow = TimelineEditor.GetWindow();
            var director = TimelineEditor.inspectedDirector;
            var track = timelineClip?.GetParentTrack() as HumanoidIKTrack;
            if (!timelineWindow || !director || timelineClip == null || !clip || !track)
            {
                return;
            }

            if (_activeDirector != director)
            {
                ClearEntries();
                _activeDirector = director;
            }

            var binding = director.GetGenericBinding(track);
            var now = EditorApplication.timeSinceStartup;
            if (VisibleClips.TryGetValue(timelineClip, out var entry))
            {
                if (entry.Binding != binding)
                {
                    GizmoDrawer.ClearPreviewContexts();
                }

                entry.Clip = clip;
                entry.Track = track;
                entry.Director = director;
                entry.Binding = binding;
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
                    Binding = binding,
                    LastSeen = now
                });
            SceneView.RepaintAll();
        }

        internal static bool IsVisible(HumanoidIKClip clip)
        {
            if (!clip ||
                !TimelineEditor.GetWindow() ||
                TimelineEditor.inspectedDirector != _activeDirector)
            {
                return false;
            }

            var now = EditorApplication.timeSinceStartup;
            foreach (var entry in VisibleClips.Values)
            {
                if (entry.Clip == clip &&
                    entry.Director == _activeDirector &&
                    IsWithinVisibilityWindow(now, entry.LastSeen))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsSelectedAndVisible(HumanoidIKClip clip)
        {
            if (!IsVisible(clip)) return false;

            var selectedClips = TimelineEditor.selectedClips;
            foreach (var entry in VisibleClips.Values)
            {
                if (entry.Clip == clip &&
                    entry.Director == _activeDirector &&
                    ContainsReference(selectedClips, entry.TimelineClip))
                {
                    return true;
                }
            }

            return false;
        }


        internal static float GetOpacityMultiplier(bool isSelected)
        {
            return isSelected ? 1f : UnselectedOpacityMultiplier;
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

            if (_migratedDirector != director)
            {
                _migratedDirector = director;
                if (HumanoidIKClipSpaceMigration.EnsureTimelineDirectorLocal(director))
                {
                    TimelineEditor.Refresh(RefreshReason.ContentsModified);
                    SceneView.RepaintAll();
                }
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
            var currentEvent = Event.current;
            if (TryHandleFrameSelected(sceneView, currentEvent))
            {
                return;
            }

            if (currentEvent?.type != EventType.Repaint ||
                !_activeDirector ||
                TimelineEditor.inspectedDirector != _activeDirector)
            {
                return;
            }
            var selectedClips = TimelineEditor.selectedClips;
            if(!EditorUtil.IsPreviewEnabled() && selectedClips.Length == 0) return;

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


            var previousZTest = Handles.zTest;
            var previousColor = Handles.color;
            var previousMatrix = Handles.matrix;
            try
            {
                Handles.matrix = Matrix4x4.identity;
                Handles.zTest = CompareFunction.LessEqual;
                DrawPreviewBatch(selectedClips, drawSelected: false);
                DrawPreviewBatch(selectedClips, drawSelected: true);
            }
            finally
            {
                GizmoDrawer.CancelFrame();
                Handles.matrix = previousMatrix;
                Handles.zTest = previousZTest;
                Handles.color = previousColor;
                DrawClips.Clear();
            }
        }

        static bool TryHandleFrameSelected(SceneView sceneView, Event currentEvent)
        {
            if (!sceneView ||
                !IsFrameSelectedCommand(currentEvent) ||
                !_activeDirector ||
                TimelineEditor.inspectedDirector != _activeDirector ||
                !TryGetSelectedFrameBounds(out var bounds) ||
                !sceneView.Frame(bounds, EditorApplication.isPlaying))
            {
                return false;
            }

            currentEvent.Use();
            return true;
        }

        static bool TryGetSelectedFrameBounds(out Bounds bounds)
        {
            bounds = default;
            var selectedClips = TimelineEditor.selectedClips;
            if (selectedClips == null || selectedClips.Length == 0) return false;

            var hasBounds = false;
            var now = EditorApplication.timeSinceStartup;
            for (var i = 0; i < selectedClips.Length; i++)
            {
                var timelineClip = selectedClips[i];
                if (timelineClip == null ||
                    !VisibleClips.TryGetValue(timelineClip, out var entry) ||
                    entry.Director != _activeDirector ||
                    !entry.Clip ||
                    !entry.Track ||
                    !IsWithinVisibilityWindow(now, entry.LastSeen) ||
                    !GizmoDrawer.TryResolveClipPreview(
                        entry.Clip,
                        entry.Director,
                        entry.Track,
                        1f,
                        out var pose))
                {
                    continue;
                }

                var targetBounds = GetTargetFrameBounds(pose.TargetPosition);
                if (hasBounds)
                {
                    bounds.Encapsulate(targetBounds);
                }
                else
                {
                    bounds = targetBounds;
                    hasBounds = true;
                }
            }

            return hasBounds;
        }

        internal static bool IsFrameSelectedCommand(Event currentEvent)
        {
            return currentEvent != null &&
                   currentEvent.type == EventType.ExecuteCommand &&
                   currentEvent.commandName == FrameSelectedCommandName;
        }

        internal static Bounds GetTargetFrameBounds(Vector3 targetPosition)
        {
            return new Bounds(targetPosition, Vector3.one * TargetFrameBoundsSize);
        }

        static void DrawPreviewBatch(
            TimelineClip[] selectedClips,
            bool drawSelected)
        {
            GizmoDrawer.BeginFrame(EventType.Repaint);
            try
            {
                for (var i = 0; i < DrawClips.Count; i++)
                {
                    var entry = DrawClips[i];
                    var isSelected = ContainsReference(selectedClips, entry.TimelineClip);
                    if (isSelected != drawSelected) continue;

                    if (GizmoDrawer.TryResolveClipPreview(
                            entry.Clip,
                            entry.Director,
                            entry.Track,
                            GetOpacityMultiplier(isSelected),
                            out var pose))
                    {
                        GizmoDrawer.DrawClipPreview(entry.Clip, in pose);
                    }
                }

                GizmoDrawer.FlushFrame();
            }
            finally
            {
                GizmoDrawer.CancelFrame();
            }
        }

        static bool ContainsReference(
            TimelineClip[] selectedClips,
            TimelineClip candidate)
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
            GizmoDrawer.ClearPreviewContexts();
            _activeDirector = null;
            _migratedDirector = null;
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
            GizmoDrawer.Dispose();
        }
    }
}