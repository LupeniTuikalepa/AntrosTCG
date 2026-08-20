using System;
using System.Collections.Generic;
using UnityEngine;

namespace CutsceneEngine
{
    internal readonly struct HumanoidIKReferenceBonePose
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;

        public HumanoidIKReferenceBonePose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }

    internal sealed class HumanoidIKReferencePose
    {
        readonly Animator _animator;
        readonly Avatar _avatar;
        readonly Dictionary<Transform, HumanoidIKReferenceBonePose> _bones =
            new Dictionary<Transform, HumanoidIKReferenceBonePose>();
        readonly List<Transform> _path = new List<Transform>();

        public bool IsValidFor(Animator animator)
        {
            return animator && animator == _animator && animator.avatar == _avatar && _bones.Count > 0;
        }

        public static bool TryCreate(Animator animator, out HumanoidIKReferencePose referencePose)
        {
            referencePose = null;
            if (!HumanoidIKUtility.IsUsableHumanoid(animator)) return false;

            var pose = new HumanoidIKReferencePose(animator);
            if (pose._bones.Count == 0) return false;

            referencePose = pose;
            return true;
        }

        public bool TryGetBonePose(Transform bone, out HumanoidIKReferenceBonePose pose)
        {
            pose = default;
            return bone && _bones.TryGetValue(bone, out pose);
        }

        public bool TryGetRelativeMatrix(Transform ancestor, Transform descendant, out Matrix4x4 matrix)
        {
            matrix = Matrix4x4.identity;
            _path.Clear();

            var current = descendant;
            while (current && current != ancestor)
            {
                _path.Add(current);
                current = current.parent;
            }

            if (current != ancestor) return false;

            for (var i = _path.Count - 1; i >= 0; i--)
            {
                if (!TryGetBonePose(_path[i], out var pose)) return false;
                matrix *= Matrix4x4.TRS(pose.Position, pose.Rotation, pose.Scale);
            }

            return true;
        }

        HumanoidIKReferencePose(Animator animator)
        {
            _animator = animator;
            _avatar = animator.avatar;
            BuildBoneMap(animator.transform, _avatar.humanDescription.skeleton, _bones);
        }

        internal static void BuildBoneMap(
            Transform root,
            SkeletonBone[] skeleton,
            Dictionary<Transform, HumanoidIKReferenceBonePose> destination)
        {
            destination.Clear();
            if (!root || skeleton == null || skeleton.Length == 0) return;

            var posesByName = new Dictionary<string, Queue<HumanoidIKReferenceBonePose>>();
            for (var i = 0; i < skeleton.Length; i++)
            {
                var bone = skeleton[i];
                if (string.IsNullOrEmpty(bone.name)) continue;
                if (!posesByName.TryGetValue(bone.name, out var poses))
                {
                    poses = new Queue<HumanoidIKReferenceBonePose>();
                    posesByName.Add(bone.name, poses);
                }

                poses.Enqueue(new HumanoidIKReferenceBonePose(bone.position, bone.rotation, bone.scale));
            }

            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var transform = transforms[i];
                if (!posesByName.TryGetValue(transform.name, out var poses) || poses.Count == 0) continue;
                destination.Add(transform, poses.Dequeue());
            }
        }
    }
}
