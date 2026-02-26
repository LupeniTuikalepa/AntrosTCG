using UnityEngine;

namespace ATCG.Battle.Players.Runtime.UI
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/PlayerManaIcon")]
    public class PlayerManaIcon : MonoBehaviour
    {
        private static readonly int Active = Animator.StringToHash("IsActive");
        public bool IsActive { get; private set; }

        [SerializeField]
        private Animator animator;


        public void Activate()
        {
            if(IsActive)
                return;

            IsActive = true;
            animator.SetBool(Active, true);

        }

        public void Deactivate()
        {
            if(!IsActive)
                return;

            IsActive = false;
            animator.SetBool(Active, false);
        }
    }
}