# Look At Track

`LookAtTrack` turns a bound Humanoid or Generic Animator toward an authored target position during Timeline playback and preview.

## Source files

- Track: `Runtime/LookAt/LookAtTrack.cs`
- Clip and serialized options: `Runtime/LookAt/LookAtClip.cs`
- Behaviour and mixer: `Runtime/LookAt/LookAtBehaviour.cs`, `Runtime/LookAt/LookAtMixerBehaviour.cs`
- Late update driver, Generic mapping, and shared types: `Runtime/LookAt/LookAtLateUpdateDriver.cs`, `Runtime/LookAt/LookAtGenericRigMapping.cs`, `Runtime/LookAt/LookAtTypes.cs`
- Editor authoring: `Editor/LookAt`
- EditMode tests: `Tests/Editor/LookAtTests.cs`

## Setup

1. Add a **Look At Track** to a Timeline.
2. Bind a Humanoid or Generic `Animator`.
3. For a Generic Animator, select the track and verify the automatically populated **Pelvis** and **Head** reference fields. The bound Animator owns its own mapping, so changing the track binding initializes the new character independently. The derived Body/Neck and Eye fields are shown in the default-closed **Manual Bone Setup** foldout. Use **Initialize Bone Mapping** to overwrite all mapped bones from the current Pelvis and Head.
4. Add a Look At clip and either assign its optional **Target** Transform or edit its **Position**.
5. Adjust **Chin Offset** to lower or raise the chin while the eyes remain on the target.
6. Adjust **Eyes**, **Head**, **Neck**, and **Body** rotation weights.
7. Set each body's **Horizontal (Yaw)** and **Vertical (Pitch)** minimum/maximum rotation limits.
8. Blend or ease clips to transition between targets, chin poses, body-part weights, and angle limits.

The target reference is stored as an `ExposedReference<Transform>`, so scene objects remain compatible with Timeline binding and prefab workflows.

## Clip options

- `target`: optional exposed Transform target. Its world position takes priority when it resolves.
- `position`: Director-local fallback target used without a resolved Transform.
- `gizmoColor`: shared color and opacity for the clip's Scene view target gizmo and Timeline bottom accent. The existing weight-tinted clip background and automatic-blink markers remain separate.
- `chinOffset`: normalized head-pitch bias from `-1` (chin down) to `1` (chin up). `0` is neutral, and new clips default to `-0.1` for a slightly lowered chin.
- `eyesWeight`, `headWeight`, `neckWeight`, `bodyWeight`: independent `0..1` channel strengths. New clips default to `1`, `0.5`, `0.2`, and `0.05`.
- `eyesAngleLimits`, `headAngleLimits`, `neckAngleLimits`, `bodyAngleLimits`: per-channel yaw and pitch ranges.
- Blink fields: `blinkBlendShapeKeys`, `blinkMode`, `blinkCurve`, `blinkFrequency`, `blinkDuration`, `automaticBlinkCurve`, and `blinkNoiseOffset`.
- Upper-eyelid follow fields: `upperEyelidFollowBlendShapeKeys`, `upperEyelidFollowWeight`, and `upperEyelidFollowCurve`.
- Lower-eyelid follow fields: `lowerEyelidFollowBlendShapeKeys`, `lowerEyelidFollowWeight`, and `lowerEyelidFollowCurve`.
- Horizontal eyelid follow fields: `horizontalEyelidFollowBlendShapeKeys`, `horizontalEyelidFollowWeight`, and `horizontalEyelidFollowCurve`.
- `clipCaps` reports blending and extrapolation support.

## Target position

- When **Target** resolves to a Transform, its current world position is used.
- Otherwise, **Position** is used and stored in the owning `PlayableDirector` object's local space.
- A new clip without a usable bound rig starts at local `(0, 1, 1)`.
- With a usable bound rig, a new clip starts one Director-local unit forward from the current eye center. The head position is used when no eye bones are mapped.

Transform targets and Director-local positions are resolved when the driver applies its stored state. In Edit mode, moving a Transform target therefore updates the character's look direction without scrubbing or rebuilding the Timeline graph.

The local position is converted with the Director transform every frame, so moving, rotating, or scaling the Director also moves the authored look target.

## Blending

Each body-part channel is blended independently:

- Timeline input weight is multiplied by the clip's body-part weight.
- Each clip resolves either its Transform target or its Director-local Position to a world position.
- A channel's target position is the weighted average of all contributing resolved positions.
- Horizontal and vertical limits are averaged using the same contributing channel weights.
- Chin Offset is averaged with the contributing Head channel weights.
- The summed channel weight is clamped to `0..1` before it rotates bones.

Transform targets and stored positions can participate in the same blend. This also allows, for example, the eyes to retain one target while the head begins blending toward another.

## Chin offset

**Chin Offset** adds up to 30 degrees of pitch to the Head channel before its rotation limits and weight are applied. Negative values lower the chin so the eye bones look upward toward the unchanged target; positive values raise the chin so the eyes look downward toward it. Eyes, Neck, and Body continue to use the original target direction, and the final relative eye direction continues to drive the configured eyelid-follow BlendShapes.

## Blinking and eyelid follow

- `Automatic` blink mode creates deterministic blinks from local clip time, `blinkFrequency`, `blinkDuration`, `blinkNoiseOffset`, and `automaticBlinkCurve`. Frequency `0` disables automatic blinking.
- `AnimationCurve` mode evaluates `blinkCurve` over normalized clip time. Curve value `0` closes the eyelids and `1` opens them.
- Every matching BlendShape below the bound Animator is driven; blank keys and renderers without the named shape are skipped.
- Blink weights blend across overlapping clips. Directional eyelid follow is evaluated from the resolved eye pitch/yaw after Look At rotation, then combined with blinking for each matching shape.
- Upper follow responds while looking down, lower follow while looking up, and horizontal follow can use side/direction tokens in its cached key mapping. Their curves use normalized direction input: `0` is down/left, `0.5` is forward, and `1` is up/right.
- Timeline preview property collection includes all configured eyelid BlendShapes, so preview restoration should not leave modified weights behind.

## Rotation limits

Each of **Eyes**, **Head**, **Neck**, and **Body** has two independent Min-Max sliders:

- **Horizontal (Yaw)** limits left/right turning.
- **Vertical (Pitch)** limits downward/upward turning.

Both ranges are stored in degrees and can be set from `-180` to `180`. New clips default to Eyes Yaw `(-40, 40)`, Eyes Pitch `(-25, 25)`, and `(-90, 90)` for both axes on Head, Neck, and Body. Previously authored clips retain their serialized limits; legacy clips without initialized limits remain unrestricted.

Limits clamp the requested turn relative to the current animated forward direction before the channel weight is applied. This preserves the animation pose while preventing the Look At contribution from exceeding the authored range. The Body range limits the combined mapped upper-body chain rather than being applied once per Spine, Chest, and Upper Chest bone.

## Rig mapping

Humanoid Animators continue to use the Avatar mapping:

- **Eyes** uses mapped `LeftEye` and `RightEye` bones. If neither eye is mapped, the eye channel is ignored.
- **Head** uses `Head`.
- **Neck** uses `Neck` when mapped.
- **Body** distributes its weight across mapped `Spine`, `Chest`, and `UpperChest` bones.

For Generic Animators, a hidden `LookAtGenericRigMapping` component on the bound Animator stores direct Transform references. This keeps scene references out of the Timeline asset and gives every bound character an independent mapping:

- **Pelvis** is the lower boundary of the Look At chain and is not rotated. The transforms between Pelvis and Neck are distributed across the Body channel. Clearing Pelvis therefore ignores Body while Head and Neck can continue to work.
- **Head** is the required Head bone. Its immediate parent is used as Neck. Clearing Head ignores Generic Look At rotation.
- The fields are populated when a Generic binding is first inspected. A Generic Animator without a stored component uses automatic detection at runtime. Clearing an initialized field is preserved as an intentional Ignore choice.
- Eye bones are resolved internally below Head. Names containing `eye` are considered after eyelid, lash, brow, target, and aim names are excluded. Names containing `left` or `right`, and standalone `L`/`R` tokens separated by spaces, `-`, `_`, `.`, or similar delimiters, are preferred; local X is only a side fallback. Eye position never determines character forward.
- **Manual Bone Setup** is closed by default and shows the stored mapping used at runtime. **Body Bones** uses Unity's standard Transform array UI in lower-to-upper order; its last valid element is Neck and its preceding elements use Body weight. Left Eye and Right Eye are stored separately. Editing any field is the manual override and empty fields are ignored. **Initialize Bone Mapping** is the only reset action and overwrites the mapped Body, Neck, and Eye fields from the current Pelvis and Head.

For both rig types, the reference forward is the bound Animator Transform's local `+Z`. The runtime transforms that direction into each bone's imported reference frame. Generic rigs align mesh bind space to the Animator through the renderer's root bone before calculating each local forward axis; this supports rigs whose Head forward is local `+Y` or another axis. The current hierarchy frame is used only when no usable bind pose exists.

## Scene view

While a Timeline window is open, every Look At clip whose GUI is currently drawn displays a target marker at its resolved position. The marker, dotted lines, and clip-bottom accent share that clip's **Gizmo Color**. With two mapped eyes, one dotted line starts at each eye; with one mapped eye, a single line starts there. Without eyes, the line starts at a direct Head-end child such as `Head_End` when available, then falls back to Head itself. Unselected clips use a lighter Scene view preview, and selected clips are drawn at full opacity on top.

For clips without a resolved Transform target, the Move and Transform tools expose a position handle. Global mode uses world axes, while Local mode follows the `PlayableDirector` rotation. Handle edits are converted back into Director-local Position values. The marker and line are authoring aids and do not change with clip weight.

## Runtime order

Timeline samples are collected by `LookAtMixerBehaviour` and applied by the hidden `LookAtLateUpdateDriver` after normal animation evaluation. Its execution order is before `HumanoidIKLateUpdateDriver`, allowing hand and foot IK to solve after upper-body look rotation during runtime.

## Public scripting API

- `LookAtAngleLimits` stores `horizontal` and `vertical` ranges, exposes `MinimumAngle`, `MaximumAngle`, and `Unrestricted`, and can be constructed from two `Vector2` ranges.
- `LookAtBlinkMode` selects `Automatic` or `AnimationCurve` evaluation.
- `LookAtLateUpdateDriver.GetOrCreate(animator)` returns the hidden per-Animator application driver used by the mixer; normal user code should configure Timeline clips instead of calling it directly.

`LookAtBehaviour` public fields mirror all clip options plus resolver/local-time transport data. The remaining public-looking members in `LookAtTypes.cs` and the driver belong to internal state or private nested types and are not externally callable package API.

## Agent notes

- Inspect both `LookAtClip` and `LookAtTypes` when changing blink curves, direction parsing, or BlendShape matching; the clip owns serialization while shared helpers own evaluation and caches.
- Preserve deterministic automatic blinking across Timeline scrubbing. Do not replace clip-time sampling with frame-time randomness.
- Keep Look At before Humanoid IK in late-update order unless the intended pose dependency is deliberately changed and visually revalidated.
