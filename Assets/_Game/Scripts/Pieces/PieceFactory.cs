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

        public RegularPiece CreateRegularPiece(string key, Transform parent)
        {
            PieceView view = Object.Instantiate(viewMapping.PieceViewPrefab, parent);
            view.Initialize(gameFieldView.CellSize);
            view.SetSprite(viewMapping.GetSprite(key));
            return new RegularPiece(view, key);
        }

        public RegularPiece CreateRandomRegularPiece(IReadOnlyList<string> allowedKeys, Transform parent)
        {
            string key = allowedKeys[Random.Range(0, allowedKeys.Count)];
            return CreateRegularPiece(key, parent);
        }
    }
}
