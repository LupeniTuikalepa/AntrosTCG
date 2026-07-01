using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Animations
{
    public interface IRuntimeEntityWithAnimator : IRuntimeEntity
    {
        Animator Animator { get; }
    }
}