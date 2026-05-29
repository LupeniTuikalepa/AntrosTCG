#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections.Generic;

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
                    if(AssetDatabase.IsSubAsset(obj))
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
        public static void ClearChildren(this Transform transform, params Transform[] ignore)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                ClearEditor(transform, ignore);
            else
#endif
                ClearRuntime(transform, ignore);
        }

#if UNITY_EDITOR
        private static void ClearEditor(this Transform transform, params Transform[] ignore)
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                bool valid = true;
                for (int j = 0; j < ignore.Length; j++)
                {
                    if (ignore[j] == child)
                    {
                        valid = false;
                        break;
                    }
                }

                if (!valid)
                {
                    continue;
                }

                objectsToDestroy.Add(child.gameObject);
            }
        }
#endif

        private static void ClearRuntime(this Transform transform, params Transform[] ignore)
        {
            foreach (Transform child in transform)
            {
                bool valid = true;
                for (int j = 0; j < ignore.Length; j++)
                {
                    if (ignore[j] == child)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    child.DestroyGameObject();
            }
        }


        public static void Activate(this GameObject unityObject) => unityObject.SetActive(true);
        public static void Deactivate(this GameObject unityObject) => unityObject.SetActive(false);

        public static void DestroyGameObject(this Component component) => component.gameObject.Destroy();
        public static void Destroy<T> (this T unityObject) where T : Object
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