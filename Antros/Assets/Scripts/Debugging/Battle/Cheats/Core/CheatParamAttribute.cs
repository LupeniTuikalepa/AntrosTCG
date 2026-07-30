using System;

namespace ATCG.Debugging.Cheats
{
    /// <summary>
    /// Marks a public field of a cheat as an editable parameter. The editor Cheats tool renders a
    /// control matching the field type (int/float → field or slider when a range is given,
    /// bool → toggle, string → text, enum / [Flags] enum → enum field, Vector2/3(Int) → vector
    /// field, Color → colour field, UnityEngine.Object subtypes → object field) and binds it back
    /// to the instance, so Execute just reads the field. For entity targets use
    /// <see cref="CheatTargetAttribute"/> instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CheatParamAttribute : Attribute
    {
        /// <summary>Optional display label (defaults to the nicified field name).</summary>
        public string Label { get; set; }

        /// <summary>Optional tooltip shown on the control.</summary>
        public string Tooltip { get; set; }

        /// <summary>Lower bound for numeric fields; set together with <see cref="Max"/> to get a slider.</summary>
        public double Min { get; set; } = double.NaN;

        /// <summary>Upper bound for numeric fields; set together with <see cref="Min"/> to get a slider.</summary>
        public double Max { get; set; } = double.NaN;

        public bool HasRange => !double.IsNaN(Min) && !double.IsNaN(Max);

        public CheatParamAttribute() { }

        public CheatParamAttribute(string label) => Label = label;
    }
}
