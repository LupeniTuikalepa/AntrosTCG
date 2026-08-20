# Cutscene Core

## Purpose

`Cutscene` wraps Unity `PlayableDirector` playback with cutscene-specific state, events, marker navigation, loop escape support, audio listener switching, and runtime binding helpers.

## Source files

- Runtime: `Runtime/Cutscene.cs`
- State enum: `Runtime/CutsceneState.cs`
- Marker type: `Runtime/CutsceneMarker.cs`
- Inspector: `Editor/CutsceneInspector.cs`
- Marker editor: `Editor/CutsceneMarkerEditor.cs`, `Editor/CutsceneMarkerInspector.cs`

## Required setup

1. Add `Cutscene` to a GameObject that also has `PlayableDirector`.
2. Assign the `PlayableDirector` to `Cutscene.director`, or let `Reset` and `Awake` find it on the same GameObject.
3. Assign a Timeline asset to the director.
4. Bind Timeline tracks to scene objects as required by each track document.

## Runtime state

- `state` returns `Playing`, `Paused`, or `None`.
- `time` mirrors `PlayableDirector.time`.
- `duration` mirrors `PlayableDirector.duration`.
- `completionCount` increments when playback reaches the end.
- `loopCount` tracks `PlayableDirector` wrap-mode loops, not `LoopClip` repeats.
- `reachedTheEnd` is true while playing and the director time is at the end.
- `activeCutscenes` contains cutscenes that have entered playback and have not yet stopped.

## Events

- Static callbacks: `onCutsceneStarted`, `onCutscenePaused`, `onCutsceneStopped`.
- Instance callbacks: `onStateChanged`, `onReachedTheEnd`, `onReachedMarker`.
- Inspector events: `onPlayed`, `onPaused`, `onStopped`.

Use instance events when the logic belongs to one cutscene. Use static callbacks only for global systems that need to observe every active cutscene.

## Playback API

- `Play()` starts or resumes playback.
- `PlayAt(markerName)` starts playback at a marker.
- `Pause()` pauses the director.
- `Stop()` stops the director.
- `SetTime(time)` assigns director time directly.

## Marker API

- `GetMarker(markerName)` finds a marker by name across output tracks.
- `GoToMarker(markerName)` jumps director time to a marker.
- `RegisterExitMarker(markerName)` stops playback when that marker is reached.
- `RegisterJumpMarkers(startMarkerName, endMarkerName)` jumps from one marker to another.
- `CompareMarkerTiming(markerName, time)` returns the relative ordering of a named marker and an arbitrary Timeline time.
- `IsBefore(markerName)` and `IsAfter(markerName)` compare current director time to a marker.

`CutsceneMarker` exposes editor presentation fields only: `color`, `lineStyle`, `showName`, `lineTextureSize`, and `description`. They change the Timeline marker overlay and label, not playback or marker lookup behavior.

## Loop integration

- `EscapeCurrentLoop(toEnd)` asks the active `LoopBehaviour` to exit.
- `IsInLoopClip(out loop)` checks whether current time is inside any loop clip.
- `IsInActiveLoopClip(out loop)` checks whether current time is inside a loop that has not finished.

Read `07-Loop-Track.md` before changing loop behavior.

## Runtime binding API

- `RemoveBindingFrom(trackName)` clears bindings on tracks with the given name.
- `RemoveBindingFrom<T>(trackName)` clears bindings by track type and optional name.
- `AddBindingTo(trackName, bindingObject)` assigns a binding by track name.
- `AddBindingTo<T>(trackName, bindingObject)` assigns a binding by track type and optional name.
- `ReplaceBindings(original, target)` replaces bound components whose GameObject matches `original`.

Agent warning: the generic overloads currently use the track output target type during filtering. Inspect `Runtime/Cutscene.cs` before relying on this behavior for broad automated rebinding.

## Audio listener behavior

If `disableMainAudioListener` is true, the cutscene attempts to disable the main camera audio listener during playback and restore it afterward. Check this setting when debugging missing audio or duplicated listener warnings.

## Agent notes

- `Cutscene` is the safest entry point for user-facing playback control.
- Use direct `PlayableDirector` APIs only when a task explicitly needs raw Timeline behavior.
- Marker names are string-based; validate spelling before assuming navigation failed.
- When changing marker or loop logic, inspect both `Cutscene.cs` and `Runtime/Loop`.
