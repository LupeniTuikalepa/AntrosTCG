namespace ATCG.Battle
{
    public interface IInteractionController
    {
        int MaxSelectables { get; }

        bool Validate(PlayerInteractable interactable);

        void OnSelected(PlayerInteractable playerInteractable);
        void OnUnselected(PlayerInteractable playerInteractable);

        void OnPointerEnter(PlayerInteractable playerInteractable);
        void OnPointerExit(PlayerInteractable playerInteractable);
    }
}