# Impulse Track

## Purpose

`ImpulseTrack` fires Cinemachine impulse events from Timeline clips. It is for camera shake or other impulse listener reactions.

## Source files

- Runtime: `Runtime/Impulse/ImpulseTrack.cs`
- Clip: `Runtime/Impulse/ImpulseClip.cs`
- Behaviour: `Runtime/Impulse/ImpulseBehaviour.cs`
- Editor: `Editor/Impulse`

## Dependency symbols

The feature depends on Cinemachine symbols:

- `CINEMACHINE`
- `CINEMACHINE_2_8_OR_NEWER`
- `CINEMACHINE_3_OR_NEWER`

The code has version-specific branches for Cinemachine 2.x and 3.x APIs.

## Timeline setup

1. Add an `ImpulseTrack`.
2. Add an `ImpulseClip`.
3. Configure the clip impulse definition.
4. Optionally assign `impulsePoint` if the impulse should originate from a transform.
5. Add Cinemachine impulse listeners in the scene as required by Cinemachine.

## Binding

The track has no Timeline binding. The clip owns the impulse definition and optional exposed transform reference.

## Clip options

- `impulseDefinition`: Cinemachine impulse shape, type, channel, duration, distance, and propagation settings.
- `velocity`: impulse velocity passed to `CreateEvent`.
- `impulsePoint`: optional transform used as the event position.

## Runtime behavior

- The impulse is fired in `OnBehaviourPlay`.
- For uniform impulse types, the event position may be `Vector3.zero`.
- For non-uniform impulse types, `impulsePoint.position` is used when available.
- In editor preview, the behavior tries to clear residual listener reaction noise when the graph pauses or stops.

## Agent notes

- Do not document this as a camera track; it is an event track for Cinemachine impulse listeners.
- If a clip duration looks wrong, inspect `ImpulseTrack.OnCreateClip`; duration is derived from the Cinemachine impulse definition.
- If camera shake is absent, check impulse channels and listener setup before changing Cutscene Engine code.
