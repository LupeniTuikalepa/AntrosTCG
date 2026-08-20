# Video Play Track

## Purpose

`VideoPlayTrack` plays Unity `VideoClip` assets from Timeline and outputs them to screen overlay, camera background, or a render texture.

## Source files

- Runtime: `Runtime/VideoPlay/VideoPlayTrack.cs`
- Clip: `Runtime/VideoPlay/VideoPlayClip.cs`
- Behaviour: `Runtime/VideoPlay/VideoPlayBehaviour.cs`
- Enums: `Runtime/VideoPlay/VideoRenderTarget.cs`
- Editor: `Editor/VideoPlay`

## Timeline setup

1. Add a `VideoPlayTrack`.
2. Add a `VideoPlayClip`.
3. Assign `video`.
4. Choose render target and audio output.
5. If using `RenderTexture`, assign `renderTexture`.
6. If using `AudioSource`, assign the exposed audio source.

## Binding

The track has no binding.

## Clip options

- `video`: `VideoClip` to play.
- `loop`: enables clip looping and Timeline looping capability.
- `aspectRatio`: Unity `VideoAspectRatio` setting.
- `renderTarget`: `Screen`, `Background`, or `RenderTexture`.
- `sortingOrder`: overlay canvas order for screen output.
- `renderTexture`: explicit render texture target.
- `audioOutputTarget`: `Mute`, `Direct`, or `AudioSource`.
- `audioSource`: exposed audio source used when output target is `AudioSource`.
- `audioVolume`: final volume multiplier.

## Runtime behavior

- The clip duration follows `video.length` when a video is assigned.
- The behavior creates a hidden `VideoPlayer` GameObject.
- Screen output creates a hidden overlay canvas and `RawImage`.
- Background output renders to a camera far plane.
- Render texture output writes to the provided render texture, or a temporary one when needed.
- Timeline time drives `VideoPlayer.externalReferenceTime`.
- Clip weight drives camera alpha, overlay alpha, and audio volume.
- Temporary objects are destroyed when the playable is destroyed.

## Agent notes

- If video appears black, check whether the video is prepared and whether the render target has a valid texture.
- If UI appears behind video, check `sortingOrder`.
- If audio is silent, check `audioOutputTarget` before inspecting AudioSource setup.
- This track creates hidden runtime objects; cleanup bugs should be investigated in `VideoPlayBehaviour.OnPlayableDestroy`.
