using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Helteix.Tools.Editor
{
    public class CountLinesOfCode : EditorWindow
    {
        private SerializedObject serializedWindow;
        private SerializedProperty foldersToIgnoreProperty;
        private SerializedProperty foldersToLookFor;

        public List<string> foldersToIgnore = new List<string>();
        public List<string> folders;



        [MenuItem("Tools/Count Lines of Code")]
        public static void ShowWindow()
        {
            GetWindow<CountLinesOfCode>("Count Lines of Code");
        }

        [MenuItem("Assets/Count C# lines in folder", priority = 2)]
        public static void CountLineInFolder()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string path = string.IsNullOrEmpty(assetPath)
                ? Application.dataPath
                : assetPath;

            int totalLines = 0;
            int totalScripts = 0;
            CountLinesInFolder(path, ref totalScripts, ref totalLines, new List<string>());

            Debug.Log($"Total lines of code: {totalLines} for {totalScripts} scripts");
        }

        private void OnEnable()
        {
            serializedWindow = new SerializedObject(this);
            foldersToIgnoreProperty = serializedWindow.FindProperty(nameof(foldersToIgnore));
            foldersToLookFor = serializedWindow.FindProperty(nameof(folders));
        }

        private void OnGUI()
        {
            serializedWindow.Update();

            EditorGUILayout.PropertyField(foldersToIgnoreProperty, true);
            EditorGUILayout.PropertyField(foldersToLookFor);

            if (GUILayout.Button("Count Lines of Code"))
            {
                CountLines();
            }

            serializedWindow.ApplyModifiedProperties();
        }

        private void CountLines()
        {
            int totalLines = 0;
            int totalScripts = 0;
            if(folders.Count == 0)
                CountLinesInFolder("Assets", ref totalScripts, ref totalLines, foldersToIgnore);
            else
            {
                foreach (var folderPath in folders)
                    CountLinesInFolder(folderPath, ref totalScripts, ref totalLines, foldersToIgnore);
            }

            Debug.Log($"Total lines of code: {totalLines} for {totalScripts} scripts");
        }

        private static void CountLinesInFolder(string folderPath, ref int totalScripts, ref int totalLines, List<string> foldersToIgnore)
        {
            string assetsPath = string.IsNullOrEmpty(folderPath) ? Application.dataPath : Path.GetFullPath(folderPath);
            string[] allScripts = Directory.GetFiles(assetsPath, "*.cs", SearchOption.AllDirectories);
            List<string> foldersToIgnorePaths = new List<string>();

            foreach (string folder in foldersToIgnore)
            {
                string path = Path.Combine(assetsPath, folder);
                if (Directory.Exists(path))
                {
                    foldersToIgnorePaths.Add(path);
                }
            }


            foreach (string script in allScripts)
            {
                bool ignore = false;

                foreach (string ignorePath in foldersToIgnorePaths)
                {
                    if (script.StartsWith(ignorePath))
                    {
                        ignore = true;
                        break;
                    }
                }

                if (!ignore)
                {
                    totalScripts++;
                    string[] lines = File.ReadAllLines(script);
                    bool isInCommentedSpace = false;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i].Trim();

                        if (isInCommentedSpace && line.Contains("*/"))
                        {
                            isInCommentedSpace = false;
                            continue;
                        }

                        if(isInCommentedSpace)
                            continue;

                        if(string.IsNullOrWhiteSpace(line))
                            continue;

                        if (line.StartsWith("//"))
                            continue;

                        if (line.Contains("/*"))
                        {
                            isInCommentedSpace = true;
                            continue;
                        }

                        totalLines++;
                    }
                }
            }

        }
    }
}