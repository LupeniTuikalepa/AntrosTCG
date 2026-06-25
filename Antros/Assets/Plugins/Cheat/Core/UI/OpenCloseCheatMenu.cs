using Cheats.Core.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cheats.Samples
{
    public class OpenCloseCheatMenu : MonoBehaviour
    {
	    [SerializeField] private Canvas cheatMenu;
	    [SerializeField] private CheatsUIController uiController;
        private bool keyOne = false;
        private bool keyTwo = false;

        private void Start()
        {
	        cheatMenu.enabled = false;
        }

        private void Update()
        {
	        if (keyOne && keyTwo)
	        {
		        cheatMenu.enabled = true;
		        uiController.ReloadCheats();
	        }
        }
        public void One(InputAction.CallbackContext ctx)
        {
	        if (ctx.performed)
	        {
		        keyOne = true;
	        }
	        else
	        {
		        keyOne = false;
	        }
        }
        public void Two(InputAction.CallbackContext ctx)
        {
	        if (ctx.performed)
	        {
		        keyTwo = true;
	        }
	        else
	        {
		        keyTwo = false;
	        }
        }

        public void Close(InputAction.CallbackContext ctx)
        {
	        cheatMenu.enabled = false;
	        uiController.cheats.Clear();
	        keyOne = false;
	        keyTwo = false;
        }
    }
}
