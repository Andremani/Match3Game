using System.Collections.Generic;
using UnityEngine;

namespace Pieces
{
    public class PieceFactory
    {
        private readonly PieceViewMappingConfig viewMapping;
        private readonly GameFieldView gameFieldView;

        public PieceFactory(PieceViewMappingConfig viewMapping, GameFieldView gameFieldView)
        {
            this.viewMapping = viewMapping;
            this.gameFieldView = gameFieldView;
        }

        public RegularPiece CreateRegularPiece(string regularPieceType, Transform parent)
        {
            PieceView view = Object.Instantiate(viewMapping.PieceViewPrefab, parent);
            view.Initialize(gameFieldView.CellSize);
            view.SetSprite(viewMapping.GetSprite(regularPieceType));
            return new RegularPiece(view, regularPieceType);
        }

        public RegularPiece CreateRandomRegularPiece(IReadOnlyList<string> allowedRegularPieceTypes, Transform parent)
        {
            string regularPieceType = allowedRegularPieceTypes[Random.Range(0, allowedRegularPieceTypes.Count)];
            return CreateRegularPiece(regularPieceType, parent);
        }
    }
}
