namespace Configuration
{
    public class LevelConfig
    {
        public LevelConfig(
            int horizontalSize,
            int verticalSize,
            int maxRegularPieceTypes,
            char[] regularPieceTypes,
            char winPieceType,
            int winPieceAmount,
            int maxTurns,
            char[,] map)
        {
            HorizontalSize = horizontalSize;
            VerticalSize = verticalSize;
            MaxRegularPieceTypes = maxRegularPieceTypes;
            RegularPieceTypes = regularPieceTypes;
            WinPieceType = winPieceType;
            WinPieceAmount = winPieceAmount;
            MaxTurns = maxTurns;
            Map = map;
        }

        public int HorizontalSize { get; }
        public int VerticalSize { get; }
        public int MaxRegularPieceTypes { get; }
        public char[] RegularPieceTypes { get; }
        public char WinPieceType { get; }
        public int WinPieceAmount { get; }
        public int MaxTurns { get; }
        public char[,] Map { get; }
    }
}
