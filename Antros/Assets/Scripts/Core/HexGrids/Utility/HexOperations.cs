namespace ATCG.HexGrids.Utility
{
    public static class HexOperations
    {
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

        public static int Distance(HexCoordinates a, HexCoordinates b) => Subtract(b, a).Length();
    }
}