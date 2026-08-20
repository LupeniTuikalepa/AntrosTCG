# TimeScale Track

## Purpose

`TimeScaleTrack` controls global `Time.timeScale` from Timeline.

## Source files

- Runtime: `Runtime/TimeScale/TimeScaleTrack.cs`
- Clip: `Runtime/TimeScale/TimeScaleClip.cs`
- Behaviour: `Runtime/TimeScale/TimeScaleBehaviour.cs`
- Mixer: `Runtime/TimeScale/TimeScaleMixerBehaviour.cs`
- Editor: `Editor/TimeScale`

## Timeline setup

1. Add a `TimeScaleTrack`.
2. Add one or more `TimeScaleClip` instances.
3. Set target time scale and optional multiplier curve on each clip.

## Binding

The track has no binding.

## Clip options

- `timeScale`: target global time scale. Minimum is `0.001`.
- `multiplier`: curve evaluated over normalized clip time and multiplied into `timeScale`.

## Runtime behavior

- Active clips blend by Timeline weight.
- The mixer interpolates from `1` to the weighted target time scale.
- The final value is clamped to at least `TimeScaleClip.MinTimeScale`.
- `Time.timeScale` is reset to `1` when the playable is destroyed.

## Agent notes

- This controls global Unity time; it can affect gameplay, physics, coroutines, and systems outside the cutscene.
- The track cannot set time scale to exactly `0` because Timeline would stop progressing.
- If a subtitle should ignore this track, set `SubtitleText.ignoreTimeScale`.
