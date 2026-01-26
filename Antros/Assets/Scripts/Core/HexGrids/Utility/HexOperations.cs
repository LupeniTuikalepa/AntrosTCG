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

        public static int Distance(HexCell a, HexCell b) => 1;
        public static int Length(HexCell a) => 1;

        public static HexCoordinates Add(HexCoordinates a, HexCoordinates b) {
            return new HexCoordinates(a.X + b.X, a.Y + b.Y);
        }

        public static HexCoordinates Subtract(HexCoordinates a, HexCoordinates b) {
            return  new HexCoordinates(a.X - b.X, a.Y - b.Y);
        }

        public static HexCoordinates Multiply(HexCoordinates a, int k) {
            return new HexCoordinates(a.X * k, a.Y * k);
        }

        public static HexCoordinates GetDirection(HexDirection hexDirection) => directions[(int)hexDirection];
        public static HexCoordinates Offset(HexCoordinates a, HexDirection direction, int offset = 1)
        {
            HexCoordinates hexDirection = GetDirection(direction);
            return Add(a, Multiply(hexDirection, offset));
        }
        public static int Distance(HexCoordinates a, HexCoordinates b) => Subtract(b, a).Length();
    }
}