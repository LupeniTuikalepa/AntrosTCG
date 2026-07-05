using ATCG.Battle.Players.Local.Phases;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI.Inspector
{
    public abstract class EntityInspectorTabElement : MonoBehaviour
    {
        public abstract bool Connect(InspectEntityPhase phase);
        public abstract void Disconnect(InspectEntityPhase phase);
    }
}