using System;
using ATCG.Cutscenes;
using UnityEditor;
using UnityEngine;

namespace ATCG.Editor.Tools.CutsceneEditor
{
    /// <summary>
    /// Creates a new cutscene definition asset of a chosen kind: makes the folder, instantiates the
    /// ScriptableObject, and scaffolds its stage (timeline + director prefab variant) via
    /// <see cref="CutsceneAssetBuilder"/>. Single-phase — unlike capacities there is no source-gen, so
    /// the concrete type already exists and no recompile is needed.
    /// </summary>
    public static class CutsceneCreator
    {
        private const string Root = "Assets/Project/Cutscenes";

        public static bool Create(Type type, string rawName, out CutsceneDefinition created, out string message)
        {
            created = null;

            string name = (rawName ?? string.Empty).Trim();
            if (!IsValidName(name))
            {
                message = "Name must be letters, digits or underscore, and not start with a digit.";
                return false;
            }

            if (type == null || type.IsAbstract || !typeof(CutsceneDefinition).IsAssignableFrom(type))
            {
                message = "Pick a valid cutscene type.";
                return false;
            }

            string folder = EnsureFolder($"{Root}/{CategoryFolder(type)}/{name}");
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}.asset");

            CutsceneDefinition definition = (CutsceneDefinition)ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(definition, assetPath);

            // Persist + reload so we operate on a stable, imported instance before the builder's own
            // Refresh (otherwise the director assignment can be silently dropped on a stale reference).
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            definition = AssetDatabase.LoadAssetAtPath<CutsceneDefinition>(assetPath);
            if (definition == null)
            {
                message = $"The created asset couldn't be reloaded at {assetPath}.";
                return false;
            }

            if (!CutsceneAssetBuilder.TryBuild(definition, folder, name, out string buildMessage))
                Debug.LogWarning($"[CutsceneEditor] Stage not fully built for '{name}': {buildMessage}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            created = AssetDatabase.LoadAssetAtPath<CutsceneDefinition>(assetPath);
            EditorGUIUtility.PingObject(created);
            Debug.Log($"[CutsceneEditor] Created cutscene '{name}' ({type.Name}).");
            message = null;
            return true;
        }

        private static bool IsValidName(string s)
        {
            if (string.IsNullOrEmpty(s) || (!char.IsLetter(s[0]) && s[0] != '_'))
                return false;
            for (int i = 1; i < s.Length; i++)
                if (!char.IsLetterOrDigit(s[i]) && s[i] != '_')
                    return false;
            return true;
        }

        // Groups new cutscene assets by broad kind under the Cutscenes root (mirrors the Explore tab).
        private static string CategoryFolder(Type type)
        {
            if (type == typeof(AttackCutscene))
                return "PhysicalAttacks";
            if (type == typeof(DeployCutscene))
                return "Deployments";
            return type.Name;
        }

        private static string EnsureFolder(string path)
        {
            path = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path))
                return path;

            string[] parts = path.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
            return path;
        }
    }
}
