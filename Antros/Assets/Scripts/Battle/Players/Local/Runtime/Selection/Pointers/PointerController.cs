using UnityEngine;

namespace ATCG.Battle
{
    public abstract class PointerController : MonoBehaviour
    {
        public abstract PlayerInteractable GetPointerInteractable();
    }
}