# Agent Quickstart

Use this file when you need a fast orientation before editing or using Cutscene Engine.

## Project facts

- Package root: `Assets/Cutscene Engine`
- Runtime namespace: `CutsceneEngine`
- Editor namespace: `CutsceneEngineEditor`
- Main runtime component: `Runtime/Cutscene.cs`
- Timeline examples: `Track Examples/Scenes`
- Timeline playable assets: `Track Examples/Sources/Timeline`
- Unity target noted by project instructions: Unity 6000.0.47f1

## Common Timeline pattern

Most track features follow this pattern:

- `Runtime/<Feature>/<Feature>Track.cs` defines the Timeline track and binding type.
- `Runtime/<Feature>/<Feature>Clip.cs` defines the clip asset and serialized user options.
- `Runtime/<Feature>/<Feature>Behaviour.cs` applies the clip state at runtime.
- `Runtime/<Feature>/<Feature>MixerBehaviour.cs` blends multiple clips when the feature supports blending.
- `Editor/<Feature>/...` defines inspectors and Timeline editor drawing.

Feature documents name serialized authoring fields and callable runtime helpers. Unity lifecycle overrides such as `CreatePlayable`, `CreateTrackMixer`, `ProcessFrame`, and `GatherProperties` are integration entry points rather than user-facing calls; inspect their source when changing evaluation or preview behavior. Public fields on a `PlayableBehaviour` usually mirror clip data for graph transport and should be configured through the clip unless a document says otherwise.

## Conditional features

Check preprocessor symbols before assuming a feature exists in a build:

- `CINEMACHINE`, `CINEMACHINE_2_8_OR_NEWER`, `CINEMACHINE_3_OR_NEWER` gate camera impulse support.
- `URP` gates `Light2DTrack` and Universal Render Pipeline decal behavior.
- `HDRP` gates High Definition Render Pipeline light and decal behavior.
- `TMP` gates TextMesh Pro subtitle and color support.
- `LOCALIZATION` gates localized subtitle strings.
- `VFX` gates Visual Effect Graph color support.
- `UNITY_POST_PROCESSING_STACK_V2` gates legacy Post Processing Stack volume support.

## Safe editing rules for agents

- Inspect the runtime file for the feature before changing behavior.
- Inspect the matching editor inspector when changing visible authoring options.
- If editing C# files, refresh or import in Unity if possible, then check Unity compile state and console errors.
- Do not rely only on script validation outside Unity; Unity console errors are the source of truth.
- Preserve `.meta` files when moving files under `Assets`.
- For new assets or documentation files under `Assets`, add a `.meta` file or let Unity create it before committing.

## Quick binding map

| Feature | Track class | Required binding |
| --- | --- | --- |
| Camera overlap | `CameraOverlapTrack` | `Camera` |
| Color | `ColorTrack` | `GameObject` |
| Impulse | `ImpulseTrack` | none; clip uses optional impulse point |
| Light | `LightTrack` | `Light` |
| Light 2D | `Light2DTrack` | `Light2D` under `URP` |
| Loop | `LoopTrack` | none |
| Particle | `ParticleTrack` | none; clip uses exposed `ParticleSystem` |
| Subtitle | `SubtitleTrack` | `SubtitleText` |
| Time scale | `TimeScaleTrack` | none |
| Transform | `TransformTrack` | `Transform` |
| Video play | `VideoPlayTrack` | none |
| Volume control | `VolumeTrack` | none |
| Humanoid IK | `HumanoidIKTrack` | humanoid `Animator` |
| Look At | `LookAtTrack` | humanoid `Animator` |

## Prefer example scenes

When a user asks how a track is intended to be used, inspect the matching example scene and playable asset:

- `1. Camera Overlap (Cinemachine).unity`
- `2. Color.unity`
- `3. Impulse (Cinemachine).unity`
- `4. Light.unity`
- `5. Loop.unity`
- `6. Particle.unity`
- `7. Subtitle.unity`
- `8. TimeScale.unity`
- `9. Transform.unity`
- `10. Video Play.unity`
- `11. Volume (URP HDRP).unity`
