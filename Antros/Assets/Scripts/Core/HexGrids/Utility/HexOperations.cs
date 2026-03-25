using System.Collections.Generic;
using UnityEngine;

namespace ATCG.HexGrids.Utility
{
    public static class HexOperations
    {
        private static readonly HexCoordinates[] directions = {
            new HexCoordinates(1, 0),
            new HexCoordinates(1, -1),
            new HexCoordinates(0, -1),
            new HexCoordinates(-1, 0),
            new HexCoordinates(-1, 1),
            new HexCoordinates(0, 1)
        };

        public static int DirectionsCount => directions.Length;


        public static HexCoordinates Add(this HexCoordinates a, HexCoordinates b) {
            return new HexCoordinates(a.X + b.X, a.Y + b.Y);
        }

        public static HexCoordinates Subtract(this HexCoordinates a, HexCoordinates b) {
            return  new HexCoordinates(a.X - b.X, a.Y - b.Y);
        }

        public static HexCoordinates Multiply(this HexCoordinates a, int k) {
            return new HexCoordinates(a.X * k, a.Y * k);
        }
        public static HexCoordinates GetNeighbor(this HexCoordinates coordinates, int direction) => coordinates.GetNeighbor((HexDirection)direction);
        public static HexCoordinates GetNeighbor(this HexCoordinates coordinates, HexDirection direction) => coordinates + direction.GetDirection();
        public static HexCoordinates GetDirection(this HexDirection hexDirection) => directions[(int)hexDirection];
        public static HexCoordinates GetDirection(this HexCoordinates a, HexCoordinates b) => b - a;
        public static HexCoordinates Reverse(this HexCoordinates a) => a * -1;
        public static HexCoordinates GetNormalizedDirection(this HexCoordinates a, HexCoordinates b) => a.GetDirection(b).Normalize();

        public static HexCoordinates Offset(this HexCoordinates a, int direction, int offset = 1) => a.Offset((HexDirection)direction, offset);
        public static HexCoordinates Offset(this HexCoordinates a, HexDirection direction, int offset = 1)
        {
            HexCoordinates hexDirection = direction.GetDirection();
            return a.Add(hexDirection.Multiply(offset));
        }

        public static HexCoordinates NearestCardinal(this HexCoordinates coord)
        {
            if (coord.IsCardinal())
                return coord;

            HexCoordinates normalized = coord.Normalize();

            HexCoordinates best = directions[0];
            float bestDot = float.MinValue;

            for (int i = 0; i < directions.Length; i++)
            {
                HexCoordinates c = directions[i];
                float dot = c.X * normalized.X + c.Y * normalized.Y + c.Z * normalized.Z;
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = c;
                }
            }

            return best;
        }

        public static bool IsCardinal(this HexCoordinates coord)
        {
            return (coord.X == 0 || coord.Y == 0 || coord.Z == 0) && (coord.X + coord.Y + coord.Z == 0);
        }

        public static HexCoordinates Normalize(this HexCoordinates coord)
        {
            int distance = Mathf.Max(Mathf.Abs(coord.X), Mathf.Abs(coord.Y), Mathf.Abs(coord.Z));
            if (distance == 0)
                return coord;

            return new HexCoordinates(coord.X / distance, coord.Y / distance);
        }

        public static int Distance(this HexCoordinates a, HexCoordinates b) => b.Subtract(a).Length();

        public static void GetSpiral(this HexCoordinates center, int radius, List<HexCoordinates> results)
        {
            results.AddRange(center.GetSpiral(radius));
        }

        public static IEnumerable<HexCoordinates> GetSpiral(this HexCoordinates center, int radius)
        {
            if(radius <= 0)
                yield break;

            yield return center;

            // Boucle de 1 jusqu'au rayon inclus
            for (int k = 1; k <= radius; k++)
            {
                foreach (var coord in center.GetRing(k))
                    yield return coord;
            }
        }

        public static void GetLine(HexCoordinates a, HexCoordinates b, List<HexCoordinates> results)
        {
            results.AddRange(a.GetLine(b));
        }

        public static IEnumerable<HexCoordinates> GetLine(this HexCoordinates a, HexCoordinates b)
        {
            int distance = Distance(a, b);

            FractionalHexCoordinates aFrac = new(a.X, a.Y, a.Z);
            FractionalHexCoordinates bFrac = new(b.X, b.Y, b.Z);

            float step = 1f / Mathf.Max(distance, 1);
            for (int i = 0; i <= distance; i++)
            {
                FractionalHexCoordinates hexLerp = aFrac.HexLerp(bFrac, step * i);
                yield return hexLerp.HexRound();
            }
        }

        public static void GetRing(this HexCoordinates center, int radius, List<HexCoordinates> results)
        {
            results.AddRange(center.GetRing(radius));
        }

        public static IEnumerable<HexCoordinates> GetRing(this HexCoordinates center, int radius)
        {
            if (radius <= 0)
                yield break;
            HexCoordinates direction = HexDirection.BottomLeft.GetDirection();
            HexCoordinates hex = center + direction.Multiply(radius);

            for (int i = 0; i < DirectionsCount; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    yield return hex;
                    hex += directions[i];
                }
            }
        }
    }
}