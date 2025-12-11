using UnityEngine;

namespace ATCG.Battle.Players.Runtime.Local.UI.HUD
{
    public class PlayerManaIcon : MonoBehaviour
    {
        public bool IsActive { get; private set; }

        [SerializeField]
        private Animator animator;


        public void Activate()
        {
            if(IsActive)
                return;

            IsActive = true;
            animator.Play("OnActivate");

        }

        public void Deactivate()
        {
            if(!IsActive)
                return;

            IsActive = false;
            animator.Play("OnDeactivate");
        }
    }
}