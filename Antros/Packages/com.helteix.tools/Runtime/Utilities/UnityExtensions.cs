#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Pool;
#endif

using System;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Helteix.Tools
{
    public static class UnityEngineExtensions
    {

#if UNITY_EDITOR
        private static List<Object> objectsToDestroy;

        [InitializeOnLoadMethod]
        private static void ConnectToEditor()
        {
            objectsToDestroy = new List<Object>();
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (!Application.isPlaying)
            {
                foreach (var obj in objectsToDestroy)
                {
                    if (AssetDatabase.IsSubAsset(obj))
                        continue;

                    if (obj != null)
                        Object.DestroyImmediate(obj);
                }

                objectsToDestroy.Clear();
            }
        }
#endif

        public static T InstantiatePrefab<T>(this T obj, Transform parent = null) where T : Object
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Keep the prefab connection (so edits to the source show live) ONLY when 'obj' is
                // the ROOT of a prefab: InstantiatePrefab always spawns the whole prefab, so for a
                // nested object it would spawn the container instead of the selected object. A
                // nested object or a plain scene object gets a plain copy of exactly itself.
                GameObject go = obj as GameObject ?? (obj as Component)?.gameObject;
                Object assetRoot = null;
                if (go != null)
                {
                    if (PrefabUtility.IsPartOfPrefabAsset(go) && go.transform.parent == null)
                        assetRoot = go;                                              // prefab asset root
                    else if (PrefabUtility.IsAnyPrefabInstanceRoot(go))
                        assetRoot = PrefabUtility.GetCorrespondingObjectFromSource(go); // scene instance root
                }

                if (assetRoot != null)
                {
                    // InstantiatePrefab returns the root GameObject, so 'as T' is null when T is a
                    // component — resolve the matching component off the root in that case.
                    Object instance = PrefabUtility.InstantiatePrefab(assetRoot, parent);
                    if (instance is T typed)
                        return typed;
                    if (instance is GameObject root && obj is Component component)
                        return root.GetComponent(component.GetType()) as T;
                    return instance as T;
                }
            }
#endif
            return Object.Instantiate(obj, parent);

        }

        public static int ClearChildren(this Transform transform, bool detachChildren = true, params Transform[] ignore)
        {
            int destroyed = 0;
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (Array.IndexOf(ignore, child) >= 0)
                    continue;

                if(detachChildren)
                    child.SetParent(null, false);

                if (Application.isPlaying)
                    Object.Destroy(child.gameObject);
                else
                    Object.DestroyImmediate(child.gameObject);
                destroyed++;
            }

            return destroyed;
        }

        public static void Activate(this GameObject unityObject) => unityObject.SetActive(true);
        public static void Deactivate(this GameObject unityObject) => unityObject.SetActive(false);

        public static void DestroyGameObject(this Component component) => component.gameObject.Destroy();

        public static void Destroy<T>(this T unityObject) where T : Object
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                objectsToDestroy.Add(unityObject);
            else
#endif
                Object.Destroy(unityObject);

        }

    }
}