using System;
using UnityEngine;

namespace Helteix.Tools.TypeMapping
{
    [System.Serializable]
    public struct TypeRef
    {
        [SerializeField]
        private string guid;

        [SerializeField]
        private bool isNativeType;

        public Type Type
        {
            get
            {
                if(isNativeType)
                    return Type.GetType(guid);

                if (TypeMappingManager.TryGetType(guid, out var t))
                    return t;

                Debug.LogError($"Could not find type mapped with guid {guid}. Returning null.");
                return null;
            }
        }

        public bool IsAssignableFrom(Type type)
        {
            if(Type.IsAssignableFrom(type))
                return true;

            if (type == null || Type == null)
                return false;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == Type)
                return true;

            foreach (var it in type.GetInterfaces())
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == Type)
                    return true;
            }

            Type baseType = type.BaseType;
            return baseType != null && IsAssignableFrom(baseType);
        }

        public bool IsValid => TypeMappingManager.IsValid(guid);

        public object CreateInstance() => CreateInstance<object>();

        public T CreateInstance<T>()
        {
            if (TypeMappingManager.TryGetType(guid, out Type type))
            {
                object instance = Activator.CreateInstance(type);
                return (T)instance;
            }

            return default;
        }

        public static implicit operator Type(TypeRef @ref) => @ref.Type;
    }
}