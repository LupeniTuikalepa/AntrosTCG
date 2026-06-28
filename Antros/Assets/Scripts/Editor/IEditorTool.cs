using UnityEngine.UIElements;

namespace ATCG.Editor
{
    /// <summary>
    /// A tool hosted by the Antros TCG Editor hub. Implement this and the hub will
    /// discover it automatically (reflection over all loaded types) and add a button
    /// for it in the left rail — no hub code to touch when adding a new tool.
    ///
    /// Lifecycle:
    ///   BuildUI()       — called once, returns the tool's root element (cached by the hub).
    ///   OnActivated()   — the tool became the visible one; subscribe to events here.
    ///   OnDeactivated() — another tool was selected; unsubscribe / pause work here.
    /// Keeping subscriptions in Activated/Deactivated (not in BuildUI) means inactive
    /// tools cost nothing — no timers, no event handling running in the background.
    /// </summary>
    public interface IEditorTool
    {
        /// <summary>Display name shown in the rail button and header.</summary>
        string DisplayName { get; }

        /// <summary>
        /// Short glyph (1–2 chars) shown in the rail badge — a letter or unicode symbol.
        /// Keeps icons asset-free; the badge is styled in USS.
        /// </summary>
        string Icon { get; }

        /// <summary>Sort order in the rail (lower = higher up). Ties break by name.</summary>
        int Order { get; }

        /// <summary>Build and return the tool's root UI. Called once; result is cached.</summary>
        VisualElement BuildUI();

        /// <summary>The tool became visible.</summary>
        void OnActivated();

        /// <summary>The tool was hidden (another tool selected, or window closed).</summary>
        void OnDeactivated();
    }
}
