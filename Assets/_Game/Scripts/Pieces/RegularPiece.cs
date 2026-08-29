namespace Pieces
{
    public class RegularPiece : Piece
    {
        public override string Key => RegularPieceType;
        
        public RegularPiece(PieceView view, string regularPieceType) : base(view)
        {
            RegularPieceType = regularPieceType;
        }

        public string RegularPieceType { get; }
    }
}
