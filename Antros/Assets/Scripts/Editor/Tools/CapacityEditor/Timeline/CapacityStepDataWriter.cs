using System.Text;
using ATCG.Capacities;
using UnityEditor;
using UnityEngine;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Writes scan results back into a CapacityData asset's generated per-step
    /// fields ({Identifier}StepData). Those fields are ReadOnly auto-properties
    /// by design (CapacityDataGenerator), so writing goes through SerializedObject
    /// on the backing field — never reflection, never InternalsVisibleTo.
    /// </summary>
    public static class CapacityStepDataWriter
    {
        public static bool TrySetQteCount(CapacityData capacity, string stepName, int qteCount)
        {
            SerializedObject so = new(capacity);
            string identifier = ToIdentifier(stepName);
            SerializedProperty stepDataProp = so.FindProperty($"<{identifier}StepData>k__BackingField");

            if (stepDataProp == null)
            {
                Debug.LogWarning($"[CapacityTimelineEditor] No generated field '{identifier}StepData' on " +
                                  $"'{capacity.GetType().Name}' for step '{stepName}'. Resync [WithStep] attributes.");
                return false;
            }

            SerializedProperty stepNameProp = stepDataProp.FindPropertyRelative("<StepName>k__BackingField");
            SerializedProperty qteCountProp = stepDataProp.FindPropertyRelative("<QTEsCount>k__BackingField");

            bool changed = qteCountProp.intValue != qteCount || stepNameProp.stringValue != stepName;
            if (!changed)
                return false;

            stepNameProp.stringValue = stepName;
            qteCountProp.intValue = qteCount;
            so.ApplyModifiedProperties();
            return true;
        }

        // Mirrors CapacityDataGenerator.ToIdentifier exactly (raw step name -> PascalCase
        // field identifier). Keep both in sync if the generator's algorithm changes.
        public static string ToIdentifier(string raw)
        {
            StringBuilder sb = new(raw.Length);
            bool upperNext = true;
            foreach (char c in raw)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(upperNext ? char.ToUpperInvariant(c) : c);
                    upperNext = false;
                }
                else
                {
                    upperNext = true;
                }
            }
            if (sb.Length == 0)
                return "_";
            if (char.IsDigit(sb[0]))
                sb.Insert(0, '_');
            return sb.ToString();
        }
    }
}
