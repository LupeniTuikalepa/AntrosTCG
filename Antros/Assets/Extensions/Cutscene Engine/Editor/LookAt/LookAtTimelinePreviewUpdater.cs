using System.Collections.Generic;
using System.Reflection;
using CutsceneEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CutsceneEngineEditor
{
    [InitializeOnLoad]
    internal static class LookAtTimelinePreviewUpdater
    {
        const double IdlePreviewProbeInterval = 0.25;

        static readonly PropertyInfo TimelineStateProperty =
            typeof(TimelineEditor).GetProperty(
                "state",
                BindingFlags.Static | BindingFlags.NonPublic);
        static readonly PropertyInfo PreviewModeProperty =
            TimelineStateProperty?.PropertyType.GetProperty(
                "previewMode",
                BindingFlags.Instance | BindingFlags.Public);

        static readonly HashSet<LookAtLateUpdateDriver> AppliedDrivers =
            new HashSet<LookAtLateUpdateDriver>();
        static readonly HashSet<LookAtLateUpdateDriver> EligibleDrivers =
            new HashSet<LookAtLateUpdateDriver>();
        static readonly List<LookAtLateUpdateDriver> RemovedDrivers =
            new List<LookAtLateUpdateDriver>();

        static bool _isShutdown;
        static bool _updateRequested = true;
        static double _nextIdlePreviewProbe;

        static LookAtTimelinePreviewUpdater()
        {
            EditorApplication.update += Update;
            SceneView.beforeSceneGui += BeforeSceneGUI;
            Undo.undoRedoPerformed += OnUndoRedo;
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
        }

        internal static bool IsClipActiveAtTime(TimelineClip timelineClip, double time)
        {
            if (timelineClip?.asset is not LookAtClip clip ||
                !clip.HasAnyEffect())
            {
                return false;
            }

            var extrapolatedEnd = timelineClip.extrapolatedStart +
                                  timelineClip.extrapolatedDuration;
            if (time < timelineClip.extrapolatedStart || time >= extrapolatedEnd)
            {
                return false;
            }

            float timelineWeight;
            if (timelineClip.IsPreExtrapolatedTime(time))
            {
                timelineWeight = timelineClip.EvaluateMixIn(timelineClip.start);
            }
            else if (timelineClip.IsPostExtrapolatedTime(time))
            {
                timelineWeight = timelineClip.EvaluateMixOut(timelineClip.end);
            }
            else
            {
                timelineWeight =
                    timelineClip.EvaluateMixIn(time) *
                    timelineClip.EvaluateMixOut(time);
            }

            return timelineWeight > Mathf.Epsilon;
        }

        internal static void NotifyClipChanged(
            PlayableDirector director,
            TimelineClip timelineClip)
        {
            if (!TryGetPreviewContext(out var inspectedDirector, out _) ||
                inspectedDirector != director ||
                timelineClip?.GetParentTrack() is not LookAtTrack track ||
                track.mutedInHierarchy ||
                !IsClipActiveAtTime(timelineClip, director.time))
            {
                SceneView.RepaintAll();
                return;
            }

            RequestBoundDriver(track, director);
            QueuePreviewUpdate();
        }

        internal static void NotifyClipChanged(
            PlayableDirector director,
            LookAtClip clip)
        {
            if (!director || !clip ||
                !TryGetPreviewContext(out var inspectedDirector, out var timeline) ||
                inspectedDirector != director)
            {
                SceneView.RepaintAll();
                return;
            }

            foreach (var outputTrack in timeline.GetOutputTracks())
            {
                if (outputTrack is not LookAtTrack track ||
                    track.mutedInHierarchy)
                {
                    continue;
                }

                foreach (var timelineClip in track.GetClips())
                {
                    if (timelineClip.asset == clip &&
                        IsClipActiveAtTime(timelineClip, director.time))
                    {
                        RequestBoundDriver(track, director);
                        QueuePreviewUpdate();
                        return;
                    }
                }
            }

            SceneView.RepaintAll();
        }

        static void Update()
        {
            var now = EditorApplication.timeSinceStartup;
            if (!ShouldPollPreview(
                    now,
                    AppliedDrivers.Count > 0,
                    _updateRequested,
                    _nextIdlePreviewProbe))
            {
                return;
            }

            _updateRequested = false;
            _nextIdlePreviewProbe = now + IdlePreviewProbeInterval;
            ApplyPreviewChanges(
                forceApply: false,
                requestRepaint: true);
        }

        static void BeforeSceneGUI(SceneView sceneView)
        {
            if (Event.current != null && Event.current.type != EventType.Repaint) return;
            if (AppliedDrivers.Count == 0 && !_updateRequested) return;

            ApplyPreviewChanges(
                forceApply: true,
                requestRepaint: false);
        }

        static void OnUndoRedo()
        {
            if (!TryGetPreviewContext(out var director, out var timeline)) return;

            foreach (var outputTrack in timeline.GetOutputTracks())
            {
                if (outputTrack is LookAtTrack track &&
                    !track.mutedInHierarchy &&
                    HasActiveClip(track, director.time))
                {
                    RequestBoundDriver(track, director);
                }
            }

            QueuePreviewUpdate();
        }

        static void ApplyPreviewChanges(
            bool forceApply,
            bool requestRepaint)
        {
            EligibleDrivers.Clear();
            if (_isShutdown || Application.isPlaying)
            {
                RestoreIneligibleDrivers();
                return;
            }

            if (TryGetPreviewContext(out var director, out var timeline))
            {
                CollectEligibleDrivers(director, timeline);
            }

            var poseChanged = RestoreIneligibleDrivers();
            foreach (var driver in EligibleDrivers)
            {
                if (!driver) continue;

                if (forceApply || !AppliedDrivers.Contains(driver))
                {
                    driver.RequestEditorApply();
                }

                driver.RefreshEditorInputs();
                if (driver.ApplyPendingEditorState())
                {
                    AppliedDrivers.Add(driver);
                    poseChanged = true;
                }
            }

            EligibleDrivers.Clear();
            if (!poseChanged || !requestRepaint) return;

            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

        static void CollectEligibleDrivers(
            PlayableDirector director,
            TimelineAsset timeline)
        {
            foreach (var outputTrack in timeline.GetOutputTracks())
            {
                if (outputTrack is not LookAtTrack track ||
                    track.mutedInHierarchy ||
                    !HasActiveClip(track, director.time))
                {
                    continue;
                }

                var animator = director.GetGenericBinding(track) as Animator;
                if (!animator) continue;

                var driver = animator.GetComponent<LookAtLateUpdateDriver>();
                if (driver && driver.HasActiveEditorStateFor(director.transform))
                {
                    EligibleDrivers.Add(driver);
                }
            }
        }

        static bool RestoreIneligibleDrivers()
        {
            var changed = false;
            RemovedDrivers.Clear();
            foreach (var driver in AppliedDrivers)
            {
                if (driver && EligibleDrivers.Contains(driver)) continue;

                if (driver) changed |= driver.RestoreEditorPose();
                RemovedDrivers.Add(driver);
            }

            for (var i = 0; i < RemovedDrivers.Count; i++)
            {
                AppliedDrivers.Remove(RemovedDrivers[i]);
            }

            RemovedDrivers.Clear();
            return changed;
        }

        static bool TryGetPreviewContext(
            out PlayableDirector director,
            out TimelineAsset timeline)
        {
            director = null;
            timeline = null;
            if (Application.isPlaying || !IsTimelinePreviewMode()) return false;

            director = TimelineEditor.inspectedDirector;
            timeline = TimelineEditor.inspectedAsset;
            return director &&
                   director.isActiveAndEnabled &&
                   timeline &&
                   HasLookAtTrack(timeline);
        }

        static bool IsTimelinePreviewMode()
        {
            if (TimelineStateProperty == null || PreviewModeProperty == null) return false;

            var state = TimelineStateProperty.GetValue(null);
            return state != null &&
                   PreviewModeProperty.GetValue(state) is bool previewMode &&
                   previewMode;
        }

        static bool HasLookAtTrack(TimelineAsset timeline)
        {
            foreach (var outputTrack in timeline.GetOutputTracks())
            {
                if (outputTrack is LookAtTrack) return true;
            }

            return false;
        }

        static bool HasActiveClip(LookAtTrack track, double time)
        {
            foreach (var timelineClip in track.GetClips())
            {
                if (IsClipActiveAtTime(timelineClip, time)) return true;
            }

            return false;
        }



        static void RequestBoundDriver(
            LookAtTrack track,
            PlayableDirector director)
        {
            var animator = director.GetGenericBinding(track) as Animator;
            if (!animator) return;

            var driver = animator.GetComponent<LookAtLateUpdateDriver>();
            if (driver && driver.HasActiveEditorStateFor(director.transform))
            {
                driver.RequestEditorApply();
            }
        }

        internal static bool ShouldPollPreview(
            double now,
            bool hasAppliedDrivers,
            bool updateRequested,
            double nextIdleProbe)
        {
            return hasAppliedDrivers ||
                   updateRequested ||
                   now >= nextIdleProbe;
        }

        internal static void RequestPreviewUpdate()
        {
            if (_isShutdown) return;

            _updateRequested = true;
            EditorApplication.QueuePlayerLoopUpdate();
        }

        static void QueuePreviewUpdate()
        {
            RequestPreviewUpdate();
            SceneView.RepaintAll();
        }

        static void Shutdown()
        {
            if (_isShutdown) return;

            _isShutdown = true;
            EditorApplication.update -= Update;
            SceneView.beforeSceneGui -= BeforeSceneGUI;
            Undo.undoRedoPerformed -= OnUndoRedo;
            AssemblyReloadEvents.beforeAssemblyReload -= Shutdown;
            EditorApplication.quitting -= Shutdown;

            EligibleDrivers.Clear();
            RestoreIneligibleDrivers();
            AppliedDrivers.Clear();
            RemovedDrivers.Clear();
        }
    }
}
