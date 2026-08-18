using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// A card-deployment cutscene: the animation played when a card is deployed onto the board. Its
    /// single step, "Deployed", marks the frame the unit actually arrives on the grid — its handler
    /// runs the deployment resolution (spawn / reveal), just like an attack's "Hit" marks the frame
    /// the blow lands. All deploy cutscenes share this fixed step (declared once in code), so authoring
    /// is just placing the Deployed marker on the timeline. The director is serialized on the base
    /// CutsceneDefinition.
    /// </summary>
    [CreateAssetMenu(menuName = "ATCG/Cutscenes/Deploy Cutscene")]
    public sealed class DeployCutscene : CutsceneDefinition
    {
        public const string DEPLOYED = "Deployed";

        private static readonly string[] Steps = { DEPLOYED };

        public override IReadOnlyList<string> DeclaredSteps => Steps;
    }
}