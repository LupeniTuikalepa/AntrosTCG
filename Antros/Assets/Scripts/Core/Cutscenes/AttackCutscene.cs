using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// A physical-attack cutscene. Its single step, "Hit", marks the moment the blow lands — its
    /// handler runs the damage command. All attack cutscenes share this fixed step (declared once in
    /// code), so authoring is just placing the Hit marker on the timeline. Referenced by a hero card;
    /// a shared default can cover heroes that don't have a custom one. The director is serialized on
    /// the base CutsceneDefinition.
    /// </summary>
    [CreateAssetMenu(menuName = "ATCG/Cutscenes/Attack Cutscene")]
    public sealed class AttackCutscene : CutsceneDefinition
    {
        public const string HIT = "Hit";

        private static readonly string[] Steps = { HIT };

        public override IReadOnlyList<string> DeclaredSteps => Steps;
    }
}
