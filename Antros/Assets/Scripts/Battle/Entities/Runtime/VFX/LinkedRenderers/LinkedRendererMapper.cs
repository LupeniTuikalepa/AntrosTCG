using System.Collections.Generic;
using ATCG.Battle.Entities.Runtime.Heroes;
using ATCG.Battle.Entities.Runtime.VFX;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    /// <summary>
    /// Auto-tags every renderer under the character with a <see cref="LinkedRendererKey"/>,
    /// no manual per-object configuration needed for body-part keys:
    /// - Static props (MeshRenderer, e.g. a sword) walk up the hierarchy to the nearest
    ///   ancestor that IS a mapped humanoid bone — parent a sword under RightHand and it
    ///   picks up RightHand automatically.
    /// - Skinned meshes (SkinnedMeshRenderer — the body, or a skinned clothing piece)
    ///   aren't parented under a single bone, so instead this unions the keys of every
    ///   bone in their own bones[] array.
    /// Clothes/Weapons have no geometric signal to infer from, so they're intentionally
    /// left out here — tag those by hand on the LinkedRenderer for now.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class LinkedRendererMapper : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        private void Awake()
        {
            if (animator != null)
                animator = GetComponentInChildren<Animator>();

            Map();
        }

        private void Reset()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        public void SetAnimator(Animator a)
        {
	        this.animator = a;
	        Map();
        }

        // Only the humanoid bones with a meaningful LinkedRendererKey equivalent are
        // mapped — fingers, jaw articulation details, etc. have no key and are simply
        // skipped when walking up toward a mapped ancestor or unioning skinning bones.
        private static LinkedRendererKey ToLinkedKey(HumanBodyBones bone) => bone switch
        {
            HumanBodyBones.Head => LinkedRendererKey.Head,

            HumanBodyBones.LeftEye or HumanBodyBones.RightEye => LinkedRendererKey.Eyes,
            HumanBodyBones.Jaw => LinkedRendererKey.Mouth,

            HumanBodyBones.LeftShoulder or HumanBodyBones.LeftUpperArm or HumanBodyBones.LeftLowerArm
                => LinkedRendererKey.LeftArm,
            HumanBodyBones.RightShoulder or HumanBodyBones.RightUpperArm or HumanBodyBones.RightLowerArm
                => LinkedRendererKey.RightArm,

            HumanBodyBones.LeftHand => LinkedRendererKey.LeftHand,
            HumanBodyBones.RightHand => LinkedRendererKey.RightHand,

            HumanBodyBones.LeftUpperLeg or HumanBodyBones.LeftLowerLeg or HumanBodyBones.LeftFoot or HumanBodyBones.LeftToes
                => LinkedRendererKey.LeftLeg,
            HumanBodyBones.RightUpperLeg or HumanBodyBones.RightLowerLeg or HumanBodyBones.RightFoot or HumanBodyBones.RightToes
                => LinkedRendererKey.RightLeg,

            HumanBodyBones.Hips or HumanBodyBones.Spine or HumanBodyBones.Chest or HumanBodyBones.UpperChest or HumanBodyBones.Neck
                => LinkedRendererKey.Chest,

            _ => LinkedRendererKey.None,
        };

        private void Map()
        {
	        if (animator == null)
		        return;
	        
            using (DictionaryPool<Transform, LinkedRendererKey>.Get(out var boneKeys))
            {
                BuildBoneKeyTable(boneKeys);

                foreach (Renderer renderer in animator.GetComponentsInChildren<Renderer>(true))
                {
                    LinkedRendererKey key = renderer is SkinnedMeshRenderer skinned
                        ? KeyFromSkinning(skinned, boneKeys)
                        : KeyFromHierarchy(renderer.transform, boneKeys);

                    if (key == LinkedRendererKey.None)
                        continue;

                    LinkedRenderer linked = renderer.GetComponent<LinkedRenderer>();
                    if (linked == null)
                        linked = renderer.gameObject.AddComponent<LinkedRenderer>();

                    linked.SetRenderer(renderer);
                    linked.SetKeys(key);
                }
            }
        }

        // Every humanoid bone that has a mapped key, keyed by its Transform — the single
        // source of truth used both to resolve a prop's nearest tagged ancestor and a
        // skinned mesh's influencing bones.
        private void BuildBoneKeyTable(Dictionary<Transform, LinkedRendererKey> boneKeys)
        {
            foreach (object value in System.Enum.GetValues(typeof(HumanBodyBones)))
            {
                HumanBodyBones bone = (HumanBodyBones)value;
                LinkedRendererKey key = ToLinkedKey(bone);
                if (key == LinkedRendererKey.None)
                    continue;

                Transform t = animator.GetBoneTransform(bone);
                if (t == null)
                    continue;

                boneKeys[t] = boneKeys.TryGetValue(t, out LinkedRendererKey existing) ? existing | key : key;
            }
        }

        // Static props: walk up from the renderer to the nearest ancestor that IS a
        // mapped bone. A sword parented under RightHand picks up RightHand with no
        // manual tagging; anything not under a recognized bone yields None (skipped).
        private static LinkedRendererKey KeyFromHierarchy(Transform t, Dictionary<Transform, LinkedRendererKey> boneKeys)
        {
            for (Transform current = t; current != null; current = current.parent)
            {
                if (boneKeys.TryGetValue(current, out LinkedRendererKey key))
                    return key;
            }
            return LinkedRendererKey.None;
        }

        // Skinned meshes (body, or a skinned clothing piece) aren't parented under a
        // single bone — they're driven by their own bones[] array via bindposes — so this
        // unions the keys of every bone that actually influences the mesh instead.
        private static LinkedRendererKey KeyFromSkinning(SkinnedMeshRenderer renderer, Dictionary<Transform, LinkedRendererKey> boneKeys)
        {
            LinkedRendererKey combined = LinkedRendererKey.None;
            Transform[] bones = renderer.bones;
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != null && boneKeys.TryGetValue(bones[i], out LinkedRendererKey key))
                    combined |= key;
            }
            return combined;
        }
    }
}