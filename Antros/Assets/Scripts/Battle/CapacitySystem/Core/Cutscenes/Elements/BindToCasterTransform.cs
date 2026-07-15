 using ATCG.Battle.CapacitySystem.Core.Properties;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements
{
    public class BindToCasterTransform : MonoBehaviour, ICapacityCutsceneElement
    {
        [SerializeField]
        private UnityEvent<Transform> onConnected;

        [SerializeField]
        private bool bindToBone;
        [SerializeField, ShowIf(nameof(bindToBone))]
        private HumanBodyBones customBone;

        // Resolves the caster actor from the context (game or preview) and fires the
        // bind event with the requested transform: a specific bone when bindToBone and
        // an animator is available, otherwise the actor's root transform.
        public void Connect(ICapacityContext context)
        {
            if (!context.TryGetProperty(CapacityContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;

            if (bindToBone && caster.Animator != null)
                onConnected?.Invoke(caster.Animator.GetBoneTransform(customBone));
            else
            {
                onConnected?.Invoke(caster.transform);
            }
        }

        public void Disconnect()
        {

        }
    }
}