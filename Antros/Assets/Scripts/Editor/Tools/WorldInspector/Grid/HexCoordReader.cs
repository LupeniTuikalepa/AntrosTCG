using System;
using System.Collections.Generic;
using System.Reflection;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Reads axial/cube coordinates out of the game's HexCoordinates type, which lives
    /// in a separate assembly. HexCoordinates is cube-based with public x and y (z is
    /// derived as -x-y). We resolve the x/y members once by reflection and cache the
    /// accessors so per-entity reads stay cheap.
    /// </summary>
    public static class HexCoordReader
    {
        private static Type cachedType;
        private static Func<object, int> getX;
        private static Func<object, int> getY;

        public readonly struct Axial : IEquatable<Axial>
        {
            public readonly int X;
            public readonly int Y;
            public int Z => -X - Y;

            public Axial(int x, int y)
            {
                X = x;
                Y = y;
            }

            public bool Equals(Axial other) => X == other.X && Y == other.Y;
            public override bool Equals(object obj) => obj is Axial a && Equals(a);
            public override int GetHashCode() => unchecked(X * 73856093 ^ Y * 19349663);
            public override string ToString() => $"({X} : {Y})";
        }

        public static bool TryRead(object hexCoordinatesBoxed, out Axial axial)
        {
            axial = default;
            if (hexCoordinatesBoxed == null)
                return false;

            Type type = hexCoordinatesBoxed.GetType();
            if (type != cachedType)
                Resolve(type);

            if (getX == null || getY == null)
            {
                InspectorLog.Warn($"Couldn't resolve X/Y on {type.Name} — grid can't place cells. Check the field names.");
                return false;
            }

            try
            {
                axial = new Axial(getX(hexCoordinatesBoxed), getY(hexCoordinatesBoxed));
                return true;
            }
            catch (Exception e)
            {
                InspectorLog.Warn("Reading HexCoordinates X/Y threw — grid layout may be incomplete", e);
                return false;
            }
        }

        private static void Resolve(Type type)
        {
            cachedType = type;
            getX = BuildAccessor(type, "x", "X");
            getY = BuildAccessor(type, "y", "Y");
        }

        private static Func<object, int> BuildAccessor(Type type, params string[] names)
        {
            foreach (string n in names)
            {
                FieldInfo field = type.GetField(n, BindingFlags.Public | BindingFlags.Instance);
                if (field != null && IsIntLike(field.FieldType))
                    return o => Convert.ToInt32(field.GetValue(o));

                PropertyInfo prop = type.GetProperty(n, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && IsIntLike(prop.PropertyType))
                    return o => Convert.ToInt32(prop.GetValue(o));
            }
            return null;
        }

        private static bool IsIntLike(Type t) =>
            t == typeof(int) || t == typeof(short) || t == typeof(long) || t == typeof(byte);
    }
}
