# Light2D Track

## Purpose

`Light2DTrack` animates Universal Render Pipeline `Light2D` components from Timeline.

## Source files

- Runtime: `Runtime/Light2D/Light2DTrack.cs`
- Clip: `Runtime/Light2D/Light2DClip.cs`
- Mixer: `Runtime/Light2D/Light2DMixerBehaviour.cs`
- Shared option enum: `Runtime/Light/ValueControlOption.cs`
- Editor: `Editor/Light2D`

## Dependency symbol

This feature is compiled only when `URP` is defined.

## Timeline setup

1. Add a `Light2DTrack`.
2. Bind it to a `UnityEngine.Rendering.Universal.Light2D`.
3. Add one or more `Light2DClip` instances.
4. Configure track-level control options.
5. Configure clip values.

## Binding

The track requires a `Light2D` binding.

## Track controls

Each controlled property uses `ValueControlOption`:

- `colorControl`
- `intensityControl`
- `pointLightInnerRadiusControl`
- `pointLightOuterRadiusControl`
- `pointLightInnerAngleControl`
- `pointLightOuterAngleControl`
- `falloffControl`
- `falloffStrengthControl`
- `shadowStrengthControl`
- `shadowSoftnessControl`
- `shadowFalloffStrengthControl`
- `volumetricIntensityControl`
- `volumetricShadowIntensityControl`

## Clip options

- `color`, `intensity`
- point light radius and angle fields
- falloff and falloff strength
- shadow strength, softness, and falloff strength
- volumetric intensity and volumetric shadow intensity
- optional intensity noise fields
- optional intensity curve fields

## Runtime behavior

- Active clips are blended by Timeline input weight.
- Initial `Light2D` values are cached on first processing.
- Shadow softness and shadow falloff are applied only when the Unity version exposes those fields.
- Intensity curve and intensity noise modify the final intensity.

## Agent notes

- Do not suggest this feature outside URP.
- If only color and intensity work, check Unity version and Light2D property availability.
- The control option defaults are similar to the 3D light track: color replaces and intensity multiplies.
