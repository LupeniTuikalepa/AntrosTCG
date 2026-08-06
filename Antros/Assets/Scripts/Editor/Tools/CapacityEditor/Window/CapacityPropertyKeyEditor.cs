using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ATCG.Capacities;
using UnityEditor;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Reads and rewrites a capacity's injected-property keys — the
    /// <c>[CapacityPropertyKey] public const string NAME_PROPERTY = "NAME";</c> constants declared
    /// on the runtime logic struct and used with <c>InjectProperty</c> / <c>TryGetProperty</c>.
    /// Same idea as the target-tag editor, but on the logic script (that's where the keys live —
    /// see IceSpear). A key "NAME" is emitted as the const identifier <c>NAME_PROPERTY</c> holding
    /// the string <c>"NAME"</c>, so existing references stay stable as long as they follow the
    /// convention.
    /// </summary>
    public static class CapacityPropertyKeyEditor
    {
        private const string AttributeName = "CapacityPropertyKey";
        private const string AttributeUsing = "using ATCG.Capacities.Attributs;";
        private const string NameSuffix = "_PROPERTY";

        // A key attribute line, e.g. `[CapacityPropertyKey]` (optionally namespace-qualified).
        private static readonly Regex AttrLine = new Regex(
            @"^\s*\[\s*(?:[\w.]+\.)?CapacityPropertyKey(?:Attribute)?\s*\]\s*$",
            RegexOptions.Compiled);

        // A `public const string NAME = "VALUE";` line (VALUE captured).
        private static readonly Regex ConstLine = new Regex(
            @"^\s*public\s+const\s+string\s+\w+\s*=\s*""(?<value>[^""]*)""\s*;\s*$",
            RegexOptions.Compiled);

        /// <summary>The key values declared on the capacity's runtime logic struct.</summary>
        public static List<string> ReadKeys(CapacityData capacity)
        {
            List<string> keys = new();

            string path = CapacityStepEditor.LocateLogicScript(capacity);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return keys;

            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length - 1; i++)
            {
                if (!AttrLine.IsMatch(lines[i]))
                    continue;

                Match m = ConstLine.Match(lines[i + 1]);
                if (m.Success)
                    keys.Add(m.Groups["value"].Value);
            }

            return keys;
        }

        /// <summary>
        /// Replaces the logic struct's property-key consts with exactly <paramref name="finalKeys"/>,
        /// each emitted as <c>[CapacityPropertyKey] public const string KEY_PROPERTY = "KEY";</c> at
        /// the top of the struct, then triggers a recompile.
        /// </summary>
        public static bool Apply(CapacityData capacity, IReadOnlyList<string> finalKeys, out string error)
        {
            error = null;

            string path = CapacityStepEditor.LocateLogicScript(capacity);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                error = "Couldn't locate the capacity's runtime logic script.";
                return false;
            }

            List<string> src = File.ReadAllLines(path).ToList();

            // Drop the existing key declarations (attribute line + the const line under it).
            List<string> lines = new();
            for (int i = 0; i < src.Count; i++)
            {
                if (AttrLine.IsMatch(src[i]))
                {
                    if (i + 1 < src.Count && ConstLine.IsMatch(src[i + 1]))
                        i++;
                    continue;
                }
                lines.Add(src[i]);
            }

            string logicName = LogicName(capacity);
            int structIdx = lines.FindIndex(l => Regex.IsMatch(l, $@"\bstruct\s+{Regex.Escape(logicName)}\b"));
            if (structIdx < 0)
            {
                error = $"Couldn't find struct '{logicName}' in the logic script.";
                return false;
            }

            int braceIdx = lines.FindIndex(structIdx, l => l.Contains("{"));
            if (braceIdx < 0)
            {
                error = "Couldn't find the logic struct body.";
                return false;
            }

            List<string> clean = finalKeys
                .Select(k => (k ?? string.Empty).Trim())
                .Where(k => k.Length > 0)
                .Distinct()
                .ToList();

            if (clean.Count > 0)
            {
                string indent = Regex.Match(lines[structIdx], @"^\s*").Value + "    ";
                List<string> decls = new();
                foreach (string key in clean)
                {
                    decls.Add($"{indent}[{AttributeName}]");
                    decls.Add($"{indent}public const string {key}{NameSuffix} = \"{key}\";");
                }
                lines.InsertRange(braceIdx + 1, decls);

                EnsureUsing(lines);
            }

            File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();
            return true;
        }

        private static void EnsureUsing(List<string> lines)
        {
            if (lines.Any(l => l.TrimStart().StartsWith(AttributeUsing)))
                return;

            int lastUsing = lines.FindLastIndex(l => l.TrimStart().StartsWith("using "));
            if (lastUsing >= 0)
                lines.Insert(lastUsing + 1, AttributeUsing);
            else
                lines.Insert(0, AttributeUsing);
        }

        private static string LogicName(CapacityData capacity)
        {
            string n = capacity.GetType().Name;
            return n.EndsWith("Data") ? n[..^4] : n;
        }
    }
}
