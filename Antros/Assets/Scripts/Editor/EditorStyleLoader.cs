using UnityEditor;
using UnityEngine.UIElements;

namespace ATCG.Editor
{
    /// <summary>
    /// Loads a USS StyleSheet by file name and attaches it to a root element. Unlike a
    /// silent lookup, this logs a clear warning when a sheet can't be found — a missing
    /// EditorTheme.uss (which defines every --atcg-* token) otherwise makes all windows
    /// render unstyled with no error at all, which is painful to diagnose.
    /// </summary>
    public static class EditorStyleLoader
    {
        public static void Load(VisualElement root, string ussFileName)
        {
            string nameNoExt = System.IO.Path.GetFileNameWithoutExtension(ussFileName);
            foreach (string guid in AssetDatabase.FindAssets($"{nameNoExt} t:StyleSheet"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(ussFileName))
                    continue;
                StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (sheet != null)
                {
                    root.styleSheets.Add(sheet);
                    return;
                }
            }

            UnityEngine.Debug.LogWarning(
                $"[Antros Editor] Stylesheet '{ussFileName}' not found in the project. " +
                $"The UI will render unstyled. Make sure it is imported as a StyleSheet asset. " +
                $"(EditorTheme.uss defines the --atcg-* tokens every other sheet relies on.)");
        }
    }
}
