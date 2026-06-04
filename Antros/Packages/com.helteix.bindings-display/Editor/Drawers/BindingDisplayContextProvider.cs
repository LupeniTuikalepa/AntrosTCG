using System.Collections.Generic;
using Helteix.ControlDisplay.Data;
using UnityEditor;

namespace Helteix.ControlDisplay.Editor.Editor.Editor.Drawers
{
    public class BindingDisplayContextProvider
    {
        private static readonly Dictionary<SerializedObject, BindingDisplayContextProvider> contextProviders = new ();

        public BindingDisplaySettings Settings => source.targetObject as BindingDisplaySettings;
        public int SpriteVariantCount => Settings.SpriteVariantsCount;

        private readonly SerializedObject source;

        public static BindingDisplayContextProvider AddContextProvider(SerializedObject source)
        {
            BindingDisplayContextProvider provider = new BindingDisplayContextProvider(source);

            contextProviders[source] = provider;
            return provider;
        }

        public static void RemoveContext(SerializedObject source)
        {
            contextProviders.Remove(source);
        }

        public static bool TryGetContext(SerializedObject source, out BindingDisplayContextProvider provider)
        {
            return contextProviders.TryGetValue(source, out provider);
        }


        private BindingDisplayContextProvider(SerializedObject source)
        {
            this.source = source;
        }

    }
}