using ATCG.Capacities.Attributs;
using UnityEngine;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// A card-deployment cutscene: the animation played when a card is deployed onto the board. Its
    /// single step "Deployed" — declared with [WithStep] and generated into a const + DeclaredSteps
    /// override — marks the frame the unit arrives on the grid (its handler runs the deployment).
    /// </summary>
    [CreateAssetMenu(menuName = "ATCG/Cutscenes/Deploy Cutscene")]
    [WithStep("Deployed")]
    public sealed partial class DeployCutscene : CutsceneDefinition
    {
    }
}
