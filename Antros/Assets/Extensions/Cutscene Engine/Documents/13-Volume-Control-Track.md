# Volume Control Track

## Purpose

`VolumeTrack` plays render pipeline volume or post-processing profiles during a cutscene by instantiating temporary volume objects and blending their weight with Timeline clip weight.

## Source files

- Runtime: `Runtime/VolumeControl/VolumeTrack.cs`
- Clip: `Runtime/VolumeControl/VolumeClip.cs`
- Behaviour: `Runtime/VolumeControl/VolumeBehaviour.cs`
- Editor: `Editor/VolumeControl`

## Dependency symbols

This feature needs one of:

- `URP`
- `HDRP`
- `UNITY_POST_PROCESSING_STACK_V2`

Without these symbols, `VolumeClip` creates an empty playable and has no useful volume behavior.

## Timeline setup

1. Add a `VolumeTrack`.
2. Set track `basePriority` and `volumeLayer`.
3. Add a `VolumeClip`.
4. Assign a volume profile through the clip inspector or the editor action.
5. Blend clips as needed.

## Binding

The track has no binding.

## Track options

- `basePriority`: priority applied to instanced volume objects so they override gameplay volumes.
- `volumeLayer`: layer assigned to the temporary volume GameObject.

## Clip options

- `volumeProfile`: `VolumeProfile` under `URP` or `HDRP`, or `PostProcessProfile` under legacy post-processing.
- `basePriority`: copied from the track.
- `volumeLayer`: copied from the track.

## Runtime behavior

- The clip creates a temporary hidden volume GameObject.
- Priority is `basePriority + clip.startTime`, so later clips can override earlier clips when weights overlap.
- Clip weight is assigned to the instanced volume weight.
- The temporary object is destroyed when the playable is destroyed.
- `VolumeClip.clipCaps` enables blending, extrapolation, and looping when a supported volume dependency is compiled; otherwise it reports no capabilities.
- `VolumeBehaviour.instancedVolume` exposes the generated Volume or PostProcessVolume for diagnostics. Its other public fields are transport copies of `volumeProfile`, computed `volumePriority`, and `volumeLayer`.

## Editor helper

`Editor/VolumeControl/VolumeTrackAddFromProfileAction.cs` defines an `Add From Volume Profile` Timeline menu action for creating clips from profile data.

## Agent notes

- Do not look for a track binding; profile selection is clip data.
- If post-processing does not appear, check render pipeline symbols, camera volume layer masks, and `volumeLayer`.
- If two volume clips conflict, inspect their start times and computed priorities.
