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

        public static HexCoordinates Offset(this HexCoordinates a, int direction, int offset = 1) => a.Offset((HexDirection)direction, offset);
        public static HexCoordinates Offset(this HexCoordinates a, HexDirection direction, int offset = 1)
        {
            HexCoordinates hexDirection = direction.GetDirection();
            return a.Add(hexDirection.Multiply(offset));
        }

        public static int Distance(HexCoordinates a, HexCoordinates b) => b.Subtract(a).Length();

        public static void GetSpiral(this HexCoordinates center, int radius, List<HexCoordinates> results)
        {
            results.Add(center);

            // Boucle de 1 jusqu'au rayon inclus
            for (int k = 1; k <= radius; k++)
                center.GetRing(k, results);
        }

        public static void GetLine(HexCoordinates a, HexCoordinates b, List<HexCoordinates> results)
        {
            int distance = Distance(a, b);

            FractionalHexCoordinates aFrac = new(a.X, a.Y, a.Z);
            FractionalHexCoordinates bFrac = new(b.X, b.Y, b.Z);

            float step = 1f / Mathf.Max(distance, 1);
            for (int i = 0; i <= distance; i++)
            {
                FractionalHexCoordinates hexLerp = aFrac.HexLerp(bFrac, step * i);
                results.Add(hexLerp.HexRound());
            }
        }

        public static void GetRing(this HexCoordinates center, int radius, List<HexCoordinates> results)
        {
            // Le code ne fonctionne pas pour radius == 0 car la boucle j ne s'exécuterait jamais
            // et on ne retournerait aucune liste, ou une liste vide.
            if (radius <= 0)
                return;

            // 1. On commence par se déplacer du centre vers un coin du cercle (direction 4)
            // à une distance égale au rayon.
            HexCoordinates direction = HexDirection.BottomLeft.GetDirection();
            HexCoordinates hex = center + direction.Multiply(radius);

            // 2. On parcourt les 6 directions de l'hexagone
            for (int i = 0; i < DirectionsCount; i++)
            {
                // 3. Pour chaque direction, on avance de 'radius' pas
                for (int j = 0; j < radius; j++)
                {
                    results.Add(hex);
                    hex += directions[i];
                }
            }
        }
    }
}