using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// A physical-attack cutscene. Its single step, "Hit", marks the moment the blow lands — its
    /// handler runs the damage command. All attack cutscenes share this fixed step (declared once in
    /// code), so authoring is just placing the Hit marker on the timeline. Referenced by a hero card;
    /// a shared default can cover heroes that don't have a custom one.
    /// </summary>
    [CreateAssetMenu(menuName = "ATCG/Cutscenes/Attack Cutscene")]
    public sealed class AttackCutscene : CutsceneDefinition
    {
        public const string HIT = "Hit";

        private static readonly string[] Steps = { HIT };

        // A plain serialized field (not a get-only auto-property) so the inspector shows it as an
        // editable "Director" slot — a get-only auto-property's backing field is readonly and renders
        // greyed out.
        [SerializeField] private PlayableDirector director;
        public override PlayableDirector Director => director;

        public override IReadOnlyList<string> DeclaredSteps => Steps;
    }
}
