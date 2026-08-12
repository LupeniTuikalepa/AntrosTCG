using System.Collections.Generic;
using ATCG.Cutscenes;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Capacities;
using UnityEditor;
using UnityEngine.Timeline;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Applies step add/rename/remove edits to a capacity by rewriting its Data script's [WithStep]
    /// attributes and its runtime logic script's Execute{Step} partial methods. Non-WithStep
    /// attributes are preserved. Removed methods are either commented out or deleted (caller's choice).
    /// </summary>
    public static class CapacityStepEditor
    {
        public enum StepAction { Ignore, Rename, Remove }

        public struct Edit
        {
            public string original;
            public StepAction action;
            public string newName;
        }

        public static string LocateDataScript(CapacityData capacity)
        {
            MonoScript ms = MonoScript.FromScriptableObject(capacity);
            return ms != null ? AssetDatabase.GetAssetPath(ms) : null;
        }

        public static string LocateLogicScript(CapacityData capacity)
        {
            string logicName = LogicName(capacity);
            foreach (string guid in AssetDatabase.FindAssets($"{logicName} t:MonoScript"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith($"/{logicName}.cs") && path.Contains("CapacitySystem/Capacities"))
                    return path;
            }
            return null;
        }

        public static bool Apply(CapacityData capacity, IReadOnlyList<Edit> edits, IReadOnlyList<string> added,
            bool commentRemoved, out string error)
        {
            error = null;

            string dataPath = LocateDataScript(capacity);
            string logicPath = LocateLogicScript(capacity);

            if (string.IsNullOrEmpty(dataPath))
            {
                error = "Couldn't locate the capacity's Data script.";
                return false;
            }
            if (string.IsNullOrEmpty(logicPath))
            {
                error = "Couldn't locate the capacity's runtime logic script.";
                return false;
            }

            List<string> cleanAdded = (added ?? new List<string>())
                .Select(s => (s ?? string.Empty).Trim())
                .Where(s => s.Length > 0)
                .ToList();

            // Final step set for the Data attributes.
            List<string> finalSteps = new();
            foreach (Edit e in edits)
            {
                switch (e.action)
                {
                    case StepAction.Remove: continue;
                    case StepAction.Rename: finalSteps.Add(e.newName); break;
                    default: finalSteps.Add(e.original); break;
                }
            }
            foreach (string a in cleanAdded)
                if (!finalSteps.Contains(a))
                    finalSteps.Add(a);

            RewriteDataAttributes(dataPath, capacity.GetType().Name, finalSteps);
            RewriteLogicMethods(logicPath, capacity.GetType().Name, LogicName(capacity), edits, cleanAdded, commentRemoved);
            UpdateTimelineMarkers(capacity, edits);

            AssetDatabase.Refresh();
            return true;
        }

        // ---- Timeline: rename/remove StepMarkers ----------------------------

        private static void UpdateTimelineMarkers(CapacityData capacity, IReadOnlyList<Edit> edits)
        {
            TimelineAsset timeline = capacity.CutsceneTimeline;
            if (timeline == null)
                return;

            Dictionary<string, string> renames = edits
                .Where(e => e.action == StepAction.Rename)
                .ToDictionary(e => e.original, e => e.newName);
            HashSet<string> removes = new(edits.Where(e => e.action == StepAction.Remove).Select(e => e.original));

            if (renames.Count == 0 && removes.Count == 0)
                return;

            bool changed = false;

            void Process(TrackAsset track)
            {
                foreach (IMarker im in track.GetMarkers().ToList())
                {
                    if (im is not StepMarker marker)
                        continue;

                    string step = marker.StepName;
                    if (removes.Contains(step))
                    {
                        track.DeleteMarker(marker);
                        changed = true;
                    }
                    else if (renames.TryGetValue(step, out string newName))
                    {
                        SerializedObject so = new(marker);
                        SerializedProperty prop = so.FindProperty("stepName");
                        if (prop != null)
                        {
                            prop.stringValue = newName;
                            so.ApplyModifiedProperties();
                            changed = true;
                        }
                    }
                }
            }

            if (timeline.markerTrack != null)
                Process(timeline.markerTrack);
            foreach (TrackAsset track in timeline.GetOutputTracks())
                Process(track);

            if (changed)
            {
                EditorUtility.SetDirty(timeline);
                AssetDatabase.SaveAssets();
            }
        }

        // ---- Data script: [WithStep] attributes -----------------------------

        private static void RewriteDataAttributes(string path, string dataTypeName, IReadOnlyList<string> steps)
        {
            List<string> lines = File.ReadAllLines(path)
                .Where(l => !l.TrimStart().StartsWith("[WithStep("))
                .ToList();

            int classIdx = lines.FindIndex(l => Regex.IsMatch(l, $@"\bclass\s+{Regex.Escape(dataTypeName)}\b"));
            if (classIdx < 0)
                return;

            string indent = Regex.Match(lines[classIdx], @"^\s*").Value;
            List<string> attrs = steps.Select(s => $"{indent}[WithStep(\"{s}\")]").ToList();
            lines.InsertRange(classIdx, attrs);

            File.WriteAllLines(path, lines);
        }

        // ---- Logic script: Execute{Step} methods ----------------------------

        private static void RewriteLogicMethods(string path, string dataTypeName, string logicName,
            IReadOnlyList<Edit> edits, IReadOnlyList<string> added, bool commentRemoved)
        {
            string text = File.ReadAllText(path);

            // Renames: swap the identifier everywhere it appears.
            foreach (Edit e in edits.Where(e => e.action == StepAction.Rename))
                text = Regex.Replace(text, $@"\bExecute{Regex.Escape(e.original)}\b", $"Execute{e.newName}");

            // Removes: comment out or delete each method span (re-find after each edit).
            foreach (Edit e in edits.Where(e => e.action == StepAction.Remove))
            {
                if (!TryFindMethodSpan(text, $"Execute{e.original}", out int start, out int end))
                    continue;

                string span = text.Substring(start, end - start);
                if (commentRemoved)
                {
                    text = text[..start] + CommentBlock(span) + text[end..];
                }
                else
                {
                    int cut = end;
                    while (cut < text.Length && (text[cut] == '\r' || text[cut] == '\n'))
                        cut++;
                    text = text[..start] + text[cut..];
                }
            }

            // Adds: insert stubs before the struct's closing brace.
            if (added.Count > 0 && TryFindStructClose(text, logicName, out int closeIdx))
            {
                string stubs = string.Concat(added.Select(s =>
$"\n        // Step wired by [WithStep(\"{s}\")] on {dataTypeName}.\n" +
$"        private partial void Execute{s}({dataTypeName} data, CapacityStepContext ctx)\n" +
$"            => throw new System.NotImplementedException();\n"));

                text = text[..closeIdx] + stubs + text[closeIdx..];
            }

            File.WriteAllText(path, text);
        }

        private static string CommentBlock(string span)
        {
            string[] lines = span.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmedEnd = lines[i].TrimEnd('\r');
                if (trimmedEnd.Length == 0)
                    continue;
                string indent = Regex.Match(lines[i], @"^\s*").Value;
                lines[i] = indent + "// " + lines[i].Substring(indent.Length);
            }
            return string.Join("\n", lines);
        }

        private static bool TryFindMethodSpan(string text, string executeName, out int start, out int end)
        {
            start = end = -1;
            Match m = Regex.Match(text, $@"\b{Regex.Escape(executeName)}\s*\(");
            if (!m.Success)
                return false;

            int ls = text.LastIndexOf('\n', m.Index);
            start = ls < 0 ? 0 : ls + 1;

            int parenOpen = text.IndexOf('(', m.Index);
            int parenClose = MatchDelimiter(text, parenOpen, '(', ')');
            if (parenClose < 0)
                return false;

            int i = parenClose + 1;
            while (i < text.Length && char.IsWhiteSpace(text[i]))
                i++;
            if (i >= text.Length)
                return false;

            if (text[i] == '=' && i + 1 < text.Length && text[i + 1] == '>')
            {
                int semi = text.IndexOf(';', i);
                if (semi < 0)
                    return false;
                end = semi + 1;
                return true;
            }

            if (text[i] == '{')
            {
                int close = MatchDelimiter(text, i, '{', '}');
                if (close < 0)
                    return false;
                end = close + 1;
                return true;
            }

            return false;
        }

        private static bool TryFindStructClose(string text, string logicName, out int closeIdx)
        {
            closeIdx = -1;
            Match m = Regex.Match(text, $@"\bstruct\s+{Regex.Escape(logicName)}\b");
            if (!m.Success)
                return false;

            int brace = text.IndexOf('{', m.Index);
            if (brace < 0)
                return false;

            closeIdx = MatchDelimiter(text, brace, '{', '}');
            return closeIdx >= 0;
        }

        private static int MatchDelimiter(string text, int openIdx, char open, char close)
        {
            int depth = 0;
            for (int i = openIdx; i < text.Length; i++)
            {
                if (text[i] == open) depth++;
                else if (text[i] == close)
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
            }
            return -1;
        }

        private static string LogicName(CapacityData capacity)
        {
            string n = capacity.GetType().Name;
            return n.EndsWith("Data") ? n[..^4] : n;
        }
    }
}
