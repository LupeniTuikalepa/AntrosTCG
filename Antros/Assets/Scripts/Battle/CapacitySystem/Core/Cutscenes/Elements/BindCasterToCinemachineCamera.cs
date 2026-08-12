using ATCG.Battle.CapacitySystem.Core.Properties;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

using ATCG.Cutscenes;
namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements
{
    /// <summary>
    /// Automatic version of BindToCasterTransform for a Cinemachine camera: resolves the
    /// caster from the context and wires it straight onto the camera's Follow and/or
    /// LookAt, no UnityEvent/manual rig wiring needed. Follow and LookAt are independent —
    /// enable either one, both, or neither — and each can target the caster's root
    /// transform or one of its bones (if it has an Animator).
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class BindCasterToCinemachineCamera : MonoBehaviour, ICapacityCutsceneElement
    {
        [SerializeField]
        private CinemachineCamera cinemachineCamera;

        [Space]
        [SerializeField]
        private bool bindFollow = true;
        [SerializeField, ShowIf(nameof(bindFollow))]
        private bool followBindToBone;
        [SerializeField, ShowIf("@bindFollow && followBindToBone")]
        private HumanBodyBones followBone;

        [Space]
        [SerializeField]
        private bool bindLookAt;
        [SerializeField, ShowIf(nameof(bindLookAt))]
        private bool lookAtBindToBone;
        [SerializeField, ShowIf("@bindLookAt && lookAtBindToBone")]
        private HumanBodyBones lookAtBone;

        private void Reset() => cinemachineCamera = GetComponent<CinemachineCamera>();

        // Resolves the caster actor from the context (game or preview) and wires whichever
        // of Follow/LookAt is enabled onto its root transform or the requested bone.
        public void Connect(ICutsceneContext context)
        {
            if (cinemachineCamera == null)
                return;

            if (!context.TryGetProperty(CutsceneContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;

            if (bindFollow)
                cinemachineCamera.Follow = ResolveTransform(caster, followBindToBone, followBone);

            if (bindLookAt)
                cinemachineCamera.LookAt = ResolveTransform(caster, lookAtBindToBone, lookAtBone);
        }

        public void Disconnect()
        {

        }

        // Falls back to the caster's root transform when not asked for a bone, or when
        // asked for one but the caster has no Animator to resolve it from.
        private static Transform ResolveTransform(ICutsceneActor caster, bool useBone, HumanBodyBones bone)
        {
            if (useBone && caster.Animator != null)
            {
                Transform boneTransform = caster.Animator.GetBoneTransform(bone);
                if (boneTransform != null)
                    return boneTransform;
            }

            return caster.transform;
        }
    }
}
