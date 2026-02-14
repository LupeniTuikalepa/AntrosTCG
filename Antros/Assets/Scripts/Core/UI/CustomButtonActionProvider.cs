using UnityEngine;
using UnityEngine.InputSystem;

namespace ATCG.UI
{
    public abstract class CustomButtonActionProvider : MonoBehaviour
    {
        protected internal abstract InputAction GetAction();
    }
}