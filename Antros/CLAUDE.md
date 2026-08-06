# Project rules — Antros / ATCG

## Editor UI
- **UI Toolkit only.** All editor UI (custom inspectors, editor windows, overlays, property
  drawers) must be built with UI Toolkit (`CreateInspectorGUI`, `VisualElement`, `UxmlElement`).
  **Never IMGUI** — no `OnInspectorGUI`, `EditorGUILayout`, `EditorGUI`, `OnGUI`.
- Styling via **external USS only** — no inline styles.
