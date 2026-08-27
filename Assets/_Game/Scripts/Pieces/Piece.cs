using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Pieces
{
    public abstract class Piece
    {
        protected Piece(PieceView view)
        {
            View = view;
            view.Piece = this;
        }

        public PieceView View { get; }
        public Vector2Int Coordinate { get; set; }

        public void SetPositionImmediate(Vector3 worldPosition) => View.SetPosition(worldPosition);

        public UniTask MoveToAsync(Vector3 worldPosition, float duration) => View.MoveAsync(worldPosition, duration);

        public UniTask PlayCollapseAsync(float duration) => View.PlayCollapseAsync(duration);

        public void Destroy() => Object.Destroy(View.gameObject);
    }
}
