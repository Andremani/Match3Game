using System.Collections.Generic;

namespace Configuration
{
    public class PieceMappingsConfig
    {
        public PieceMappingsConfig(Dictionary<char, string> pieceKeysBySymbol)
        {
            PieceKeysBySymbol = pieceKeysBySymbol;
        }

        public Dictionary<char, string> PieceKeysBySymbol { get; }
    }
}
