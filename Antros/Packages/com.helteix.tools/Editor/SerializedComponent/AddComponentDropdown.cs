using System;
using System.Linq;
using System.Reflection;
using Helteix.Tools.SerializedComponent;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Helteix.Tools.Editor.SerializedComponent
{
    public class AddComponentDropdown
    {
        public event Action<Type> OnTypeSelected;

        private GenericMenu menu;

        public AddComponentDropdown(string pathConstraint = null, Type typeConstraint = null, bool showNonCompatibleComponents = false) : base()
        {
            menu = new GenericMenu();

            Type neededType = typeConstraint ?? typeof(ISComponent);

            var types = TypeCache.GetTypesDerivedFrom(typeof(ISComponent));

            foreach (Type type in types)
            {
                if(type.IsAbstract || type.IsInterface)
                    continue;

                if(type.IsSubclassOf(typeof(UnityEngine.Object)))
                    continue;

                string path = $"Others/{type.Name}";
                var attribute = type.GetCustomAttribute<AddSerializedComponentMenuAttribute>();
                if (attribute != null)
                    path = attribute.Path;

                bool valid = true;

                if (!string.IsNullOrEmpty(pathConstraint))
                {
                    // Debug.Log($"{path} with {pathConstraint} => {path.StartsWith(pathConstraint)}");
                    valid &= path.StartsWith(pathConstraint);
                }

                if (neededType.IsClass)
                    valid &= type == neededType || type.IsSubclassOf(neededType);
                if (neededType.IsInterface)
                    valid &= type.GetInterfaces().Any(ctx => ctx == neededType);

                if (valid)
                {
                    if(!string.IsNullOrEmpty(pathConstraint))
                        path = path.Remove(0, pathConstraint.Length + (path.EndsWith('/') ? 0 : 1));

                    menu.AddItem(new GUIContent(path), false, () => { OnTypeSelected?.Invoke(type); });
                }
                else if (showNonCompatibleComponents)
                {
                    menu.AddDisabledItem(new GUIContent(path), false);
                }
            }
        }

        public void Show(EventBase eventBase)
        {
            menu.DropDown(new Rect(eventBase.originalMousePosition, Vector2.zero));
        }
    }
}