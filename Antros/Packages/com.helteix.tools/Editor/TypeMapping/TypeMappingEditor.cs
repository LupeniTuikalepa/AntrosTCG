using System;
using System.Collections.Generic;
using System.IO;
using Helteix.Tools.TypeMapping;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Helteix.Tools.Editor.TypeMapping
{
    public static class TypeMappingEditor
    {
        public static TypeMappingCollection GetOrCreate()
        {
            TypeMappingCollection collection = null;

            var preloaded = PlayerSettings.GetPreloadedAssets();
            for (int i = 0; i < preloaded.Length; i++)
            {
                if (preloaded[i] is TypeMappingCollection c)
                {
                    collection = c;
                    break;
                }
            }

            if (collection != null)
                return collection;

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(TypeMappingCollection)}");
            if (guids.Length == 0)
            {
                collection = ScriptableObject.CreateInstance<TypeMappingCollection>();
                AssetDatabase.CreateAsset(collection, "Assets/TypeMappingCollection.asset");
                AssetDatabase.SaveAssets();
            }
            else
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var asset = AssetDatabase.LoadAssetAtPath<TypeMappingCollection>(path);

                    if (asset != null)
                    {
                        collection = asset;
                        break;
                    }
                }
            }

            using (ListPool<Object>.Get(out var list))
            {
                list.AddRange(preloaded);
                list.Add(collection);

                PlayerSettings.SetPreloadedAssets(list.ToArray());
            }

            return collection;
        }

        [MenuItem("Tools/Helteix/Setup TypeMapping Collection")]
        internal static void ScanProject()
        {
            EditorUtility.DisplayProgressBar("[Type Mapping]", "Scanning project...", 0);
            try
            {
                // Normally, some computation happens here.
                // This example uses Sleep.
                if (!EditorSceneManager.SaveOpenScenes())
                {
                    Debug.LogError("Couldn't save opened scenes. Aborting project scan.");
                    return;
                }

                HashSet<string> list = new HashSet<string>();

                EditorUtility.DisplayProgressBar("[Type Mapping]", "Scanning scenes...", .2f);
                ScanBuildScenes(list);

                EditorUtility.DisplayProgressBar("[Type Mapping]", "Scanning prefabs...", .4f);
                ScanPrefabs(list);

                EditorUtility.DisplayProgressBar("[Type Mapping]", "Scanning scriptable objects...", .6f);
                ScanScriptableObjects(list);

                EditorUtility.DisplayProgressBar("[Type Mapping]", "Scanning preloaded assets...", .8f);
                ScanPreloadedAssets(list);

                EditorUtility.DisplayProgressBar("[Type Mapping]", "Scanning always included classes...", 1);
                ScanAlwaysIncludedClasses(list);

                var collection = GetOrCreate();
                SerializedObject collectionSerializedObject = new SerializedObject(collection);

                SerializedProperty serializedProperty = collectionSerializedObject.FindProperty("entries");
                serializedProperty.arraySize = 0;

                foreach (var guid in list)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                    serializedProperty.InsertArrayElementAtIndex(serializedProperty.arraySize);
                    SerializedProperty element = serializedProperty.GetArrayElementAtIndex(serializedProperty.arraySize - 1);
                    element.boxedValue = new TypeMapEntry()
                    {
                        guid = guid,
                        typeInfos = asset.GetClass().AssemblyQualifiedName
                    };
                }

                collectionSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

        }

        private static void ScanBuildScenes(HashSet<string> list)
        {
            List<string> openedScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                openedScenes.Add(scene.path);
            }

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                var editorBuildSettingsScene = scenes[i];
                if(!editorBuildSettingsScene.enabled)
                    continue;

                var scene = EditorSceneManager.OpenScene(editorBuildSettingsScene.path, OpenSceneMode.Single);

                EditorUtility.DisplayProgressBar("[Type Mapping]", $"Scanning {editorBuildSettingsScene.path} scene..", (float)i / scenes.Length);

                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
                        if (mb != null)
                            Scan(mb, list);
                }
            }

            bool hasLoadedFirst = false;
            foreach (var path in openedScenes)
            {
                EditorSceneManager.OpenScene(path, hasLoadedFirst ? OpenSceneMode.Single : OpenSceneMode.Additive);
                hasLoadedFirst = true;
            }
        }

        private static void ScanPrefabs(HashSet<string> list)
        {
            string[] findAssets = AssetDatabase.FindAssets("t:Prefab");
            for (int i = 0; i < findAssets.Length; i++)
            {
                string guid = findAssets[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (go == null)
                    continue;

                EditorUtility.DisplayProgressBar("[Type Mapping]", $"Scanning {assetPath}...", (float)i / findAssets.Length);

                foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
                    if (mb != null)
                        Scan(mb, list);
            }
        }

        private static void ScanScriptableObjects(HashSet<string> list)
        {
            string[] findAssets = AssetDatabase.FindAssets("t:ScriptableObject");
            for (int i = 0; i < findAssets.Length; i++)
            {
                string guid = findAssets[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                try
                {
                    var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                    if (scriptableObject == null)
                        continue;

                    EditorUtility.DisplayProgressBar("[Type Mapping]", $"Scanning {assetPath}...",
                        (float)i / findAssets.Length);
                    Scan(scriptableObject, list);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private static void ScanPreloadedAssets(HashSet<string> list)
        {
            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            for (int i = 0; i < preloadedAssets.Length; i++)
            {
                EditorUtility.DisplayProgressBar("[Type Mapping]", $"Scanning {preloadedAssets[i].name}...", (float)i / preloadedAssets.Length);
                Scan(preloadedAssets[i], list);
            }
        }

        private static void ScanAlwaysIncludedClasses(HashSet<string> list)
        {
            var typeCollection = TypeCache.GetTypesWithAttribute<AlwaysIncludeInTypeMappingAttribute>();

            string[] findAssets = AssetDatabase.FindAssets("t:MonoScript");
            for (int i = 0; i < findAssets.Length; i++)
            {
                string guid = findAssets[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

                if (monoScript == null)
                    continue;

                EditorUtility.DisplayProgressBar("[Type Mapping]", $"Scanning {assetPath}...", (float)i / findAssets.Length);
                if (typeCollection.Contains(monoScript.GetClass()))
                {
                    list.Add(guid);
                }
            }
        }


        private static void Scan(Object unityObject, HashSet<string> list)
        {
            using SerializedObject so = new SerializedObject(unityObject);

            SerializedProperty sp = so.GetIterator();
            while (sp.NextVisible(true))
            {
                if(sp.propertyType != SerializedPropertyType.Generic)
                    continue;

                if (sp.isArray)
                    continue;

                if(sp.type != nameof(TypeRef))
                    continue;

                if (sp.boxedValue is not TypeRef)
                    continue;

                using SerializedProperty guidProp = sp.FindPropertyRelative("guid");
                if (!string.IsNullOrEmpty(guidProp.stringValue))
                    list.Add(guidProp.stringValue);
            }
        }
    }
}