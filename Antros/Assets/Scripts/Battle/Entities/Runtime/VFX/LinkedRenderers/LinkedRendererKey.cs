using System;

namespace ATCG.Battle.Entities.Runtime.VFX
{
    /// <summary>
    /// Where a <see cref="LinkedRenderer"/> lives on an entity. Flags so a single renderer
    /// can be tagged with several places at once (e.g. a chest-plate mesh can be both
    /// <see cref="Chest"/> and <see cref="Clothes"/>), and so a query can target an exact
    /// combination instead of a single bone. Not bone-driven on purpose: Clothes and
    /// Weapons aren't part of the skeleton, so <see cref="LinkedRendererMapper"/> can only
    /// auto-assign the body-part flags — clothing/weapon renderers get their key set by
    /// hand on the LinkedRenderer component.
    /// </summary>
    [Flags]
    public enum LinkedRendererKey
    {
        None = 0,
        All = -1,
        
        Head = 1 << 0,
        Eyes = 1 << 1,
        Mouth = 1 << 2,
        RightArm = 1 << 3,
        LeftArm = 1 << 4,
        RightHand = 1 << 5,
        LeftHand = 1 << 6,
        RightLeg = 1 << 7,
        LeftLeg = 1 << 8,
        Chest = 1 << 9,
        Clothes = 1 << 10,
        Weapons = 1 << 11,

        // Convenience unions — not extra bits, just shorthands for common combinations.
        Arms = LeftArm | RightArm,
        Legs = LeftLeg | RightLeg,
        Hands = LeftHand | RightHand,
        Body = Head | Eyes | Mouth | RightArm | LeftArm | RightHand | LeftHand | RightLeg | LeftLeg | Chest,
    }

    /// <summary>
    /// Bitwise helpers for <see cref="LinkedRendererKey"/>. Plain '&amp;'/'|' on same-typed
    /// enums doesn't box in C#, but Enum.HasFlag does (it takes object) — these avoid it.
    /// </summary>
    public static class LinkedRendererKeyExtensions
    {
        /// <summary>True if 'value' has at least one of the flags in 'mask'.</summary>
        public static bool HasAny(this LinkedRendererKey value, LinkedRendererKey mask) => (value & mask) != 0;

        /// <summary>True if 'value' has every flag in 'mask'.</summary>
        public static bool HasAll(this LinkedRendererKey value, LinkedRendererKey mask) => (value & mask) == mask;
    }
}