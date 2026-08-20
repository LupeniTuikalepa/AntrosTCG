# Subtitle Track

## Purpose

`SubtitleTrack` displays subtitle text through a bound `SubtitleText` component. It supports direct text, optional localization, virtual clips, fade display, typing display, and clip enter/exit callbacks.

## Source files

- Track: `Runtime/Subtitle/SubtitleTrack.cs`
- Clip: `Runtime/Subtitle/SubtitleClip.cs`
- Behaviour: `Runtime/Subtitle/SubtitleBehaviour.cs`
- Display component: `Runtime/Subtitle/SubtitleText.cs`
- Effect data: `Runtime/Subtitle/SubtitleFadeParameter.cs`, `Runtime/Subtitle/TypingEffectParameter.cs`, `Runtime/Subtitle/TypingEffect.cs`
- Enums: `Runtime/Subtitle/SubtitleTextType.cs`, `Runtime/Subtitle/TextDisplayEffect.cs`
- Editor: `Editor/Subtitle`

## Timeline setup

1. Add `SubtitleText` to the UI or text GameObject.
2. Add a `SubtitleTrack`.
3. Bind the track to the `SubtitleText` component.
4. Add `SubtitleClip` instances.
5. Set text or localized string data per clip.

## Binding

The track requires a `SubtitleText` binding.

## Supported text targets

`SubtitleText` auto-detects:

- legacy `UnityEngine.UI.Text`
- legacy `TextMesh`
- `TMP_Text` when `TMP` is defined
- UI Toolkit `Label` inside a `UIDocument`

For UI Toolkit, `elementName` is the Label name or slash-separated lookup path.

## SubtitleText options

- `ignoreTimeScale`: fade and typing effects use real time when true.
- `deactivateIfEmpty`: disables the GameObject after empty text is displayed.
- `prefix`: prepended to non-empty subtitle text.
- `textDisplayEffect`: `None`, `Fade`, or `Typing`.
- `subtitleFadeParameter`: fade curve and fade-in/out times.
- `typingEffectParameter`: per-character or total typing duration, fade-out, and per-character timing overrides.
- `onChanged`: invoked every time the displayed string changes.

## Clip options

- `isVirtual`: does not write UI text but still invokes `SubtitleClip.OnClipEnter` and `OnClipExit`.
- `text`: direct subtitle text.
- `useLocalizedString` and `localizedString`: available when `LOCALIZATION` is defined.
- `overrideTypingEffectParameter`: lets a clip override the bound `SubtitleText` typing settings once.

## Runtime behavior

- On first active frame, the clip invokes enter callback and initializes display.
- `None` and `Fade` set full text immediately.
- `Typing` reveals text over clip time.
- On pause with zero effective weight, the subtitle clears immediately and invokes exit callback.
- Fade and typing alpha are applied by modifying the detected text target.

## Public scripting API

- `SubtitleText.textType` reports the detected target type. The concrete target references are `text_legacy`, `textMesh`, `text_tmp` under `TMP`, and `uiDocument` / `text_uielement` for UI Toolkit.
- `SubtitleText.SetText(text)` applies `prefix`, updates the detected target, invokes `onChanged`, and honors `deactivateIfEmpty`.
- `SubtitleText.Clear(immediately)` clears through the configured display effect; pass `true` when the old text must disappear without a fade or typing tail.
- `SubtitleClip.GetText()` returns the localized value when localization is enabled for the clip, otherwise `text`, with null normalized to an empty string.
- `SubtitleClip.OnClipEnter` and `OnClipExit` are static events carrying the clip, its evaluated behavior, and the bound `SubtitleText`.
- `TypingEffect.GetTypingText(text)` yields progressively revealed grapheme-aware strings. `TypingEffect.CalcDuration(text, parameter)` calculates the complete typing and fade duration.
- `TypingEffectParameter` exposes `timeMethod`, `timePerChar`, `totalDuration`, `fadeOutTime`, `fadeOutCurve`, and `additionalCharSettings`. `HasAdditionalCharSetting(c, out time)` resolves a `CharTimeOverride`; each override stores `c` and `time`.
- `SubtitleFadeParameter` exposes `curve`, `fadeInTime`, and `fadeOutTime`.

`SubtitleBehaviour` public fields are the Timeline evaluation state copied from `SubtitleClip`; configure clips and `SubtitleText` rather than constructing the behavior directly.

## Agent notes

- Virtual subtitle clips are for external systems that listen to events; they intentionally do not display text.
- If no text appears, check `SubtitleText` target detection first.
- UI Toolkit label lookup is name-based and can fail if `elementName` is wrong.
- Localization is conditional; do not document localized strings as always available.
