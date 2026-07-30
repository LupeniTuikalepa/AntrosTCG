using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.DatabaseBrowser
{
    /// <summary>
    /// Thin IEditorTool wrapper around a <see cref="DatabaseBrowserView{T}"/>. A concrete tool
    /// only declares its asset type, its Resources folder and its rail label/icon; the shared
    /// view provides the searchable, element-filterable list + inspector (same UX as Cards).
    /// </summary>
    public abstract class DatabaseBrowserTool<T> : IEditorTool where T : ScriptableObject
    {
        protected abstract string FolderPath { get; }
        public abstract string DisplayName { get; }
        public abstract string Icon { get; }
        public virtual int Order => 60;

        private DatabaseBrowserView<T> view;

        public VisualElement BuildUI()
        {
            view = new DatabaseBrowserView<T>(FolderPath, DisplayName);
            return view.Build();
        }

        public void OnActivated() => view?.Reload();

        public void OnDeactivated()
        {
        }
    }
}
