using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATCG.Capacities;
using ATCG.Enums;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Scaffolds a brand-new capacity from a name + element. Because the CapacityData asset is an
    /// instance of a type we're generating in the same operation, it runs in two phases:
    ///   1) BeginCreate — writes the logic + data scripts, stores the pending request, triggers a
    ///      recompile.
    ///   2) after the reload — creates the folder, the CapacityData asset (with name/element set),
    ///      the director prefab variant + timeline (via CapacityStageBuilder), and copies the UMotion
    ///      template.
    /// </summary>
    public static class CapacityGenerator
    {
        private const string PendingNameKey = "ATCG.CapacityGenerator.PendingName";
        private const string PendingElementKey = "ATCG.CapacityGenerator.PendingElement";

        private const string LogicRoot = "Assets/Scripts/Battle/CapacitySystem/Capacities";
        private const string DataRoot = "Assets/Scripts/Core/Capacities/Data";
        private const string DataAssetRoot = "Assets/Resources/Database/Capacities";

        public static bool BeginCreate(string capacityName, Element element, IReadOnlyList<string> steps, out string error)
        {
            capacityName = (capacityName ?? string.Empty).Trim();
            if (!IsValidIdentifier(capacityName))
            {
                error = "Name must be a valid C# identifier (letters/digits/_ , not starting with a digit).";
                return false;
            }

            List<string> cleanSteps = new();
            if (steps != null)
            {
                foreach (string raw in steps)
                {
                    string s = (raw ?? string.Empty).Trim();
                    if (s.Length == 0)
                        continue;
                    if (!IsValidIdentifier(s))
                    {
                        error = $"Step '{s}' is not a valid C# identifier.";
                        return false;
                    }
                    if (!cleanSteps.Contains(s))
                        cleanSteps.Add(s);
                }
            }

            // Fall back to a single step named after the capacity when none are declared.
            if (cleanSteps.Count == 0)
                cleanSteps.Add(capacityName);

            string elementName = element.ToString();

            string logicPath = $"{LogicRoot}/{elementName}/{capacityName}.cs";
            string dataPath = $"{DataRoot}/{elementName}/{capacityName}Data.cs";

            if (File.Exists(logicPath) || File.Exists(dataPath))
            {
                error = $"A capacity script named '{capacityName}' already exists for {elementName}.";
                return false;
            }

            EnsureFolder(Path.GetDirectoryName(logicPath));
            EnsureFolder(Path.GetDirectoryName(dataPath));

            File.WriteAllText(logicPath, LogicScript(capacityName, elementName, cleanSteps));
            File.WriteAllText(dataPath, DataScript(capacityName, elementName, cleanSteps));

            SessionState.SetString(PendingNameKey, capacityName);
            SessionState.SetString(PendingElementKey, elementName);

            AssetDatabase.Refresh();
            error = null;
            return true;
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            string name = SessionState.GetString(PendingNameKey, null);
            if (string.IsNullOrEmpty(name))
                return;

            // Defer so the freshly compiled types are fully available and the asset db is settled.
            EditorApplication.delayCall += FinishPending;
        }

        private static void FinishPending()
        {
            string name = SessionState.GetString(PendingNameKey, null);
            string elementName = SessionState.GetString(PendingElementKey, null);
            SessionState.EraseString(PendingNameKey);
            SessionState.EraseString(PendingElementKey);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(elementName))
                return;

            Type dataType = TypeCache.GetTypesDerivedFrom<CapacityData>()
                .FirstOrDefault(t => t.Name == name + "Data" && !t.IsAbstract);

            if (dataType == null)
            {
                Debug.LogError($"[CapacityGenerator] Couldn't find the generated type '{name}Data' after compile. " +
                               "Fix any compile error and delete the half-made scripts if needed.");
                return;
            }

            // 1. CapacityData asset with name + element.
            CapacityData data = (CapacityData)ScriptableObject.CreateInstance(dataType);
            EnsureFolder($"{DataAssetRoot}/{elementName}");
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{DataAssetRoot}/{elementName}/{name}.asset");
            AssetDatabase.CreateAsset(data, assetPath);

            SerializedObject so = new(data);
            SetString(so, "<Name>k__BackingField", name);
            SetEnum(so, "<Element>k__BackingField", elementName);
            so.ApplyModifiedProperties();

            // Persist + reload so we operate on a stable, imported instance. Otherwise CreateAsset (and
            // the builder's internal Refresh) can leave `data` as a stale reference, and the director
            // assignment made afterwards is silently dropped.
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            data = AssetDatabase.LoadAssetAtPath<CapacityData>(assetPath);
            if (data == null)
            {
                Debug.LogError($"[CapacityGenerator] The created capacity asset couldn't be reloaded at {assetPath}.");
                return;
            }

            // 2. Director prefab variant + timeline (reuses the existing builder, also wires
            //    CutsceneDirector back onto the data).
            if (!CapacityStageBuilder.TryBuild(data, out string message))
                Debug.LogWarning($"[CapacityGenerator] Cutscene stage not built: {message}");

            // 3. UMotion template copy.
            CopyUmotionTemplate(data, name);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Robustly (re)wire the director prefab + timeline onto the freshly reloaded data.
            // TryBuild's own assignment doesn't reliably persist through this generation flow.
            WireDirectorAndTimeline(assetPath);

            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<CapacityData>(assetPath));
            Debug.Log($"[CapacityGenerator] Created capacity '{name}' ({elementName}).");
        }

        // Fully self-contained wiring: finds the director prefab + timeline in the capacity folder,
        // assigns the timeline to the prefab's PlayableDirector, and assigns that director back onto
        // the CapacityData. Reloads everything from disk so nothing depends on stale in-memory state.
        private static void WireDirectorAndTimeline(string assetPath)
        {
            CapacityData data = AssetDatabase.LoadAssetAtPath<CapacityData>(assetPath);
            if (data == null)
                return;

            string folder = CapacityAssetLayout.EnsureCapacityFolder(data);

            GameObject prefab = FindFirst<GameObject>(folder, go => go.GetComponentInChildren<PlayableDirector>() != null);
            if (prefab == null)
                return;

            TimelineAsset timeline = FindFirst<TimelineAsset>(folder, _ => true);
            string prefabPath = AssetDatabase.GetAssetPath(prefab);

            // Assign the timeline onto the prefab's director (edit prefab contents so it persists).
            if (timeline != null)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    PlayableDirector director = root.GetComponentInChildren<PlayableDirector>();
                    if (director != null && director.playableAsset != timeline)
                    {
                        director.playableAsset = timeline;
                        PrefabUtility.SavePrefabAsset(root);
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }

                AssetDatabase.SaveAssets();
            }

            // Assign the (reloaded) director back onto the data.
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            PlayableDirector assetDirector = prefab != null ? prefab.GetComponentInChildren<PlayableDirector>() : null;
            if (assetDirector == null)
                return;

            SerializedObject so = new(data);
            SerializedProperty prop = so.FindProperty("<CutsceneDirector>k__BackingField");
            if (prop != null)
            {
                prop.objectReferenceValue = assetDirector;
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        private static T FindFirst<T>(string folder, System.Func<T, bool> match) where T : UnityEngine.Object
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder }))
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null && match(asset))
                    return asset;
            }
            return null;
        }

        private static void CopyUmotionTemplate(CapacityData data, string name)
        {
            CapacityEditorSettings settings = CapacityEditorSettings.GetOrCreate();
            if (settings.umotionTemplate == null)
                return;

            string src = AssetDatabase.GetAssetPath(settings.umotionTemplate);
            if (string.IsNullOrEmpty(src))
                return;

            string folder = CapacityAssetLayout.EnsureCapacityFolder(data);
            string dest = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}Motion{Path.GetExtension(src)}");
            AssetDatabase.CopyAsset(src, dest);
        }

        private static string LogicScript(string name, string element, IReadOnlyList<string> steps)
        {
            string stepMethods = string.Join("\n\n", steps.Select(step =>
$@"        // Step wired by [WithStep(""{step}"")] on {name}Data.
        private partial void Execute{step}({name}Data data, CapacityStepContext ctx)
            => throw new NotImplementedException();"));

            return
$@"using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.Capacities.Data.{element};
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities.{element}
{{
    public partial struct {name} : ICapacity<{name}Data>
    {{
        // Valid default: tags the cell as Cell and every member on it as Member.
        public void GetTargets({name}Data data, BattleCellAspect battleCell, CapacityTargets output)
        {{
            output.Add(battleCell.EntityAddress, CapacityTags.CELL);
            foreach (var member in battleCell.GetMembers())
                output.Add(member.EntityAddress, CapacityTags.MEMBER);
        }}

        public void GetHitPattern({name}Data data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
            => throw new NotImplementedException();

{stepMethods}
    }}
}}
";
        }

        private static string DataScript(string name, string element, IReadOnlyList<string> steps)
        {
            string attributes = string.Join("\n    ", steps.Select(step => $@"[WithStep(""{step}"")]"));

            return
$@"using ATCG.Capacities.Attributs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.{element}
{{
    [CreateAssetMenu(menuName = ""ATCG/Capacities/{element}/{name}"")]
    {attributes}
    public partial class {name}Data : CapacityData
    {{
    }}
}}
";
        }

        private static void SetString(SerializedObject so, string field, string value)
        {
            SerializedProperty p = so.FindProperty(field);
            if (p != null)
                p.stringValue = value;
        }

        private static void SetEnum(SerializedObject so, string field, string enumName)
        {
            SerializedProperty p = so.FindProperty(field);
            if (p == null)
                return;

            int index = Array.IndexOf(p.enumNames, enumName);
            if (index >= 0)
                p.enumValueIndex = index;
        }

        private static bool IsValidIdentifier(string s)
        {
            if (string.IsNullOrEmpty(s) || (!char.IsLetter(s[0]) && s[0] != '_'))
                return false;
            for (int i = 1; i < s.Length; i++)
                if (!char.IsLetterOrDigit(s[i]) && s[i] != '_')
                    return false;
            return true;
        }

        private static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path))
                return;

            string[] parts = path.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
