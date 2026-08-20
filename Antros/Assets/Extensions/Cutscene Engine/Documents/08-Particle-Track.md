# Particle Track

## Purpose

`ParticleTrack` plays and optionally stops a `ParticleSystem` from Timeline clips.

## Source files

- Runtime: `Runtime/Particle/ParticleTrack.cs`
- Clip: `Runtime/Particle/ParticleClip.cs`
- Behaviour: `Runtime/Particle/ParticleBehaviour.cs`
- Mixer: `Runtime/Particle/ParticleMixerBehaviour.cs`
- Editor: `Editor/Particle`

## Timeline setup

1. Add a `ParticleTrack`.
2. Add a `ParticleClip`.
3. Assign the clip `particleSystem` exposed reference.
4. Decide whether the clip should stop the particle system when it ends.

## Binding

The track has no required binding. The clip references a `ParticleSystem` directly.

## Clip options

- `particleSystem`: exposed reference to the target particle system.
- `stopOnEnd`: stops emitting when the clip ends.
- `controlChildren`: passes the same child-control flag to `ParticleSystem.Play` and `ParticleSystem.Stop`.
- `connectName`: editor-facing helper flag used by authoring tools.

## Runtime behavior

- In play mode, the particle system plays when the clip starts.
- If the particle system is inactive because an activation track starts on the same frame, the behavior retries on the next frame.
- If `stopOnEnd` is true, the particle system stops when clip weight reaches zero after playback.
- In editor preview, auto-random seeds are initialized for deterministic preview.

## Agent notes

- Do not look for a track binding when debugging target assignment; inspect the clip exposed reference.
- If particles fail on the first frame, check GameObject activation timing.
- `stopOnEnd` stops emitting, not necessarily clears all already emitted particles.
