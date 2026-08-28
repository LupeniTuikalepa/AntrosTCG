using ATCG.Capacities.Attributs;
using UnityEngine;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// A physical-attack cutscene. Its single step "Hit" — declared with [WithStep] and generated into
    /// a const + DeclaredSteps override — marks the moment the blow lands (its handler runs the damage
    /// command). Referenced by a hero card; a shared default can cover heroes without a custom one.
    /// </summary>
    [CreateAssetMenu(menuName = "ATCG/Cutscenes/Attack Cutscene")]
    [WithStep("Hit")]
    public sealed partial class AttackCutscene : CutsceneDefinition
    {
    }
}
