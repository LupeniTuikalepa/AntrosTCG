using System;
using System.Collections.Generic;
using System.Reflection;
using ATCG.Battle.Entities.Components;
using UnityEngine.UIElements;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Renders the components of a selected entity as foldouts.
    ///
    /// Expansion state is stored per component type in a caller-owned dictionary and
    /// restored on every rebuild. That is what stops foldouts from snapping back open:
    /// previously, each refresh recreated foldouts with value=true, overriding the user.
    /// Now a closed foldout writes false into the state and the next rebuild honours it.
    /// </summary>
    public sealed class EntityComponentView
    {
        private readonly Dictionary<string, bool> expansion;

        public EntityComponentView(Dictionary<string, bool> expansionState)
        {
            expansion = expansionState;
        }

        public void Populate(VisualElement pane, World world, int entityId)
        {
            pane.Clear();

            if (world == null || entityId < 0)
                return;

            Entity entity = new(entityId);
            if (!world.IsAlive(entity))
            {
                pane.Add(Empty("(entity no longer alive)"));
                return;
            }

            EntityMeta meta;
            try { meta = world.GetMeta(entity); }
            catch { pane.Add(Empty("(meta unavailable)")); return; }

            int shown = 0;
            for (int id = 0; id < ComponentRegistry.MaxComponents; id++)
            {
                Type type = ComponentRegistry.GetTypeForComponentID(id);
                if (type == null || !meta.HasComponent(id))
                    continue;

                IComponentStore store;
                try { store = world.GetStore(id); }
                catch { continue; }
                if (store == null)
                    continue;

                object boxed;
                try { boxed = store.GetBoxed(entityId); }
                catch { continue; }
                if (boxed == null)
                    continue;

                pane.Add(BuildFoldout(store.ComponentType, boxed));
                shown++;
            }

            if (shown == 0)
                pane.Add(Empty("(no components)"));
        }

        private VisualElement BuildFoldout(Type type, object value)
        {
            string key = type.FullName ?? type.Name;
            bool open = !expansion.TryGetValue(key, out bool v) || v;

            Foldout foldout = new() { text = type.Name, value = open };
            foldout.AddToClassList("wi-component");
            foldout.RegisterValueChangedCallback(evt =>
            {
                // The Foldout's own toggle bubbles a ChangeEvent<bool>; nested toggles
                // (if any) would too, so guard by reading the foldout's resolved value
                // only when the event originates from its header toggle.
                if (evt.currentTarget == foldout)
                    expansion[key] = foldout.value;
            });

            FieldInfo[] fields = type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (fields.Length == 0)
            {
                Label tag = new("(tag \u2014 no fields)");
                tag.AddToClassList("wi-tag-note");
                foldout.Add(tag);
                return foldout;
            }

            foreach (FieldInfo field in fields)
            {
                object fieldValue;
                try { fieldValue = field.GetValue(value); }
                catch (Exception e) { fieldValue = $"<err: {e.Message}>"; }

                VisualElement row = new();
                row.AddToClassList("wi-field");

                Label name = new(CleanName(field.Name));
                name.AddToClassList("wi-field__name");
                row.Add(name);

                Label val = new(Stringify(fieldValue));
                val.AddToClassList("wi-field__value");
                row.Add(val);

                foldout.Add(row);
            }

            return foldout;
        }

        private static Label Empty(string text)
        {
            Label l = new(text);
            l.AddToClassList("wi-empty");
            return l;
        }

        private static string CleanName(string raw)
        {
            if (raw.Length > 0 && raw[0] == '<')
            {
                int end = raw.IndexOf('>');
                if (end > 1)
                    return raw.Substring(1, end - 1);
            }
            return raw;
        }

        private static string Stringify(object v)
        {
            if (v == null) return "null";
            switch (v)
            {
                case string s: return $"\"{s}\"";
                case UnityEngine.Object uo: return uo == null ? "null (UnityObject)" : uo.name;
                default: return v.ToString();
            }
        }
    }
}