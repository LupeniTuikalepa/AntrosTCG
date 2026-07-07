using ATCG.Battle.CapacitySystem.Core.Properties;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements
{
    public interface ICapacityCutsceneElement
    {
        // Connect binds the element to its driving context (game phase or editor
        // preview). Elements pull whatever refs/properties they need from it — caster,
        // screen player, capacity data — instead of depending on the concrete phase.
        void Connect(ICapacityContext context);

        void Disconnect(ICapacityContext context);
    }
}