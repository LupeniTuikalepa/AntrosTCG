# Camera Overlap Track

## Purpose

`CameraOverlapTrack` creates a temporary copy of a bound camera, renders it to a hidden render texture, and draws that texture over the screen through a temporary overlay canvas. It is useful for camera transition overlays and fade-out camera blends.

## Source files

- Runtime: `Runtime/CameraOverlap/CameraOverlapTrack.cs`
- Clip: `Runtime/CameraOverlap/CameraOverlapClip.cs`
- Behaviour: `Runtime/CameraOverlap/CameraOverlapBehaviour.cs`
- Editor: `Editor/CameraOverlap/CameraOverlapTrackEditor.cs`

## Timeline setup

1. Add a `CameraOverlapTrack` to a Timeline.
2. Bind the track to a `Camera`.
3. Add a `CameraOverlapClip`.
4. Tune the clip opacity curve and sorting order.

## Binding

The track requires a `Camera` binding through `[TrackBindingType(typeof(Camera))]`.

## Clip options

- `sortingOrder`: overlay canvas sorting order. Use a lower value than subtitles or skip UI if those must remain above the overlap.
- `followCam`: when true, the temporary overlap camera follows the bound camera transform and lens settings each frame.
- `opacityCurve`: evaluates clip-normalized time and writes overlay alpha.

## Runtime behavior

- Creates a hidden `RenderTexture`.
- Instantiates the bound camera.
- Removes `CinemachineBrain` and `AudioListener` from the temporary camera when present.
- Draws the render texture through a hidden full-screen `RawImage`.
- Releases the temporary camera, render texture, and canvas when the clip weight reaches zero or the graph stops.

## Agent notes

- This track is camera-bound and screen-space; it is not a Cinemachine virtual camera blend.
- If a user reports hidden overlay artifacts, inspect cleanup paths in `CameraOverlapBehaviour.Release`.
- If another canvas disappears behind the overlap, check `sortingOrder`.
- If the source camera moves during the clip, `followCam` must be true.
