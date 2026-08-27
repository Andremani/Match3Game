using System.Collections.Generic;

namespace Configuration
{
    public class PieceMappingsConfig
    {
        public PieceMappingsConfig(Dictionary<char, string> pieceKeysBySymbol, List<char> orderedSymbols)
        {
            PieceKeysBySymbol = pieceKeysBySymbol;
            OrderedSymbols = orderedSymbols;
        }

        public Dictionary<char, string> PieceKeysBySymbol { get; }
        public List<char> OrderedSymbols { get; }
    }
}
