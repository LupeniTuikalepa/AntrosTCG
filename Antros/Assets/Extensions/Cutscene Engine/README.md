# Cutscene Engine

Cutscene Engine is a Unity Timeline package for authoring and playing cinematic sequences. It provides custom Timeline tracks, runtime cutscene controls, Scene view authoring tools, physics recording, actor rebinding, and Timeline JSON import/export.

This repository root maps to `Assets/Cutscene Engine` inside the Unity project. Runtime code uses the `CutsceneEngine` namespace, and editor code uses `CutsceneEngineEditor`.

## Start here

- [Agent Quickstart](Documents/00-Agent-Quickstart.md) - package layout, conditional features, common track structure, and safe editing rules.
- [Cutscene Core](Documents/01-Cutscene-Core.md) - playback, markers, events, bindings, and cutscene state.
- [External Documentation](Documents/Document.md) - illustrated online documentation and the video demo.

## Feature documentation

- [Camera Overlap Track](Documents/02-Camera-Overlap-Track.md) - camera overlap and screen-fade transitions.
- [Color Track](Documents/03-Color-Track.md) - color animation for renderers, UI, decals, and visual effects.
- [Impulse Track](Documents/04-Impulse-Track.md) - Cinemachine impulse clips.
- [Light Track](Documents/05-Light-Track.md) - 3D light animation.
- [Light2D Track](Documents/06-Light2D-Track.md) - Universal Render Pipeline 2D light animation.
- [Loop Track](Documents/07-Loop-Track.md) - Timeline loop sections.
- [Particle Track](Documents/08-Particle-Track.md) - particle-system playback control.
- [Subtitle Track](Documents/09-Subtitle-Track.md) - subtitle text, fades, typing effects, and localization hooks.
- [TimeScale Track](Documents/10-TimeScale-Track.md) - global `Time.timeScale` control.
- [Transform Track](Documents/11-Transform-Track.md) - transform position, rotation, and scale control.
- [Video Play Track](Documents/12-Video-Play-Track.md) - video playback to the screen, background, or a render texture.
- [Volume Control Track](Documents/13-Volume-Control-Track.md) - render-pipeline volume and post-processing profile playback.
- [Physics Tools](Documents/14-Physics-Tools.md) - force helpers, simulation, recording, and optimized animation curves.
- [Actor Binding and Timeline JSON](Documents/15-Actor-Binding-And-Timeline-JSON.md) - actor preview, runtime binding changes, and Timeline JSON export/import.
- [Humanoid IK Track](Documents/16-Humanoid-IK-Track.md) - late-stage humanoid hand and foot IK with Scene view pose authoring.
- [Look At Track](Documents/17-Look-At-Track.md) - humanoid gaze targeting, per-body-part weights and rotation limits, Scene view preview, blinking, and gaze-driven eyelid BlendShapes.

## Package layout

- `Runtime/` - runtime components, Timeline tracks, clips, behaviours, and utilities.
- `Editor/` - inspectors, Timeline drawing, Scene view authoring, icons, and editor-only helpers.
- `Tests/Editor/` - EditMode regression tests for editor and runtime contracts.
- `Track Examples/` - example scenes, playable assets, and supporting sources.
- `Documents/` - focused current-state references for each feature and workflow.

When changing a feature, update its document under `Documents/` and preserve the matching Unity `.meta` file.
