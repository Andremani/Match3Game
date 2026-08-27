using UnityEngine;

namespace Pieces
{
    public class PieceFactory
    {
        private readonly PieceViewMappingConfig viewMapping;

        public PieceFactory(PieceViewMappingConfig viewMapping)
        {
            this.viewMapping = viewMapping;
        }

        public RegularPiece CreateRegularPiece(string key, Transform parent)
        {
            PieceView view = Object.Instantiate(viewMapping.PieceViewPrefab, parent);
            view.SetSprite(viewMapping.GetSprite(key));
            return new RegularPiece(view, key);
        }
    }
}
