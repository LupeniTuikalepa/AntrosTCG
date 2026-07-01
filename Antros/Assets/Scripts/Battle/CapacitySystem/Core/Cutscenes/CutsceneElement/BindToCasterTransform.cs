using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Entities.Runtime.Animations;
using ATCG.Battle.Players.Local.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.CutsceneElement
{
    public class BindToCasterTransform : MonoBehaviour, ICapacityCutsceneElement
    {
        [SerializeField]
        private UnityEvent<Transform> onConnected;

        [SerializeField]
        private bool bindToBone;
        [SerializeField, ShowIf(nameof(bindToBone))]
        private HumanBodyBones customBone;

        public void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer, CastCapacityPhase capacityPhase)
        {
            if (!capacityPhase.HasCaster)
                return;

            if (runtimeLocalBattlePlayer.RuntimeEntityManager.TryGetRuntimeEntity(capacityPhase.caster, out IRuntimeEntity entity))
            {
                if (bindToBone && entity is IRuntimeEntityWithAnimator entityWithAnimator)
                    onConnected?.Invoke(entityWithAnimator.Animator.GetBoneTransform(customBone));
                else
                    onConnected?.Invoke(entity.transform);
            }
        }
    }
}