# Color Track

## Purpose

`ColorTrack` animates color on a bound GameObject. It auto-detects supported components and writes to renderer materials, sprite renderers, UI graphics, UI Toolkit elements, decals, and Visual Effect Graph properties when the matching package symbol is available.

## Source files

- Runtime: `Runtime/Color/ColorTrack.cs`
- Clip: `Runtime/Color/ColorClip.cs`
- Mixer: `Runtime/Color/ColorMixerBehaviour.cs`
- Enums: `Runtime/Color/ColorType.cs`, `Runtime/Color/ColorTargetType.cs`
- Editor: `Editor/Color`

## Timeline setup

1. Add a `ColorTrack`.
2. Bind it to the GameObject that owns the target component.
3. Add one or more `ColorClip` instances.
4. Configure the track target options, then configure each clip color or gradient.

## Binding

The track requires a `GameObject` binding.

Supported target components are checked in this order:

1. `Graphic`
2. `VisualEffect` when `VFX` is defined
3. `UIDocument`
4. `DecalProjector` under `URP` or `HDRP`, otherwise legacy `Projector`
5. `SpriteRenderer`
6. `Renderer`

## Track options

- `isTint`: multiply the initial color by the clip color instead of replacing it.
- `materialIndex`: material array index for renderers. Use `-1` to apply to all materials.
- `propertyName`: shader color property name. Empty or missing property falls back to `material.color`.
- `elementName`: UI Toolkit element name or slash-separated path.
- `uiElementColorTarget`: `TextColor`, `BackgroundColor`, `ImageTint`, `TextOutlineColor`, or `BorderColor`.
- `applyToMaterialProperty`: for UI graphics or sprite renderers with custom materials, write material color instead of component color.
- `applyAlphaToDecalOpacity`: under `URP` or `HDRP`, also writes decal opacity from color alpha.

## Clip options

- `colorType`: `Default` or `Gradient`.
- `color`: fixed color when `colorType` is `Default`.
- `gradient`: evaluated over clip time when `colorType` is `Gradient`.

## Runtime behavior

- Clips support blending and extrapolation.
- The mixer blends active clips by weight.
- Renderer material writes use instantiated materials in play mode and shared materials in edit preview.
- Initial colors are cached and used to blend back when clip weight decreases.

## Public scripting API

- `ColorClip.clipCaps` declares blending and extrapolation support.
- `ColorClip.Evaluate(t)` returns `color` in `Default` mode or samples `gradient` at normalized time `t`.
- `ColorBehaviour.Evaluate(t)` performs the same operation on the Playable transport state. Its public fields mirror the clip.

## Agent notes

- If color does not change, confirm the bound GameObject owns one of the supported components.
- For renderer shader properties, check the exact color property name before changing defaults.
- For UI Toolkit, `elementName` path lookup is name-based and can fail silently if the element name is wrong.
- `ColorTargetType.UIGraphc` is misspelled in code; preserve that enum name unless intentionally making a breaking cleanup.
