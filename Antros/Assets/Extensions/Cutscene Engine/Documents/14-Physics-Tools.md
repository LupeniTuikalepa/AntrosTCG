# Physics Tools

## Purpose

The physics tools record or apply physics-driven motion for cutscene authoring. They are not a Timeline track type; they support recording Rigidbody motion into animation curves and applying configured forces.

## Source files

- Simulator: `Runtime/Physics/PhysicsSimulator.cs`
- Recording data: `Runtime/Physics/PhysicsRecordingTypes.cs`
- Recorder: `Runtime/Physics/PhysicsRecorder.cs`
- Runtime playback: `Runtime/Physics/RuntimePhysicsRecordingPlayback.cs`
- Forces: `Runtime/Physics/ForceSettings.cs`, `Runtime/Physics/ForceFieldSettings.cs`
- Curve optimization: `Runtime/Physics/AnimationCurveOptimizer.cs`
- Editor bridge: `Editor/Physics/PhysicsRecordingEditorBridge.cs`
- Inspectors: `Editor/Physics`

## Main components

### PhysicsSimulator

Controls simulation and optional recording:

- `preDelay`
- `simulationStep`
- `simulationDuration`
- `curveOptimizationValue`
- Read-only state: `IsSimulating`, `IsPaused`, `CurrentSimulationTime`, `HasRecordingTarget`, `RecordingTarget`, `LastRecordingResult`, and `RecordingResultVersion`
- `TryConfigureRecordingTarget`
- `ClearRecordingTarget`
- `StartSimulation`
- `PauseSimulation`
- `ResumeSimulation`
- `TickSimulation`
- `StopSimulation`

### ForceSettings

Applies per-object 3D or 2D forces:

- `forces`: array of `ForceData`
- `force`, `torque`
- `startDelay`
- `duration`
- `forceMode`, `forceMode2D`
- `forceCurve`, `torqueCurve`
- `showGizmos`, `forceGizmoColor`, `torqueGizmoColor`
- `useLocalForce`, `useLocalTorque`
- `applied`: non-serialized one-shot runtime state

`ForceData.Init()` assigns the default serialized values once, and `IsValid()` checks whether force or torque magnitude is meaningful. The public Rigidbody accessors on `ForceSettings` are `rb3D` and `rb2D`. Call `Initialize()` before manual use, `ApplyForces()` to use component time, or `ApplyForces(elapsedTime)` when another system owns the clock. `Has3DRigidbody()` and `Has2DRigidbody()` report the configured body kind.

### ForceFieldSettings

Applies area forces:

- `shape`: sphere, cylinder, or box; dimensions are `radius`, `length`, and `boxSize`
- `forceMagnitude`, `forceFalloff`, and `forceOverTime`
- `forceMode3D`, `forceMode2D`, and `dimension`
- `duration` and `startDelay`
- `useExplosionForce`, `explosionRadius`, and `upwardsModifier`
- `targetRoot`: optional Rigidbody search root

Call `Initialize()` before manual use. `ApplyForce()` uses component time; `ApplyForce(elapsedTime)` applies the configured field for an externally supplied elapsed time.

### RuntimePhysicsRecordingPlayback

Plays recorded transform curves at runtime:

- `Configure`
- `Play`
- `Stop`
- `Clear`
- `holdLastPose`
- `useUnscaledTime`
- `IsPlaying`

## Recording model

- `PhysicsRecorder` samples Rigidbody and Rigidbody2D transform motion.
- It records position and rotation curves relative to a binding root.
- `PhysicsRecordingResult` stores recorded curve data.
- `AnimationCurveOptimizer` reduces keyframes according to `curveOptimizationValue`.
- The editor bridge can apply recorded curves to Timeline animation tracks.

## Recording API and data types

- Configure in this order: `TryConfigureRecordingTarget(...)`, `StartSimulation(timed)`, repeated `TickSimulation(deltaTime)` calls when driving manually, then `StopSimulation(applyPausedState)`. The stop call restores Unity physics simulation modes and returns `PhysicsRecordingResult`.
- `PhysicsRecordingTarget` exposes `Director`, `Track`, `BindingRoot`, and `StartDirectorTime`.
- `PhysicsRecordingResult` exposes `Empty`, `StartTime`, `EndTime`, `Curves`, and `HasData`.
- Each `PhysicsRecordedCurve` exposes its transform `Path`, serialized `PropertyName`, and `Curve`.
- `PhysicsRigidbodyState` stores either `Rigidbody3D` or `Rigidbody2D`, plus `Transform`, `RelativePath`, mutable `InitialPosition` / `InitialRotation`, and the recorded `Curves` dictionary.
- `PhysicsRecorder.RecordKeyframes(time)` samples the configured body states, `CompleteRecording(optimizationValue)` returns optimized curves, and `GetRelativePath(root, target)` produces the binding-root-relative path used by playback.

`AnimationCurveOptimizer` is internal implementation detail even though its `Optimize` method is public inside that internal type.

## Agent notes

- Treat these as authoring and support tools, not as one of the 11 Timeline track features.
- When debugging missing recording output, check that a `PlayableDirector`, `AnimationTrack`, and binding root were found.
- Curve optimization can remove keyframes; lower optimization if precision matters.
- Force application supports both 3D and 2D bodies, but target objects must have the correct Rigidbody type.
