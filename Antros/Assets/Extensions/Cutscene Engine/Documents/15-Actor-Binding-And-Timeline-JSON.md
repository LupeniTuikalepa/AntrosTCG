# Actor Binding and Timeline JSON

## Purpose

Cutscene Engine includes helper components and utilities for rebinding Timeline tracks, previewing actors, and serializing Timeline data to JSON.

## Source files

- Actor runtime: `Runtime/Utility/CutsceneActor.cs`
- Actor preview: `Runtime/Utility/CutsceneActorPreview.cs`
- Actor part mapping: `Runtime/Utility/CutsceneActorPartBinding.cs`
- Trigger helper: `Runtime/Utility/CutsceneTrigger.cs`
- Skip button helper: `Runtime/Utility/CutsceneSkipButton.cs`
- Utility API: `Runtime/Utility/CutsceneEngineUtility.cs`
- JSON data types: `Runtime/Utility/TimelineJsonDataTypes.cs`
- JSON rebind result types: `Runtime/Utility/TimelineJsonRebindTypes.cs`
- Editor inspectors: `Editor/CutsceneActorPreviewInspector.cs`, `Editor/CutsceneInspector.cs`

## CutsceneActor

`CutsceneActor` identifies an actor by key and provides callbacks for binding reset workflows.

Important members:

- `Key`
- `Find(lookupKey)`
- `onTransformInitialized`
- `onResetBinding`
- `partBindings`

Use it when a cutscene needs to locate or rebind a logical actor rather than a hard-coded scene object.

## CutsceneActorPreview

`CutsceneActorPreview` swaps cutscene bindings between a preview actor and an origin actor for authoring.

Important fields:

- `key`
- `cutscene`
- `avatarAnimator`
- `deactivateOnAwake`
- `partBindings`

Root tracks are rebound from the preview GameObject to the runtime actor as before. Tracks bound to a child object use `partBindings`: assign the child GameObject and a stable, case-sensitive part ID on the preview, then assign the corresponding runtime child and the same ID on `CutsceneActor`. The two actors may use different names and hierarchy layouts.

Part IDs and targets must be non-empty and unique within each actor. A mapped runtime target must also contain the component required by the track binding type. Unmapped preview-child tracks and missing or incompatible runtime parts are left unchanged and reported with a warning.

When the actor is rebound, the exact original binding of every changed track is saved. Reset restores those snapshots instead of searching backward through the runtime hierarchy. `ColorTrack` also resolves changes from its live Timeline `playerData`, so a mapped `GameObject` binding takes effect even after the playable graph has already been created.

## CutsceneTrigger

`CutsceneTrigger` plays a cutscene from physics trigger events.

Important fields:

- `cutscene`
- `triggerTiming`
- `requiredTag`
- `playFromMarker`
- `markerName`
- `playOnlyOnce`

Use this for scene trigger-driven playback.

## CutsceneSkipButton

`CutsceneSkipButton` provides hold-to-skip UI for a cutscene.

Important fields:

- `cutscene`
- `button`
- `holdProgressImage`
- `canvasGroup`
- `holdTime`
- `skipKey`
- `skipAction` when the Input System is available

Use this when a cutscene needs a visible or input-driven skip affordance.

## Timeline JSON API

`CutsceneEngineUtility` includes Timeline serialization and deserialization helpers:

- `ToJson(timelineAsset, director, prettyPrint)`
- `FromJson(json, director, assignToDirector)`
- `TryParseTimelineJsonData(json, out data)`
- `TryFromJson(json, out timelineAsset, director, assignToDirector)`
- `TryFromJson(json, out timelineAsset, out loadContext, director, assignToDirector)`
- `SaveToFile(timelineAsset, filePath, director, prettyPrint)`
- `TryLoadFromFile(filePath, out timelineAsset, director, assignToDirector)`
- `RebindTrackBindings(director, loadContext, referenceMap)`

`FromJson` throws for invalid input; prefer the `TryFromJson` overloads for tools that must report failure without an exception. Request the overload with `TimelineJsonLoadContext` when bindings will be supplied after loading.

## Timeline query and time helpers

`CutsceneEngineUtility` also exposes extension methods used throughout runtime and editor tooling:

- `GetTracks(director, binding)` returns every output track bound to an object.
- `GetTracks<T>(director)` / `GetTrack<T>(director)` query by track type.
- `GetTrack<T>(director, predicate)` returns the first typed track matching a predicate.
- `GetTrack<T>(director, binding)` / `GetTracks<T>(director, binding)` combine type and binding filters.
- `GetTrackOf<T>(director, clip)` and `GetTrackOf<T>(timelineAsset, clip)` find the typed owner of a playable asset.
- `GetNormalizedTime(clip, time)` and `GetNormalizedTime(time, start, end)` convert Timeline time to `0..1` clip time.
- `GetTimelineTime(clip, normalizedTime)` and `GetTimelineTime(normalizedTime, start, end)` perform the inverse conversion.

## Binding serialization model

JSON binding data is director-relative:

- GameObjects and components under the `PlayableDirector` transform can be stored as relative paths.
- Legacy global object modes are not resolved in the current director-relative loader.
- Exposed references are stored separately from track generic bindings.
- Manual rebind maps use key priority: binding path, binding name, then the last path segment.

## Rebind result statuses

`TimelineTrackBindingRebindResult` reports:

- `rebound`
- `missing_key`
- `missing_track`
- `type_mismatch`
- `no_binding_data`

Use these statuses when building tools that import timelines into a different scene hierarchy.

## Public JSON data model

- `TimelineJsonData`: `version`, `name`, `durationMode`, `fixedDuration`, and `tracks`.
- `TrackJsonData`: `id`, `parentId`, `type`, `name`, `muted`, `locked`, `serializedJson`, `binding`, `objectReferences`, `clips`, and `markers`.
- `ClipJsonData`: `type`, `displayName`, `start`, `duration`, `clipIn`, `timeScale`, `easeInDuration`, `easeOutDuration`, `serializedJson`, and `objectReferences`.
- `MarkerJsonData`: `type`, `time`, `serializedJson`, and `objectReferences`.
- `ObjectReferenceFieldData`: `fieldName`, `isExposedReferenceDefaultValue`, and `reference`.
- `ObjectReferenceData`: `mode`, `path`, `name`, and `componentType`.
- `TimelineJsonLoadContext`: parsed `data`, created `trackById`, and the created `timelineAsset`.
- `TimelineTrackBindingRebindResult`: aggregate candidate/rebound/skip counters and per-track `entries`.
- `TimelineTrackBindingRebindEntry`: `trackId`, `trackName`, `trackType`, `keyUsed`, requested path/name, `resolvedObject`, and `status`.

These classes are mutable DTOs used by `JsonUtility`; preserve field names and collection shapes when changing the file format.

## Agent notes

- For runtime actor swaps, prefer `Cutscene.ReplaceBindings` or JSON rebind APIs over manually editing every Timeline output.
- When saving JSON, references outside the director hierarchy can be skipped with warnings.
- When loading JSON without a director, the Timeline asset can be created but track bindings cannot be restored.
- Always validate binding type conversion when using a `referenceMap`; track binding attributes determine the accepted object type.
