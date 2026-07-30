using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ATCG.Battle.Entities;
using ATCG.Debugging.Cheats;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ATCG.Editor.Tools.Cheats
{
    /// <summary>
    /// Builds the editable parameter controls for a cheat by reflecting its
    /// <see cref="CheatParamAttribute"/> / <see cref="CheatTargetAttribute"/> fields, and binds
    /// each control two-ways to the cheat instance so <c>Execute</c> just reads the fields.
    /// Supports a broad set of field types out of the box; unknown types render a hint rather
    /// than throwing, so new cheats never need editor changes.
    /// </summary>
    public static class CheatParamsRenderer
    {
        /// <summary>Returns the parameter block for a cheat, or null when it has no parameters.</summary>
        public static VisualElement Build(ICheat cheat)
        {
            VisualElement container = new VisualElement { style = { marginLeft = 10, marginTop = 2 } };
            bool any = false;

            foreach (FieldInfo field in cheat.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                CheatTargetAttribute target = field.GetCustomAttribute<CheatTargetAttribute>();
                if (target != null)
                {
                    container.Add(BuildTarget(cheat, field, target));
                    any = true;
                    continue;
                }

                CheatParamAttribute param = field.GetCustomAttribute<CheatParamAttribute>();
                if (param == null)
                    continue;

                container.Add(BuildParam(cheat, field, param));
                any = true;
            }

            return any ? container : null;
        }

        private static VisualElement BuildParam(ICheat cheat, FieldInfo field, CheatParamAttribute p)
        {
            Type t = field.FieldType;
            string label = string.IsNullOrEmpty(p.Label) ? ObjectNames.NicifyVariableName(field.Name) : p.Label;
            VisualElement control;

            if (t == typeof(int))
            {
                if (p.HasRange)
                    control = Bind(new SliderInt(label, (int)p.Min, (int)p.Max) { value = (int)field.GetValue(cheat), showInputField = true }, field, cheat);
                else
                    control = Bind(new IntegerField(label) { value = (int)field.GetValue(cheat) }, field, cheat);
            }
            else if (t == typeof(float))
            {
                if (p.HasRange)
                    control = Bind(new Slider(label, (float)p.Min, (float)p.Max) { value = (float)field.GetValue(cheat), showInputField = true }, field, cheat);
                else
                    control = Bind(new FloatField(label) { value = (float)field.GetValue(cheat) }, field, cheat);
            }
            else if (t == typeof(bool))
                control = Bind(new Toggle(label) { value = (bool)field.GetValue(cheat) }, field, cheat);
            else if (t == typeof(string))
                control = Bind(new TextField(label) { value = (string)field.GetValue(cheat) ?? string.Empty }, field, cheat);
            else if (t.IsEnum && t.IsDefined(typeof(FlagsAttribute), false))
                control = Bind(new EnumFlagsField(label, (Enum)field.GetValue(cheat)), field, cheat);
            else if (t.IsEnum)
                control = Bind(new EnumField(label, (Enum)field.GetValue(cheat)), field, cheat);
            else if (t == typeof(Vector2))
                control = Bind(new Vector2Field(label) { value = (Vector2)field.GetValue(cheat) }, field, cheat);
            else if (t == typeof(Vector3))
                control = Bind(new Vector3Field(label) { value = (Vector3)field.GetValue(cheat) }, field, cheat);
            else if (t == typeof(Vector2Int))
                control = Bind(new Vector2IntField(label) { value = (Vector2Int)field.GetValue(cheat) }, field, cheat);
            else if (t == typeof(Vector3Int))
                control = Bind(new Vector3IntField(label) { value = (Vector3Int)field.GetValue(cheat) }, field, cheat);
            else if (t == typeof(Color))
                control = Bind(new ColorField(label) { value = (Color)field.GetValue(cheat) }, field, cheat);
            else if (typeof(Object).IsAssignableFrom(t))
                control = Bind(new ObjectField(label) { objectType = t, allowSceneObjects = true, value = (Object)field.GetValue(cheat) }, field, cheat);
            else
                control = Unsupported($"{label}: unsupported type {t.Name}");

            if (!string.IsNullOrEmpty(p.Tooltip))
                control.tooltip = p.Tooltip;
            return control;
        }

        // Registers the value-changed callback that writes back to the field, and returns the field element.
        private static VisualElement Bind<TValue>(BaseField<TValue> field, FieldInfo target, ICheat cheat)
        {
            field.RegisterValueChangedCallback(e => target.SetValue(cheat, e.newValue));
            return field;
        }

        private static VisualElement BuildTarget(ICheat cheat, FieldInfo field, CheatTargetAttribute t)
        {
            string label = string.IsNullOrEmpty(t.Label) ? ObjectNames.NicifyVariableName(field.Name) : t.Label;

            if (field.FieldType != typeof(EntityAddress))
                return Unsupported($"{label}: [CheatTarget] requires an EntityAddress field");

            List<CheatTargetOption> options = ResolveTargets(cheat, t.CandidatesMethod);
            if (options.Count == 0)
            {
                DropdownField empty = new DropdownField(label, new List<string> { "— no targets —" }, 0);
                empty.SetEnabled(false);
                if (!string.IsNullOrEmpty(t.Tooltip))
                    empty.tooltip = t.Tooltip;
                return empty;
            }

            List<string> labels = options.Select(o => o.Label).ToList();
            EntityAddress current = (EntityAddress)field.GetValue(cheat);
            int idx = options.FindIndex(o => o.Address.Equals(current));
            if (idx < 0)
            {
                idx = 0;
                field.SetValue(cheat, options[0].Address);
            }

            DropdownField dropdown = new DropdownField(label, labels, idx);
            dropdown.RegisterValueChangedCallback(e =>
            {
                int i = labels.IndexOf(e.newValue);
                if (i >= 0)
                    field.SetValue(cheat, options[i].Address);
            });
            if (!string.IsNullOrEmpty(t.Tooltip))
                dropdown.tooltip = t.Tooltip;
            return dropdown;
        }

        private static List<CheatTargetOption> ResolveTargets(ICheat cheat, string method)
        {
            if (string.IsNullOrEmpty(method))
                return new List<CheatTargetOption>();

            MethodInfo info = cheat.GetType().GetMethod(method,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (info == null)
            {
                Debug.LogWarning($"[Cheats] Candidates method '{method}' not found on {cheat.GetType().Name}.");
                return new List<CheatTargetOption>();
            }

            return info.Invoke(cheat, null) is IEnumerable<CheatTargetOption> options
                ? options.ToList()
                : new List<CheatTargetOption>();
        }

        private static VisualElement Unsupported(string message)
            => new Label(message) { style = { color = new Color(0.9f, 0.6f, 0.3f), whiteSpace = WhiteSpace.Normal } };
    }
}
