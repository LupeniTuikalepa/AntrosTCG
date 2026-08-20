# Transform Track

## Purpose

`TransformTrack` animates a bound `Transform` position, rotation, scale, and optionally GameObject activation.

## Source files

- Runtime: `Runtime/Transform/TransformTrack.cs`
- Clip: `Runtime/Transform/TransformClip.cs`
- Behaviour: `Runtime/Transform/TransformBehaviour.cs`
- Mixer: `Runtime/Transform/TransformMixerBehaviour.cs`
- Enum: `Runtime/Transform/TransformMethod.cs`
- Editor: `Editor/Transform`

## Timeline setup

1. Add a `TransformTrack`.
2. Bind it to the target `Transform`.
3. Add one or more `TransformClip` instances.
4. For each transform channel, choose `Value` or `Transform`.
5. Configure offsets if needed.

## Binding

The track requires a `Transform` binding.

## Track options

- `controlActivation`: when true, the bound GameObject is active while clip weight is greater than zero and inactive otherwise.
- Read-only `initialPos`, `initialRot`, and `initialScale` cache the bound transform values used by Scene view preview and mixing. They are populated when the track mixer is created.

## Clip options

Each channel has its own method:

- `positionMethod`
- `rotationMethod`
- `scaleMethod`

`TransformMethod.Value` uses the clip value directly:

- `position`
- `rotation`
- `scale`

`TransformMethod.Transform` uses `sourceTransform` as the target reference.

Offsets:

- `applyPositionOffset`, `positionTransformLocalOffset`, `positionTransformWorldOffset`
- `applyRotationOffset`, `rotationTransformLocalOffset`, `rotationTransformWorldOffset`
- `applyScaleOffset`, `scaleTransformLocalOffset`, `scaleTransformWorldOffset`

## Runtime behavior

- Initial local position, rotation, and scale are cached on first processing.
- Multiple clips are blended by Timeline input weight.
- Value mode can optionally offset from the initial local transform.
- Transform mode converts source world transform data into the bound transform parent space.
- Rotation blending is Euler-angle based in the mixer.

## Agent notes

- Use `Value` for fixed authored poses.
- Use `Transform` when the target should follow or match another transform reference.
- Be careful with rotation wrap-around because the mixer blends Euler angles.
- If `controlActivation` is enabled, this track can disable the whole bound GameObject outside clip weight.
