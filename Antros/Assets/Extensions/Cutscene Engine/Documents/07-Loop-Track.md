# Loop Track

## Purpose

`LoopTrack` creates Timeline sections that repeat until an escape condition is satisfied.

## Source files

- Runtime: `Runtime/Loop/LoopTrack.cs`
- Clip: `Runtime/Loop/LoopClip.cs`
- Behaviour: `Runtime/Loop/LoopBehaviour.cs`
- Editor: `Editor/Loop`
- Cutscene integration: `Runtime/Cutscene.cs`

## Timeline setup

1. Add a `LoopTrack`.
2. Add a `LoopClip` over the section that should repeat.
3. Set the clip escape method.
4. Use `Cutscene.EscapeCurrentLoop` for manual escape cases.

## Binding

The track has no binding.

## Escape methods

- `Manual`: repeats until code calls escape.
- `LoopCount`: repeats until `loopCount >= targetLoopCount`.
- `Elapsed`: repeats until `elapsed >= minElapseTime`.

## Clip options

- `resetAfterEscape`: when true, the loop can be used again after it exits.
- `escapeMethod`: selected loop escape strategy.
- `targetLoopCount`: used by `LoopCount`.
- `minElapseTime`: used by `Elapsed`.
- `description`: editor-facing description.

## Runtime behavior

- The behavior stores clip `start` and `end` times.
- When playback reaches the end of the loop clip, it either exits or sets director time back to `start`.
- If the loop clip ends at the Timeline duration, loop handling runs in `PrepareFrame` to avoid end-of-timeline pause timing problems.
- `Cutscene.OnLoopedByClip` is notified when the loop returns.

## Public loop helpers

Use these from `Cutscene`:

- `EscapeCurrentLoop(bool toEnd)`
- `IsInLoopClip(out LoopBehaviour loop)`
- `IsInActiveLoopClip(out LoopBehaviour loop)`

Use these from `LoopBehaviour` only when you already have the behavior:

- `ShouldEscape()`
- `Escape(bool toEnd)`

The behavior exposes `director`, clip `start` / `end`, `endAtTimelineEnd`, computed `duration`, and current `loopCount`, `elapsed`, `escapeRequested`, and `isFinished` state. `LoopClip.behaviour` is populated after its playable is created. Treat `start`, `end`, and `endAtTimelineEnd` as Timeline-maintained values rather than authored options.

## Agent notes

- `loopCount` on `Cutscene` is not the same as `LoopBehaviour.loopCount`.
- Manual escape requires a `Cutscene` or direct `LoopBehaviour` reference.
- If the loop is at the Timeline end, debug `PrepareFrame`, not only `OnBehaviourPause`.
