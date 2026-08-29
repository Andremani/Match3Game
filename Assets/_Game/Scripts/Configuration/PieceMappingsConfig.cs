using System.Collections.Generic;

namespace Configuration
{
    public class PieceMappingsConfig
    {
        private const char EmptySymbol = '-';
        private const char EmptySymbolAlt = ' ';
        public const char RandomSymbol = '*';

        public static bool IsEmptySymbol(char symbol) => symbol == EmptySymbol || symbol == EmptySymbolAlt;

        public PieceMappingsConfig(Dictionary<char, string> pieceKeysBySymbol, List<char> orderedSymbols)
        {
            PieceKeysBySymbol = pieceKeysBySymbol;
            OrderedSymbols = orderedSymbols;
        }

        public Dictionary<char, string> PieceKeysBySymbol { get; }
        public List<char> OrderedSymbols { get; }
    }
}
