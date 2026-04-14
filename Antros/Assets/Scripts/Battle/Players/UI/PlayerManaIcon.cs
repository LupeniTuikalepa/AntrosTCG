using UnityEngine;

namespace ATCG.Battle.Players.UI
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/PlayerManaIcon")]
    public class PlayerManaIcon : MonoBehaviour
    {
        private static readonly int Active = Animator.StringToHash("IsActive");

        [SerializeField]
        private Animator animator;

        public bool IsActive { get; private set; }


        public void Activate()
        {
            if (IsActive)
                return;

            IsActive = true;
            animator.SetBool(Active, true);
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;
            animator.SetBool(Active, false);
        }
    }
}