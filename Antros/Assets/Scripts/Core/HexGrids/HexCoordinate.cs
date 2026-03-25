using System;
using ATCG.HexGrids.Utility;
using Unity.Burst;
using UnityEngine;

namespace ATCG.HexGrids
{
    [System.Serializable]
    public struct HexCoordinates : IEquatable<HexCoordinates>
    {
        public static HexCoordinates None => new();
        public bool IsValid { get; }

        [field: SerializeField]
        public int X { get; private set; }

        [field: SerializeField]
        public int Y { get; private set; }

        public int Z => -X - Y;

        public HexCoordinates (int x, int y) {
            X = x;
            Y = y;
            IsValid = true;
        }

        public static HexCoordinates FromOffsetCoordinates (int x, int z) {
            return new HexCoordinates(x, z);
        }

        public bool Equals(HexCoordinates other)
        {
            return X == other.X && Y == other.Y;
        }

        [BurstDiscard]
        public override bool Equals(object obj)
        {
            return obj is HexCoordinates other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString() => $"({X} : {Y})";
        public int Length() => (int)((Mathf.Abs(X) + Mathf.Abs(Y) + Mathf.Abs(Z)) * .5f);

        public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b) => HexOperations.Add(a, b);
        public static HexCoordinates operator -(HexCoordinates a, HexCoordinates b) => HexOperations.Subtract(a, b);
        public static HexCoordinates operator *(HexCoordinates a, int k) => HexOperations.Multiply(a, k);

        public static bool operator ==(HexCoordinates a, HexCoordinates b) => a.X == b.X && a.Y == b.Y;

        public static bool operator !=(HexCoordinates a, HexCoordinates b) => a.X != b.X || a.Y != b.Y;


        public static implicit operator Vector2Int(HexCoordinates coordinates) => new Vector2Int(coordinates.X, coordinates.Y);
        public static implicit operator (int, int)(HexCoordinates coordinates) => new (coordinates.X, coordinates.Y);
    }
}