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
                T originalSource = PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj);
                if (originalSource)
                {
                    return PrefabUtility.InstantiatePrefab(obj, parent) as T;
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