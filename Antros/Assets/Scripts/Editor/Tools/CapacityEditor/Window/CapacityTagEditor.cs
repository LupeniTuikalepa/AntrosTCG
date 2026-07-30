using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ATCG.Capacities;
using ATCG.Capacities.Attributs;
using UnityEditor;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Reads and rewrites a capacity Data script's target-tag constants. A tag is any
    /// <c>public const string</c> marked with <see cref="CapacityTargetTagAttribute"/>,
    /// emitted as <c>[CapacityTargetTag] public const string NAME = nameof(NAME);</c>. The
    /// attribute is what distinguishes a tag from unrelated consts (e.g. step-name consts).
    /// The base tags CELL/MEMBER live in <see cref="CapacityTags"/> and are not managed here.
    /// </summary>
    public static class CapacityTagEditor
    {
        private const string TagAttributeName = "CapacityTargetTag";
        private const string TagAttributeUsing = "using ATCG.Capacities.Attributs;";

        // A tag attribute line, e.g. `[CapacityTargetTag]` (optionally namespace-qualified).
        private static readonly Regex TagAttrLine = new Regex(
            @"^\s*\[\s*(?:[\w.]+\.)?CapacityTargetTag(?:Attribute)?\s*\]\s*$",
            RegexOptions.Compiled);

        // Any `public const string X = ... ;` line (only consumed right after a tag attribute).
        private static readonly Regex ConstStringLine = new Regex(
            @"^\s*public\s+const\s+string\s+\w+\s*=",
            RegexOptions.Compiled);

        /// <summary>
        /// The tag const names declared on the capacity's own Data type (via reflection,
        /// so only fields actually carrying <see cref="CapacityTargetTagAttribute"/> count).
        /// </summary>
        public static List<string> ReadTags(CapacityData capacity)
        {
            List<string> tags = new();
            if (capacity == null)
                return tags;

            foreach (FieldInfo field in capacity.GetType().GetFields(
                         BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string) &&
                    field.GetCustomAttribute<CapacityTargetTagAttribute>() != null)
                    tags.Add(field.Name);
            }

            return tags;
        }

        /// <summary>
        /// Replaces the Data script's tag consts with exactly <paramref name="finalTags"/>,
        /// each emitted as <c>[CapacityTargetTag] public const string T = nameof(T);</c> at
        /// the top of the class, then triggers a recompile.
        /// </summary>
        public static bool Apply(CapacityData capacity, IReadOnlyList<string> finalTags, out string error)
        {
            error = null;

            string path = CapacityStepEditor.LocateDataScript(capacity);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                error = "Couldn't locate the capacity's Data script.";
                return false;
            }

            List<string> src = File.ReadAllLines(path).ToList();

            // Drop the existing tag declarations (attribute line + the const line under it);
            // unrelated consts (no attribute) are left untouched.
            List<string> lines = new();
            for (int i = 0; i < src.Count; i++)
            {
                if (TagAttrLine.IsMatch(src[i]))
                {
                    if (i + 1 < src.Count && ConstStringLine.IsMatch(src[i + 1]))
                        i++; // also skip the const line
                    continue;
                }
                lines.Add(src[i]);
            }

            string dataTypeName = capacity.GetType().Name;
            int classIdx = lines.FindIndex(l => Regex.IsMatch(l, $@"\bclass\s+{Regex.Escape(dataTypeName)}\b"));
            if (classIdx < 0)
            {
                error = $"Couldn't find class '{dataTypeName}' in the Data script.";
                return false;
            }

            int braceIdx = lines.FindIndex(classIdx, l => l.Contains("{"));
            if (braceIdx < 0)
            {
                error = "Couldn't find the Data class body.";
                return false;
            }

            List<string> clean = finalTags
                .Select(t => (t ?? string.Empty).Trim())
                .Where(t => t.Length > 0)
                .Distinct()
                .ToList();

            if (clean.Count > 0)
            {
                string indent = Regex.Match(lines[classIdx], @"^\s*").Value + "    ";
                List<string> decls = new();
                foreach (string t in clean)
                {
                    decls.Add($"{indent}[{TagAttributeName}]");
                    decls.Add($"{indent}public const string {t} = nameof({t});");
                }
                lines.InsertRange(braceIdx + 1, decls);

                EnsureUsing(lines);
            }

            File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();
            return true;
        }

        // Guarantees the attribute's namespace is imported so the emitted [CapacityTargetTag] compiles.
        private static void EnsureUsing(List<string> lines)
        {
            if (lines.Any(l => l.TrimStart().StartsWith(TagAttributeUsing)))
                return;

            int lastUsing = lines.FindLastIndex(l => l.TrimStart().StartsWith("using "));
            if (lastUsing >= 0)
                lines.Insert(lastUsing + 1, TagAttributeUsing);
            else
                lines.Insert(0, TagAttributeUsing);
        }
    }
}
