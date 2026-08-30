using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Pieces
{
    public class PieceSwipeHandler
    {
        private const float ReturnDuration = 0.15f;

        private readonly SwipeGestureDetector gestureDetector;

        private PieceView pressedView;
        private Vector2 pressWorldPosition;

        public event Action<Piece, SwipeDirection> Swiped;

        public PieceSwipeHandler(SwipeGestureDetector gestureDetector)
        {
            this.gestureDetector = gestureDetector;

            gestureDetector.PressStarted += OnPressStarted;
            gestureDetector.Dragging += OnDragging;
            gestureDetector.PressCanceled += OnPressCanceled;
            gestureDetector.Swiped += OnSwiped;
        }

        private void OnPressStarted(Vector2 worldPosition)
        {
            pressWorldPosition = worldPosition;
            pressedView = RaycastPieceView(worldPosition);
        }

        private void OnDragging(Vector2 currentWorldPosition)
        {
            if (pressedView == null) return;

            pressedView.Piece?.SetNudgeOffset(currentWorldPosition - pressWorldPosition);
        }

        private void OnPressCanceled()
        {
            pressedView?.Piece?.ReturnHomeAsync(ReturnDuration).Forget();
            pressedView = null;
        }

        private void OnSwiped(Vector2 originWorldPosition, SwipeDirection direction)
        {
            PieceView view = pressedView;
            pressedView = null;

            Piece piece = view?.Piece;
            if (piece == null) return;

            piece.ReturnHomeAsync(ReturnDuration).Forget();
            Swiped?.Invoke(piece, direction);
        }

        private static PieceView RaycastPieceView(Vector2 worldPosition)
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
            return hit.collider != null ? hit.collider.GetComponent<PieceView>() : null;
        }
    }
}
