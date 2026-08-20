using CutsceneEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace CutsceneEngineEditor
{
    [CustomTimelineEditor(typeof(LookAtTrack))]
    public sealed class LookAtTrackEditor : TrackEditor
    {
        public override TrackDrawOptions GetTrackOptions(
            TrackAsset track,
            Object binding)
        {
            var options = base.GetTrackOptions(track, binding);
            options.trackColor = new Color(0.62f, 0.38f, 0.95f);
            var icon = LookAtTrackIconProvider.GetIcon();
            if (icon)
            {
                options.icon = icon;
            }

            if (binding is not Animator animator)
            {
                options.errorText = "Bind an Animator.";
            }
            else if (animator.isHuman &&
                     !HumanoidIKUtility.IsUsableHumanoid(animator))
            {
                options.errorText =
                    "The bound Humanoid Animator must use a valid Avatar.";
            }
            else if (!animator.isHuman &&
                     track is LookAtTrack lookAtTrack &&
                     !LookAtGenericRigUtility.ResolveHead(
                         animator,
                         lookAtTrack))
            {
                options.errorText =
                    "Assign a Generic Head in the Track Inspector.";
            }

            return options;
        }
    }

    [CustomEditor(typeof(LookAtTrack))]
    public sealed class LookAtTrackInspector : Editor
    {
        bool _showManualBoneSetup;
        string _initializationMessage;

        public override void OnInspectorGUI()
        {
            var track = target as LookAtTrack;
            if (!track) return;

            var director = TimelineEditor.inspectedDirector;
            var animator = director
                ? director.GetGenericBinding(track) as Animator
                : null;
            if (!animator)
            {
                EditorGUILayout.HelpBox(
                    "Bind an Animator to this track in Timeline to configure its rig mapping.",
                    MessageType.Info);
                return;
            }

            if (animator.isHuman)
            {
                DrawHumanoidStatus(animator);
                return;
            }

            var mapping = GetOrCreateMapping(animator);
            if (!mapping) return;

            InitializeMappingIfNeeded(mapping, animator);
            DrawGenericMapping(track, animator, mapping);
        }

        static void DrawHumanoidStatus(Animator animator)
        {
            if (HumanoidIKUtility.IsUsableHumanoid(animator))
            {
                EditorGUILayout.HelpBox(
                    "Humanoid mapping is read from the Avatar automatically.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "The bound Humanoid Animator needs a valid Avatar.",
                    MessageType.Warning);
            }
        }

        void DrawGenericMapping(
            LookAtTrack track,
            Animator animator,
            LookAtGenericRigMapping mapping)
        {
            var mappingObject = new SerializedObject(mapping);
            mappingObject.Update();
            var initializedProperty =
                mappingObject.FindProperty("initialized");
            var pelvisProperty = mappingObject.FindProperty("pelvis");
            var headProperty = mappingObject.FindProperty("head");
            var bodyBonesProperty =
                mappingObject.FindProperty("bodyBones");
            var leftEyeProperty = mappingObject.FindProperty("leftEye");
            var rightEyeProperty = mappingObject.FindProperty("rightEye");

            EditorGUILayout.LabelField("Generic Rig", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Automatic Bone Detection",
                EditorStyles.miniBoldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                pelvisProperty,
                new GUIContent(
                    "Pelvis",
                    "Lower boundary used to detect the upper-body chain."));
            EditorGUILayout.PropertyField(
                headProperty,
                new GUIContent(
                    "Head",
                    "Head bone and the upper reference for automatic Body, Neck, and Eye detection."));

            EditorGUILayout.HelpBox(
                "The bound Animator owns this mapping. A different Generic character receives its own mapping automatically. The Animator Transform's local +Z is used as the character forward direction.",
                MessageType.Info);

            _showManualBoneSetup = EditorGUILayout.Foldout(
                _showManualBoneSetup,
                "Manual Bone Setup",
                true);
            if (_showManualBoneSetup)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "These fields are initialized from Pelvis and Head. Editing them directly overrides the detected mapping; empty entries are ignored. The last valid Body Bones element is used as Neck.",
                    MessageType.Info);
                EditorGUILayout.PropertyField(
                    bodyBonesProperty,
                    new GUIContent(
                        "Body Bones",
                        "Lower-to-upper array. The last valid element is Neck; preceding elements use Body weight."),
                    true);
                EditorGUILayout.PropertyField(
                    leftEyeProperty,
                    new GUIContent(
                        "Left Eye",
                        "Mapped left eye below Head."));
                EditorGUILayout.PropertyField(
                    rightEyeProperty,
                    new GUIContent(
                        "Right Eye",
                        "Mapped right eye below Head."));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                initializedProperty.boolValue = true;
                mappingObject.ApplyModifiedProperties();
                NotifyMappingChanged(mapping, animator);
                _initializationMessage = null;
            }

            DrawValidation(animator, mapping);

            if (!string.IsNullOrEmpty(_initializationMessage))
            {
                EditorGUILayout.HelpBox(
                    _initializationMessage,
                    MessageType.Warning);
            }

            if (GUILayout.Button("Initialize Bone Mapping"))
            {
                InitializeMapping(
                    mapping,
                    animator,
                    "Initialize Generic Look At Bone Mapping");
            }
        }

        static LookAtGenericRigMapping GetOrCreateMapping(
            Animator animator)
        {
            var mapping =
                animator.GetComponent<LookAtGenericRigMapping>();
            if (mapping) return mapping;

            mapping = Undo.AddComponent<LookAtGenericRigMapping>(
                animator.gameObject);
            if (!mapping) return null;

            mapping.hideFlags |= HideFlags.HideInInspector;
            EditorUtility.SetDirty(mapping);
            PrefabUtility.RecordPrefabInstancePropertyModifications(mapping);
            return mapping;
        }

        void InitializeMappingIfNeeded(
            LookAtGenericRigMapping mapping,
            Animator animator)
        {
            if (mapping.initialized) return;

            InitializeMapping(
                mapping,
                animator,
                "Initialize Generic Look At Bone Mapping");
        }

        void InitializeMapping(
            LookAtGenericRigMapping mapping,
            Animator animator,
            string undoName)
        {
            var root = animator.transform;
            var head = IsDescendantOf(root, mapping.head)
                ? mapping.head
                : LookAtGenericRigUtility.DetectHead(animator);
            var pelvis = IsValidPelvis(root, mapping.pelvis, head)
                ? mapping.pelvis
                : LookAtGenericRigUtility.DetectPelvis(animator, head);
            if (!LookAtGenericRigUtility.TryBuildAutomatic(
                    animator,
                    pelvis,
                    head,
                    out var automatic))
            {
                _initializationMessage =
                    "Automatic bones cannot be resolved for the bound Generic Animator.";
                return;
            }

            Undo.RecordObject(mapping, undoName);
            StoreMapping(mapping, in automatic, pelvis);
            mapping.initialized = true;
            NotifyMappingChanged(mapping, animator);
            _initializationMessage = null;
        }

        static void StoreMapping(
            LookAtGenericRigMapping mapping,
            in LookAtGenericRigDefinition definition,
            Transform pelvis)
        {
            var body = definition.Body ?? System.Array.Empty<Transform>();
            var bodyBones = new Transform[
                body.Length + (definition.Neck ? 1 : 0)];
            System.Array.Copy(body, bodyBones, body.Length);
            if (definition.Neck)
            {
                bodyBones[^1] = definition.Neck;
            }

            mapping.pelvis = pelvis;
            mapping.head = definition.Head;
            mapping.bodyBones = bodyBones;
            mapping.leftEye = definition.LeftEye;
            mapping.rightEye = definition.RightEye;
        }

        static void DrawValidation(
            Animator animator,
            LookAtGenericRigMapping mapping)
        {
            var root = animator.transform;
            var head = mapping.head;
            if (!head)
            {
                EditorGUILayout.HelpBox(
                    "Head is empty. Generic Look At rotation is ignored.",
                    MessageType.Warning);
                return;
            }

            if (!IsDescendantOf(root, head))
            {
                EditorGUILayout.HelpBox(
                    "Head must be below the bound Animator Transform.",
                    MessageType.Warning);
                return;
            }

            if (mapping.pelvis &&
                !IsValidPelvis(root, mapping.pelvis, head))
            {
                EditorGUILayout.HelpBox(
                    "Pelvis must be an ancestor of Head under the bound Animator.",
                    MessageType.Warning);
            }

            var bodyBones = mapping.bodyBones;
            if (bodyBones != null)
            {
                for (var i = 0; i < bodyBones.Length; i++)
                {
                    var bone = bodyBones[i];
                    if (!bone) continue;
                    if (IsDescendantOf(root, bone) &&
                        bone != head &&
                        head.IsChildOf(bone))
                    {
                        continue;
                    }

                    EditorGUILayout.HelpBox(
                        "Body Bones must be ancestors of Head under the bound Animator.",
                        MessageType.Warning);
                    break;
                }
            }

            if (!IsValidEye(root, head, mapping.leftEye) ||
                !IsValidEye(root, head, mapping.rightEye))
            {
                EditorGUILayout.HelpBox(
                    "Eye bones must be children of Head under the bound Animator.",
                    MessageType.Warning);
            }
        }

        static bool IsDescendantOf(Transform root, Transform bone)
        {
            return root && bone && bone != root && bone.IsChildOf(root);
        }

        static bool IsValidPelvis(
            Transform root,
            Transform pelvis,
            Transform head)
        {
            return IsDescendantOf(root, pelvis) &&
                   head &&
                   pelvis != head &&
                   head.IsChildOf(pelvis);
        }

        static bool IsValidEye(
            Transform root,
            Transform head,
            Transform eye)
        {
            return !eye ||
                   (IsDescendantOf(root, eye) &&
                    eye != head &&
                    eye.IsChildOf(head));
        }

        static void NotifyMappingChanged(
            LookAtGenericRigMapping mapping,
            Animator animator)
        {
            EditorUtility.SetDirty(mapping);
            PrefabUtility.RecordPrefabInstancePropertyModifications(mapping);

            var driver = animator
                ? animator.GetComponent<LookAtLateUpdateDriver>()
                : null;
            if (driver)
            {
                driver.InvalidateRig();
            }

            TimelineEditor.Refresh(
                RefreshReason.ContentsModified |
                RefreshReason.SceneNeedsUpdate);
            LookAtTimelinePreviewUpdater.RequestPreviewUpdate();
            SceneView.RepaintAll();
        }
    }

    internal static class LookAtTrackIconProvider
    {
        const string IconGuid = "385fb951da76ec247bfd0ce66f58916f";

        static Texture2D _icon;

        internal static Texture2D GetIcon()
        {
            if (_icon) return _icon;

            var path = AssetDatabase.GUIDToAssetPath(IconGuid);
            if (string.IsNullOrEmpty(path)) return null;

            _icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            return _icon;
        }
    }
}
