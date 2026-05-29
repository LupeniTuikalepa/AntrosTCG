using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Helteix.Tools.Editor.Common
{
    public class FolderPropertyElement : VisualElement
    {
        private readonly string panelTitle;
        private readonly SerializedProperty property;

        public FolderPropertyElement(string panelTitle, SerializedProperty property)
        {
            this.panelTitle = panelTitle;
            this.property = property;
            VisualElement choosePathContainer = new VisualElement()
            {
                name = "Choose path container",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = EditorGUIUtility.singleLineHeight,
                }
            };

            Background background = Background.FromTexture2D(EditorGUIUtility.IconContent("d_FolderOpened Icon").image as Texture2D);


            Button openFolder = new Button(background, ClickEvent)
            {
                name =  "Button",
                style =
                {
                    width = 70,
                    marginLeft = 15,
                    marginRight = 15,
                }
            };

            Label propertyName = new Label(property.displayName)
            {
                style =
                {
                    paddingRight = 10,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    textOverflow = TextOverflow.Clip,
                }
            };
            TextField textField = new TextField()
            {
                style =
                {
                    flexGrow = 1,
                }
            };
            textField.BindProperty(property);
            
            textField.SetEnabled(false);
            
            choosePathContainer.Add(propertyName);
            choosePathContainer.Add(textField);
            choosePathContainer.Add(openFolder);
            
            Add(choosePathContainer);
        }
        
        
        void ClickEvent()
        {
            string newPath = EditorUtility.OpenFolderPanel(panelTitle, "Assets", null);
            string projectPath = Application.dataPath;
            if (!newPath.Contains(projectPath))
            {
                Debug.Log($"The chosen path as to be inside the project folder");
                return;
            }

            int size = projectPath.Length;
            string localPath = newPath.Remove(0, size);
            string finalPath = Path.Join("Assets", localPath);
            
            property.stringValue = finalPath;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}