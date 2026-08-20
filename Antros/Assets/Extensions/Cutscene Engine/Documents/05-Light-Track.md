# Light Track

## Purpose

`LightTrack` animates Unity `Light` properties from Timeline. It supports blending, extrapolation, optional intensity noise, optional intensity curves, and render-pipeline-specific light properties.

## Source files

- Runtime: `Runtime/Light/LightTrack.cs`
- Clip: `Runtime/Light/LightClip.cs`
- Mixer: `Runtime/Light/LightMixerBehaviour.cs`
- Shared option enum: `Runtime/Light/ValueControlOption.cs`
- Editor: `Editor/Light`

## Timeline setup

1. Add a `LightTrack`.
2. Bind it to a Unity `Light`.
3. Add one or more `LightClip` instances.
4. On the track, decide how each property is controlled.
5. On each clip, set target light values.

## Binding

The track requires a `Light` binding.

## Control modes

`ValueControlOption` is used by the track for each supported property:

- `None`: leave the current binding value unchanged.
- `Replace`: interpolate from the initial binding value to the clip value by Timeline weight.
- `Add`: add the clip value to the initial value.
- `Multiply`: multiply the initial value by the clip value.

## Main track controls

Common Unity `Light` controls include:

- `innerSpotAngleControl`
- `spotAngleControl`
- `colorControl`
- `colorTemperatureControl`
- `intensityControl`
- `bounceIntensityControl`
- `rangeControl`
- `shadowStrengthControl`

HDRP-specific controls include area size, angular diameter, shape radius, surface color, flare properties, intensity multiplier, volumetric multiplier, volumetric shadow dimmer, and shadow tint.

## Clip options

Common clip values include:

- `innerSpotAngle`, `outerSpotAngle`
- `areaSize`
- `color`, `colorTemperature`
- `intensity`, `bounceIntensity`, `range`
- `shadowStrength`
- `useIntensityNoise`, `intensityNoiseOffset`, `intensityNoiseSpeed`, `intensityNoisePower`, `intensityNoiseStrength`
- `useIntensityCurve`, `intensityCurve`

HDRP adds fields such as `angularDiameter`, `radius`, `surfaceColor`, flare fields, `intensityMultiplier`, `volumetricMultiplier`, `volumetricShadowDimmer`, and `shadowTint`.

## Runtime behavior

- Active clips are blended by Timeline input weight.
- Initial light values are cached when the mixer first processes the bound light.
- Intensity noise adds Perlin-noise variation to blended intensity.
- Intensity curves multiply the final intensity over clip time.

## Agent notes

- When a property is not changing, check the track-level control option first.
- `Multiply` is the default for intensity, so a clip intensity of `1` preserves the initial intensity.
- HDRP fields require `HDRP`; do not assume they exist in non-HDRP builds.
- Light preview issues usually involve initial value caching or Timeline preview property collection.
