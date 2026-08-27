namespace Pieces
{
    public class RegularPiece : Piece
    {
        public RegularPiece(PieceView view, string key) : base(view)
        {
            Key = key;
        }

        public string Key { get; }
    }
}
