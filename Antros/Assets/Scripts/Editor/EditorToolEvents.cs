namespace ATCG.Editor
{
    /// <summary>
    /// Cross-tool events carried by the EditorToolBus. Kept deliberately primitive
    /// (ids, not live objects) so tools stay decoupled and events are safe to hold.
    /// </summary>

    /// <summary>An entity was selected somewhere (list, grid). entityId &lt; 0 means cleared.</summary>
    public readonly struct EntitySelectedEvent
    {
        public readonly int EntityId;
        public EntitySelectedEvent(int entityId) => EntityId = entityId;
    }

    /// <summary>A grid cell was selected. Coordinates are the hex axial X/Y.</summary>
    public readonly struct CellSelectedEvent
    {
        public readonly int X;
        public readonly int Y;
        public CellSelectedEvent(int x, int y) { X = x; Y = y; }
    }

    /// <summary>Request that tools focus on a specific entity (e.g. timeline filter).</summary>
    public readonly struct FocusEntityRequest
    {
        public readonly int EntityId;
        public FocusEntityRequest(int entityId) => EntityId = entityId;
    }

    /// <summary>A StepMarker's assigned step changed; prompts a QTE re-scan.</summary>
    public readonly struct StepMarkerChangedEvent { }
}
