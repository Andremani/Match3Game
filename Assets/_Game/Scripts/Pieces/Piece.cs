using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Pieces
{
    public abstract class Piece
    {
        public virtual string Key => GetType().Name;

        protected Piece(PieceView view)
        {
            View = view;
            view.Piece = this;
        }

        public PieceView View { get; }
        public Vector2Int Coordinate { get; set; }

        public event Action<Piece> MatchResolved;

        public void SetPositionImmediate(Vector3 worldPosition) => View.SetPosition(worldPosition);

        public UniTask MoveToAsync(Vector3 worldPosition, float duration, Ease ease = Ease.Linear) =>
            View.MoveAsync(worldPosition, duration, ease);

        public void SetNudgeOffset(Vector2 offset) => View.SetNudgeOffset(offset);

        public UniTask ReturnHomeAsync(float duration) => View.ReturnHomeAsync(duration);

        public UniTask PlayCollapseAsync(float duration) => View.PlayCollapseAsync(duration);

        public void Destroy() => UnityEngine.Object.Destroy(View.gameObject);

        public virtual async UniTask ResolveMatchAsync(float duration)
        {
            await PlayCollapseAsync(duration);
            Destroy();
            MatchResolved?.Invoke(this);
        }

        public static bool AreSame(Piece firstPiece, Piece secondPiece) => firstPiece.Key == secondPiece.Key;

        public static bool AreSame(Piece piece, string key) => piece != null && piece.Key == key;
    }
}
