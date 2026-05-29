using System.IO;
using Helteix.Tools.Editor.Serialisation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Helteix.Tools.Editor.Settings.Drawers
{
    [CustomEditor(typeof(GameSettingsEditorPrefs))]
    public class GameSettingsEditorPrefsEditor : UnityEditor.Editor
    {


        public override VisualElement CreateInspectorGUI()
        {
            VisualElement container = new VisualElement()
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Column,
                }
            };

            VisualElement pathRow = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 0,
                    flexShrink = 1,
                    height = EditorGUIUtility.singleLineHeight * 1.2f,
                }
            };

            TextField textField = new TextField("Settings path")
            {
                style =
                {
                    flexGrow = 1,
                },

            };;
            textField.BindProperty(serializedObject.FindBackingFieldProperty(nameof(GameSettingsEditorPrefs.DefaultSettingsPath)));
            textField.SetEnabled(false);

            GUIContent icon = EditorGUIUtility.IconContent("d_FolderOpened Icon");
            Button button = new Button(Background.FromTexture2D(icon.image as Texture2D), () =>
            {
                string fromPath = GameSettingsEditorPrefs.instance.DefaultSettingsPath;
                string fullPath = Path.Combine(Application.dataPath, fromPath);
                var path = EditorUtility.OpenFolderPanel(
                    "Choose a folder to save by default all created settings",
                    fullPath,
                    string.Empty);

                if(string.IsNullOrEmpty(path))
                    return;

                if (!path.Contains(Application.dataPath))
                {
                    Debug.LogError("Settings can only be saved inside the Assets folder");
                    return;
                }

                if (path == Application.dataPath)
                    GameSettingsEditorPrefs.instance.DefaultSettingsPath = string.Empty;
                else
                {
                    string result = path.Remove(0, Application.dataPath.Length + 1);
                    GameSettingsEditorPrefs.instance.DefaultSettingsPath = result;
                }

                GameSettingsEditorPrefs.instance.Save();

            })
            {
                style =
                {
                    width = 100
                }
            };

            pathRow.Add(button);
            pathRow.Add(textField);

            container.Add(pathRow);
            return container;
        }
    }
}