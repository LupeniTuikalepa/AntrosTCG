# Humanoid IK Track

## Purpose

`HumanoidIKTrack` applies late-stage IK to one humanoid limb from Timeline.

The track binds to an `Animator`, chooses one target limb, and each clip provides the target pose, bend target, weights, optional anchor transform, and finger or toe pose.

## Source files

- Runtime: `Runtime/HumanoidIK/HumanoidIKTrack.cs`
- Clip: `Runtime/HumanoidIK/HumanoidIKClip.cs`
- Behaviour: `Runtime/HumanoidIK/HumanoidIKBehaviour.cs`
- Mixer: `Runtime/HumanoidIK/HumanoidIKMixerBehaviour.cs`
- Late update driver: `Runtime/HumanoidIK/HumanoidIKLateUpdateDriver.cs`
- Avatar reference pose: `Runtime/HumanoidIK/HumanoidIKReferencePose.cs`
- Cached digit chains: `Runtime/HumanoidIK/HumanoidIKDigitChainCache.cs`
- Shared types: `Runtime/HumanoidIK/HumanoidIKTypes.cs`
- Editor: `Editor/HumanoidIK`
  - `HumanoidIKSceneOverlay.cs` adds a dockable Scene view control panel for hand and foot digit bends.
  - Digit pose math, diagram textures, primitive rendering, and duplicate-track validation are split into dedicated helpers.
- EditMode tests: `Tests/Editor/HumanoidIKTests.cs`

## Timeline setup

1. Add a `HumanoidIKTrack`.
2. Bind the track to a humanoid `Animator`.
3. Set the track `target` to `LeftHand`, `RightHand`, `LeftFoot`, or `RightFoot`.
   - `Auto Rename Clips` defaults to enabled. When `target` changes, exact `Left`/`Right` and `Hand`/`Foot` name tokens in every clip on the track are normalized to the new target. It supports both compact names such as `LeftHand` and spaced names such as `Left Hand IK` without changing unrelated text such as `Leftover` or `Handy`.
   - Use only one track for each Animator and target. Timeline and the track Inspector show a warning when another track is bound to the same Animator and controls the same hand or foot.
4. Add one or more `HumanoidIKClip` instances. A new clip starts at the bound character's current limb position and anatomical display rotation, stored relative to the `PlayableDirector`. Hands capture the palm effector frame; feet capture the canonical sole frame. Without a usable bound humanoid, it starts at the Director-local origin with identity rotation.
5. Select a clip and use `Capture Current Limb Pose` whenever you want to recapture the target from the bound character.
6. Adjust the position, target rotation, and pole direction in Scene view using Handles.

## Clip options

- `anchorTransform`: optional target transform. When assigned, the IK target position is exactly this transform's world position. The owning `PlayableDirector` transform supplies the relative coordinate frame when this is unassigned.
- `gizmoColor`: per-clip color and opacity for the Scene view limb, bend guide, and hand/foot preview. New clips inherit the previous target-specific default colors, while existing clips retain the same target-derived fallback until a custom color is chosen.
- `position`: Director-local position used only when `anchorTransform` is unassigned. With an explicit anchor, the anchor's world position is authoritative and no positional offset is applied.
- `rotation`: an anchor-local anatomical target Euler rotation. Without an explicit anchor, it is stored in Director-local space for both hand and foot tracks. Hands use the character-independent Humanoid effector frame whose forward axis follows wrist-to-fingers and whose up axis follows the palm-facing side. Feet use the canonical sole frame whose `+Z` points toward the toes and whose `+Y` points toward the dorsum.
- `bendTarget`: a fixed Bend Goal position in the effective anchor space (stored relative to the `PlayableDirector` when unassigned). Runtime IK computes the pole direction from the limb root to this target point.
- `positionWeight`: how strongly the limb follows `position`.
- `rotationWeight`: how strongly the limb end follows `rotation`.
- `bendWeight`: how strongly `bendTarget` controls the elbow or knee plane.
- `digitWeight`: how strongly finger or toe bends are applied.
- `digitBends`: for hands, absolute clip-authored Humanoid muscle angles in degrees. Each joint X value targets Unity's `Stretched` muscle and each proximal Y value targets its `Spread` muscle. Because Humanoid exposes no individual toe muscles, toe values are anatomical offsets resolved from the immutable Avatar reference pose: X bends around the lateral axis derived from the toe direction and sole normal, Y fans around the sole normal, and Z rolls around the toe direction. Imported bone-local axis names are never assumed; for example, a rig whose local Y points toward the toe tip naturally receives Fan around its corresponding local Z axis. Positive Stretch opens the toes toward the dorsum and negative Stretch closes them toward the sole. Toe offsets are never added to the incoming animated toe rotation. Toe bend ranges are intentionally narrow: `-25..20`, `-18..8`, and `-12..5` degrees for the first three existing joints.
- `toeBaseBend`: collective `-1..1` bend for the mapped Humanoid `LeftToes` or `RightToes` root on articulated multi-toe rigs. It is applied before the five child toe branches and uses the proximal toe range of `-25..20` degrees. Simple `Foot-Toe` rigs use the first existing toe-joint control instead and ignore this field.
- `toeFan`: collective `-1..1` fan for articulated multi-toe rigs, with at most 8 degrees of proximal spread. Positive values spread the big and little toes away from the center while the middle toe stays neutral. The rotation axis is resolved from each reference toe direction and the sole normal rather than a fixed imported local axis. A simple `Foot-Toe` rig stores this value but intentionally ignores it at runtime.

## Runtime behavior

- Multiple clips are blended by Timeline input weight.
- The mixer passes weighted clip samples to the late update driver.
- Mixer sample buffers and discovered finger/toe transform chains are reused after their first build, avoiding steady-state managed allocations from these paths. A digit-chain cache is rebuilt only when the bound Avatar or mapped toe hierarchy changes.
- The late update driver resolves anchor-relative clip data after other animation and anchor transform updates.
- The generated `HumanoidIKLateUpdateDriver` applies the final pose in `LateUpdate`.
- The driver derives and caches bone-to-display corrections without using the live Timeline pose as an authored reference. Hands use the Avatar's zero-muscle Humanoid pose: wrist-to-finger direction and little-to-index span define the palm plane. Feet use the immutable `Avatar.humanDescription.skeleton`: the Foot-to-Toes vector is projected onto the Avatar-up plane to define sole forward, while its discarded vertical component records the ankle-to-sole drop. Ankle-to-knee disambiguates dorsum-up. The raw, sloped ankle-to-toe vector must never become the sole `+Z` axis. Without a mapped Toes bone, the character's reference `+Z` direction supplies the canonical forward fallback.
- Bend Goal positions are resolved in world space, and internal pole directions are derived relative to the live limb root position before blending.
- Hand poses are resolved from the clip's absolute muscle angles through the bound Avatar's Humanoid mapping. The character supplies rig retargeting, not the authored reference rotation. At full `digitWeight`, the same clip therefore resolves to the same Humanoid pose even when the character's incoming finger rotations differ.
- `digitWeight` blends from the incoming animated pose to the clip-authored absolute hand or foot target. For feet, the absolute target is `Avatar reference local rotation * authored toe offset`; the incoming Timeline rotation is only the blend source and never becomes the offset base.
- Foot targets inspect the mapped Humanoid `LeftToes` or `RightToes` transform. Two or more direct child branches are treated as articulated toe chains; zero or one child is treated as a simple `Foot-Toe` chain.
- A Humanoid without a mapped toe transform still receives foot position and rotation normally. Its five-toe gizmo remains in the neutral canonical pose, while runtime toe posing is skipped because there is no mapped transform to drive.
- On articulated toe rigs, Toe Base Bend first rotates the mapped Humanoid Toes root from its immutable Avatar reference rotation. The authored per-toe bends and Toe Fan are then combined, clamped to the narrow toe ranges, and resolved as absolute rotations from the Avatar reference skeleton for only the first three transforms that actually exist in each discovered child chain.
- On simple `Foot-Toe` rigs, the first per-toe bend entry's X curl controls the mapped toe transform relative to its immutable Avatar reference rotation. Its Y/Z values, Toe Base Bend, Toe Fan, child end markers, absent joints, and the other four toe entries have no runtime effect.
- The driver resolves hand muscles first, then solves the two-bone limb and applies its end rotation so the HumanPose conversion cannot overwrite the arm or leg IK result.
- The driver restores its previous modifications before applying the next late update when the Animator has not already produced a new pose.

## Anchoring

All clips use the owning `PlayableDirector` transform as their implicit anchor when no explicit anchor is assigned. Position, target rotation, and pole data therefore move and rotate with each Director instance, allowing the same Timeline asset to be reused at different scene poses. If no humanoid is bound when the clip is created, its zero position and identity rotation resolve to the Director's own world pose.

When a Director Timeline containing an older world-space clip is inspected, the editor upgrades that clip to Director-local storage. Position, rotation, and either the legacy bend-target point or current pole vector are converted together, so the current world-space pose does not jump. Hand and foot clips use the same migration path; cloning a legacy clip also upgrades the clone immediately. Runtime evaluation and Scene preview use the same effective Director-local fallback even before the upgraded asset is saved, so target type cannot select a different coordinate convention.

When `anchorTransform` is assigned, it overrides the implicit Director anchor. Position follows the anchor's world position exactly, while rotation remains an anchor-local target frame and the pole value remains an anchor-local direction.

This is useful for hands following tools, doors, drawers, levers, and other scene objects that may move after the Timeline clip was authored.

## Public scripting API

- `HumanoidIKTrack.target` selects the controlled limb and `autoRenameClips` controls token-based clip renaming after target changes.
- `HumanoidIKClip.RotationSpace`, `BendSpace`, `UsesHumanoidEffectorRotation`, and `UsesHumanoidPoleDirection` expose the serialized coordinate conventions. `clipCaps` reports blending and extrapolation support.
- `GetGizmoColor(target)` resolves the custom or target-default preview color; `SetGizmoColor(color)` stores a custom value.
- Serialized hand limits are `digitBendRanges`, `thumbSpreadRange`, and `fingerSpreadRanges`. `EnsureDigitBendRangesInitialized()` upgrades missing range data and reports whether serialized data changed; `GetDigitBendRange(digitIndex, jointIndex)`, `GetThumbSpreadRange()`, and `GetFingerSpreadRange(digitIndex)` return effective limits.
- `HumanoidIKJointBend` stores `proximal`, `intermediate`, and `distal`. `HumanoidIKDigitBendPose` stores the five hand/toe chains; both structs support weighted addition and multiplication for blending.
- `HumanoidIKLimbBones` exposes `Upper`, `Lower`, `End`, and `IsValid`.
- `HumanoidIKUtility.IsHand`, `IsFoot`, `IsUsableHumanoid`, and `TryGetLimbBones` are the basic validation and mapping helpers.
- `ResolveWorldPose`, `ResolveWorldVector`, `ResolveWorldDirection`, `ResolveBendVector`, and `ResolveBendDirection` convert authored anchor-space data. `ToEffectorRotation` and `ToBoneRotation` convert between anatomical effector and imported bone frames.
- Digit helpers are `GetDigitChains`, `GetToeRigKind`, `GetToeRoot`, `GetArticulatedToeFanOffset`, `GetDefaultToeBendRange`, `GetToeBaseBendAngle`, `ClampToeBend`, `ClampToeFootBend`, and `GetDigitBend`.
- `HumanoidIKLateUpdateDriver.GetOrCreate(animator)` returns the per-Animator runtime driver used by the mixer and is not needed for ordinary clip authoring.

`HumanoidIKReferencePose` and `HumanoidIKDigitChainCache` capture immutable Avatar data and cached mapped finger/toe chains, but both types are internal implementation details. `HumanoidIKBehaviour` mirrors clip fields as Playable transport state. Internal goal, sample, evaluated-state, muscle-solver, and quaternion-accumulator types are likewise not external package API even when their own members are declared public.

## Scene handles

The clip inspector draws Scene view Handles for the selected clip:

- target or explicit-anchor position handle
- target or explicit-anchor rotation handle
- bend target position handle
- the dotted bend guide connects the solved preview elbow or knee to the bend target handle
- target transform handles are rendered as an interaction overlay after the depth-tested limb preview. The Scene view Move tool shows position handles, the Rotate tool shows the rotation handle, and the Transform tool shows both
- when an explicit `Anchor Transform` is assigned, selecting the clip keeps the position and rotation handles at that anchor and edits the anchor Transform directly. The clip's unused position and anchor-local rotation remain unchanged, so authoring does not require switching to the Hierarchy object
- Scene view Global mode aligns position and rotation axes to world space; Local mode aligns them to the explicit anchor rotation when present, otherwise to the authored Humanoid effector rotation. A rotation drag latches both the controlled rotation and handle frame at mouse-down and applies only the drag-start-relative delta, so the newly authored value cannot feed back into the same drag. The bend target position handle follows the same Move/Transform tool and axis mode
- a limb preview using the bound character's current upper/lower/end bone lengths
- a depth-tested primitive preview for the selected hand or foot, with lit fills and wire outlines
- without an explicit anchor, the hand rotation handle represents the clip's Humanoid effector frame and the bound Avatar's cached correction is used only to pose the preview and runtime wrist
- without an explicit anchor, the foot rotation handle and canonical gizmo use the resolved clip sole frame directly. Runtime converts that same frame through the bound Avatar's immutable Foot-bone axis mapping, so the diagonal imported Foot bone can retain its ankle-to-toe orientation while the gizmo remains a sole-aligned target
- legacy Foot-effector clips that stored the sloped ankle-to-toe frame are interpreted through both cached corrections. This preserves their resulting Foot bone pose while displaying and converting them in the projected sole frame. Newly created, captured, manually rotated, or explicitly converted clips store the projected sole frame version
- hand dimensions continue to use immutable Avatar reference poses when available. Foot fitting may use only immutable Avatar reference metrics; it must never use the live pose, IK result, Timeline weight, or a per-frame bone rotation
- when no usable humanoid is bound, the same canonical foot and fixed default hand anatomy keep the target handles available for authoring. A Timeline binding change invalidates cached Avatar fitting data and explicitly repaints the Scene view, so unbinding immediately restores all five default sole slabs
- the limb, bend guide, and hand/foot preview color and opacity come from the selected clip's `gizmoColor`
- primitive edges use the selected `gizmoColor` unchanged; filled faces use a Scene-view-compatible lit shader with RGB multiplied by 0.8 and the selected Alpha unchanged
- filled faces write to the Scene view depth buffer, while edges only depth-test, so nearer palm and digit primitives occlude the geometry and wire edges behind them
- finger and toe joints are drawn as cylinders and small joint spheres. Finger rendering remains unchanged. Every toe preview, bound or unbound, uses the original two display segments at 40% / 32% instead of copying the hand's three-segment structure; the removed 28% distal segment is not redistributed, so the visible chain ends at 72% of its source length. Intermediate and distal authored bends are combined into the second display segment so no toe channel becomes visually inert. The 150% source multiplier therefore produces a final visible reach of 108% of the anatomical base length rather than stretching back to 150%. The big toe keeps its 120% radius multiplier and 24% diameter-to-displayed-length ratio; toes two through five apply an additional 60% radius scale, producing a 14.4% ratio
- every foot uses one canonical silhouette generator in the intrinsic frame +Z = toes, +Y = dorsum, and +X = left-foot medial. The right foot mirrors X exactly once. The unbound adult reference is 0.125 m from Foot pivot to the shared Toe Base, 0.10 m at the forefoot after the lateral allowance, and approximately 0.294 m across the rendered heel-to-big-toe envelope. Connected slab coverage remains 1.2 times the Foot-to-Toe reference span and thickness remains 0.055 m
- without a usable binding, that adult reference silhouette is drawn unchanged. With a usable Humanoid binding, immutable Avatar reference-pose metrics fit the same silhouette: base longitudinal scale is exactly projected Foot-to-Toes distance / 0.125 m, after which all slab boundaries expand by 1.2 around the mapped Toe Base; the articulated big-to-little root span is compared with the 0.072 m canonical span to determine forefoot width; and the vertical Foot-to-Toes drop contributes only to required slab height. The mapped `LeftToes` or `RightToes` reference position is the bound Toe Base pivot, while extra longitudinal coverage grows toward the heel. The fit is cached with the Avatar and never reads the live animated pose, IK result, or current weights
- five connected sole slabs form a compact heel, ankle body, moderately tapered and laterally shifted midfoot, metatarsal expansion, and low full-width forefoot. One additional toe bridge box spans from the shared Toe Base pivot to the average visual digit-root row, using the forefoot slab's fitted width, height, and sole level so the gap never reappears. In the canonical left foot, every sole slab and the bridge extend an additional 10 mm only toward the lateral, little-toe side while their medial edges remain fixed; the right foot mirrors that rule, and a bound fit scales the allowance with the measured foot width. Every box bottom shares one sole level in the neutral pose. Without a binding, canonical toe centers remain one rendered radius above that level. With a mapped Toe bone, the sole is instead fixed exactly 10 mm below the immutable shared Toe pivot in the display-local -Y direction to allow for skin or shoe thickness. Under no circumstance may the boxes be rotated to follow the diagonal Foot-to-Toes bone line; that line supplies separate horizontal length and vertical drop measurements
- five canonical toes are always present from medial big toe to lateral little toe. The canonical 72 mm big-to-little root span uses center gaps of 22 / 17 / 16 / 17 mm, keeping extra separation between the big and second toes while grouping the four lesser toes more tightly; the right foot mirrors this layout once. The mapped Humanoid `LeftToes` or `RightToes` position is exclusively the shared Toe Base bend pivot and must never be reused as a visual digit root. Without per-digit bones, the first-sphere centers are synthesized 57-65 mm forward of that pivot in canonical +Z and this distance scales with the bound foot length. An articulated binding places each available toe's first sphere center at that branch root's exact immutable reference-pose position; this override is never lifted or clamped to the sole. Its preview direction comes from the first mapped root-to-child span projected into the same immutable sole plane. Mapped spans use a 1.15 mesh-coverage multiplier and are capped to 75%-100% of that digit's fitted canonical length; their radius remains based on the fitted canonical digit with a bound-only 90% scale, so shortening no longer over-couples length and thickness. Sparse one-bone branches are not extrapolated to replace the removed distal display joint. After fitting, every bound and unbound toe renders only the same 72% reach. Simple and toe-less rigs retain the scaled canonical five-toe layout; a simple `Foot-Toe` binding still uses its mapped Toe transform as the shared group pivot because it has no per-digit roots
- rig data affects only authored toe angles: no toe mapping leaves all five toes neutral; a simple `Foot-Toe` mapping applies the first toe bend to all five previews; an articulated mapping applies the existing big-to-little per-toe bend and fan values
- `Toe Base Bend` rotates only the forefoot slab, toe bridge, and all five canonical toe chains around the canonical toe-base pivot when unbound, or the exact shared mapped Toe pivot when bound. Heel, body, arch, Foot pivot, and Foot rotation remain unchanged
- hand finger previews always resolve the selected clip's absolute muscle-angle target through the same Humanoid mapping used at runtime, independent of Timeline Preview state and `digitWeight`
- foot toe previews likewise display the clip's full authored bend independently of `digitWeight`; the Weight control affects the applied character bones, never the target gizmo's intrinsic shape or angle
- the applied character pose may still be blended by `digitWeight`, but the hand/foot gizmo never samples the live post-IK digit chain and therefore remains a stable view of the authored clip target

The preview does not create target GameObjects. Interactive controls use Handles, while
the filled hand/foot primitives are queued only during `EventType.Repaint` and submitted
as instanced fill and outline batches grouped by mesh and color. Multi-selection keeps one
reusable reference-pose and HumanPose preview context per bound Animator, so alternating
clips bound to different characters does not rebuild those resources on every Scene GUI event. All contexts are
disposed when the Inspector is disabled or rebuilt when the Animator's Avatar changes.

The Scene view also provides a `Humanoid IK` overlay panel when a `HumanoidIKClip` is selected in Timeline:

- The panel is a transient Unity overlay whose `visible` state is true only while a `HumanoidIKClip` has a valid Timeline context. Unity owns the actual rendering, docking, and collapsed state; the implementation does not poll editor updates or write `Overlay.displayed`. If a script compile or domain reload clears the Timeline selection, Unity hides the overlay container instead of leaving an empty shell, and selecting the clip again shows it normally. A floating panel records its Scene view position in `SessionState` immediately before assembly reload and restores it after the rebuilt overlay has valid geometry, preventing Unity's hidden transient layout pass from moving it back to the upper-left corner.
- The clip's gizmo color field is displayed immediately to the right of its Timeline clip name in the overlay header and exposes both RGB and Alpha.
- `Weight` controls `positionWeight`, `rotationWeight`, and `digitWeight` together. If those values differ because they were edited separately in the Inspector, the overlay shows a mixed value; changing `Weight` synchronizes all three to the selected value.
- `Bend Weight` controls how strongly the authored Bend Target determines the elbow or knee plane.
- `Stretch` is an overall `-1..1` pose control where `-1` is a fist, `0` is the default pose, and `1` is fully open. It maps every whole-finger and joint bend to the same pose while leaving the thumb's independently authored `Joint 1 Y` Spread value unchanged.
- For foot targets, `Stretch` has the same relationship to bends as it does for hands: outside a drag it is derived from the available toe controls, and changing it rewrites them on the shared `-1..1` pose scale. Articulated rigs include the mapped Humanoid Toes root and all five child branches in that operation. A simple `Foot-Toe` rig keeps Stretch and its single joint slider synchronized without adding an independent root offset; that single mapped transform rotates the toe bridge and all five synthetic roots together around the shared Toe Base pivot, with no second copy of the same bend applied at the visual roots.
- Articulated foot targets expose the mapped Humanoid Toes root as a horizontal `Toe Bone Bend` slider centered vertically before the five branch-specific sliders in the foot diagram. It uses the same left-to-right value axis as every toe bend control and rotates the shared root before the child toe bends. The canonical forefoot slab and all five toe chains follow that bend around the fixed Toe Base pivot, while the rear four slabs remain in the Foot frame. Simple `Foot-Toe` targets omit it because their existing first toe-joint slider already controls that same mapped transform.
- `Finger Fan` is an independent `-1..1` control for the index-through-little proximal `Spread` muscles. Positive values move every finger toward its Max endpoint to fan the hand open, while negative values move every finger toward Min to close toward the middle finger. Unity's Humanoid mapping already gives Index and Ring/Little opposite spatial directions, so the control does not invert individual finger muscle signs.
- Foot targets replace `Finger Fan` with `Toe Fan`. The five articulated toe branches receive a narrow symmetric proximal fan; simple `Foot-Toe` rigs preserve but ignore it.
- The default fan ranges give the outer fingers more travel and keep the middle finger nearly stationary: Index and Little use `-20..20`, Ring uses `-7.5..7.5`, and Middle uses `-1..1`.
- Hand controls are arranged on a compact hand diagram that uses `Editor/Resources/HumanoidIK/hand_dark.png` in the dark theme and `hand_light.png` in the light theme.
- The source image points to the right for `LeftHand`; `RightHand` mirrors the image and control layout horizontally.
- Each finger has a whole-finger curl slider placed beyond the fingertip side of the hand, using the same `-1..1` pose scale.
- Three longer sliders placed along each finger control its first, second, and third joints with the same `-1..1` pose scale.
- All three thumb bend sliders are horizontal, matching the other fingers.
- The clip inspector's `Finger Stretch Angle Ranges` foldout provides a Min/Max control for all three Humanoid `Stretched` muscle angles of every finger. Every finger also has a `Joint 1 Y` Min/Max range for its Humanoid `Spread` muscle angle. The thumb range calibrates only the vertical thumb Spread slider; the index-through-little ranges calibrate Finger Fan. These calibration ranges are stored per clip.
- New clips initialize the thumb ranges to `Joint 1 X -60..0`, `Joint 1 Y -60..30`, `Joint 2 X -60..30`, and `Joint 3 X -60..30`. Every joint on the index, middle, ring, and little fingers initializes to `-60..50`.
- Every bend slider maps its full `-1..1` range directly across the clip's calibrated muscle angles: `-1` uses Min as the closed-fist endpoint, `1` uses Max as the fully open endpoint, and `0` is the angular midpoint.
- The thumb also exposes the diagram's only vertical slider beside the palm. It uses the clip's `Joint 1 Y` Min/Max calibration for the thumb proximal `Spread` muscle, while the horizontal proximal slider uses the `Stretched` muscle angle.
- Hover a compact slider to see its finger, joint, and current value.
- Foot controls use the matching `Editor/Resources/HumanoidIK/foot_dark.png` and `foot_light.png` diagram. `LeftFoot` uses the source orientation; `RightFoot` mirrors both the image and control layout like the hand overlay.
- Toe rows with multiple discovered joints place a whole-toe Curl slider plus only the joint sliders backed by transforms that exist. A one-joint row hides the redundant whole-toe Curl slider; a simple rig therefore exposes only its mapped toe joint and edits the first per-toe bend entry directly. On articulated rigs, a shared horizontal Toe Bone slider sits on the middle row at the toe-root boundary, followed by the per-branch joint sliders inside the illustrated toe region. The available joint width is divided between existing joints and shortened slightly toward the little toe. The complete layout mirrors once for `RightFoot`. When the foot diagram contains only one or two visible sliders, those controls expand across the available forefoot and toe area.
- While a hand slider is being dragged, the gizmo updates immediately from the clip value and the Timeline graph refresh is deferred until the control is released. This avoids repeatedly destroying and recreating the preview driver during one drag.
- While `Stretch` is being dragged, its direct input value stays latched from the first mouse change until the actual `MouseUp` event. Transient UI Toolkit `Ignore` events and temporary IMGUI hot-control loss cannot end the drag or trigger a Timeline rebuild.
- Outside an active drag, the overlay derives `Stretch` from the clip's authored joint angles. During a drag it displays the latched direct input value instead, so Timeline evaluation cannot feed reconstructed joint values back into the slider before `MouseUp`.
- The Min/Max calibration ranges map slider values to angles but do not automatically rewrite existing clip angles merely because the overlay is redrawn.

## Notes

- The bound `Animator` must be humanoid.
- New clips, `Capture Current Limb Pose`, and Scene handle edits use anatomical display rotations for both hands and feet. They preserve the raw pole-vector magnitude; direction normalization happens only inside runtime evaluation.
- Clips created before effector-space support keep their serialized rotation numbers until explicitly converted with the original Avatar bound. Conversion preserves the visible hand or foot pose by translating the legacy bone rotation through that Avatar's bone-to-display mapping.
- Correctly mapped Humanoid Avatars are normalized automatically. Rigs with missing or incorrect hand, foot, finger, or toe mappings can only use the available fallback anatomy and may still require Avatar correction.
- Hand digit bends use Unity Humanoid finger muscles. This keeps the clip-authored angle space stable across different Humanoid rigs and lets the Avatar handle local-axis differences.
- Foot digit bends use the humanoid toes transform and child chains that exist in the model. Multi-toe rigs use the mapped Toes transform as their shared base plus its child branches as separate digits. Those branches are ordered from big toe to little toe by their immutable Avatar reference-pose position along the character's lateral axis, so prefab sibling order does not swap the fourth and fifth toe controls. Common one-chain humanoids use the explicit `Foot-Toe` behavior described above.
- Only one `HumanoidIKTrack` should control a given limb on the same Animator. Duplicate tracks are not blocked, but the Timeline and track Inspector warn that evaluation order would decide which track wins.
