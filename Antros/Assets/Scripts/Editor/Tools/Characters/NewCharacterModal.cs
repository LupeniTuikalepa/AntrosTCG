using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.Characters
{
    /// <summary>
    /// Centered modal to create a new character: pick a parent folder and a name, then on Create it
    /// makes a folder named after the character, duplicates the template .sk (set in the Characters
    /// Settings tab) into it, and selects + pings the new asset.
    /// </summary>
    public sealed class NewCharacterModal : EditorWindow
    {
        private const string DefaultFolder = "Assets/Project/Characters";

        private Action onCreated;
        private DefaultAsset parentFolder;
        private string characterName = string.Empty;

        public static void Open(Action onCreated)
        {
            NewCharacterModal window = CreateInstance<NewCharacterModal>();
            window.onCreated = onCreated;
            window.titleContent = new GUIContent("New Character");

            Vector2 size = new(430, 200);
            window.minSize = size;

            Rect main = EditorGUIUtility.GetMainWindowPosition();
            window.position = new Rect(main.center - size / 2f, size);
            window.ShowModalUtility();
        }

        private void CreateGUI()
        {
            CharacterToolSettings settings = CharacterToolSettings.GetOrCreate();
            DefaultAsset explore = settings.ExploreFolder;
            parentFolder = explore != null ? explore : AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultFolder);

            VisualElement root = rootVisualElement;
            root.style.paddingTop = root.style.paddingBottom = 10;
            root.style.paddingLeft = root.style.paddingRight = 10;

            Label title = new("Create a new character")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 8 }
            };
            root.Add(title);

            ObjectField folderField = new("Path")
            {
                objectType = typeof(DefaultAsset),
                value = parentFolder,
                tooltip = "Parent folder. A subfolder named after the character is created inside it."
            };
            folderField.RegisterValueChangedCallback(e => parentFolder = e.newValue as DefaultAsset);
            root.Add(folderField);

            TextField nameField = new("Name") { value = characterName };
            nameField.RegisterValueChangedCallback(e => characterName = e.newValue);
            root.Add(nameField);

            Label error = new()
            {
                style = { color = new Color(0.9f, 0.4f, 0.4f), whiteSpace = WhiteSpace.Normal, marginTop = 6, display = DisplayStyle.None }
            };
            root.Add(error);

            VisualElement buttons = new()
            {
                style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginTop = 10 }
            };
            buttons.Add(new Button(Close) { text = "Cancel" });
            buttons.Add(new Button(() => Create(error)) { text = "Create" });
            root.Add(buttons);

            nameField.Focus();
        }

        private void Create(Label error)
        {
            string name = (characterName ?? string.Empty).Trim();
            string parentPath = parentFolder != null ? AssetDatabase.GetAssetPath(parentFolder) : DefaultFolder;

            if (string.IsNullOrEmpty(name))
            {
                Fail(error, "Enter a name.");
                return;
            }
            if (string.IsNullOrEmpty(parentPath) || !AssetDatabase.IsValidFolder(parentPath))
            {
                Fail(error, "Pick a valid folder.");
                return;
            }

            string templatePath = CharacterToolSettings.GetOrCreate().CharacterTemplate is { } tpl
                ? AssetDatabase.GetAssetPath(tpl)
                : null;
            if (string.IsNullOrEmpty(templatePath) || !File.Exists(templatePath))
            {
                Fail(error, "Set a character template in the Settings tab first.");
                return;
            }

            string folder = $"{parentPath}/{name}";
            if (AssetDatabase.IsValidFolder(folder))
            {
                Fail(error, $"A folder '{name}' already exists here.");
                return;
            }

            AssetDatabase.CreateFolder(parentPath, name);

            string newPath = $"{folder}/{name}{Path.GetExtension(templatePath)}";
            if (!AssetDatabase.CopyAsset(templatePath, newPath))
            {
                Fail(error, "Couldn't duplicate the template character.");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Object created = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(newPath);
            Selection.activeObject = created;
            EditorGUIUtility.PingObject(created);

            onCreated?.Invoke();
            Close();
        }

        private static void Fail(Label error, string message)
        {
            error.text = message;
            error.style.display = DisplayStyle.Flex;
        }
    }
}
