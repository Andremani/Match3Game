using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pieces
{
    public class SwipeInputReader : MonoBehaviour
    {
        private const float ReturnDuration = 0.15f;

        [SerializeField] private float swipeThreshold = 0.3f;

        private Camera mainCamera;

        private PieceView pressedView;
        private Vector2 pressWorldPosition;

        public event Action<Piece, SwipeDirection> Swiped;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (TryGetPointerDownPosition(out Vector2 downPosition))
            {
                pressedView = RaycastPieceView(downPosition);
                pressWorldPosition = downPosition;
                return;
            }

            if (pressedView == null) return;

            if (!IsPointerPressed())
            {
                CancelDrag();
                return;
            }

            if (!TryGetPointerCurrentPosition(out Vector2 currentPosition)) return;

            Vector2 delta = currentPosition - pressWorldPosition;

            if (delta.magnitude < swipeThreshold)
            {
                pressedView.Piece?.SetNudgeOffset(delta);
                return;
            }

            ResolveSwipe(GetDirection(delta));
        }

        private void CancelDrag()
        {
            pressedView.Piece?.ReturnHomeAsync(ReturnDuration).Forget();
            pressedView = null;
        }

        private void ResolveSwipe(SwipeDirection direction)
        {
            Piece piece = pressedView.Piece;
            pressedView = null;

            if (piece == null) return;

            Debug.Log($"Swipe detected: {piece} -> {direction}");
            piece.ReturnHomeAsync(ReturnDuration).Forget();
            Swiped?.Invoke(piece, direction);
        }

        private PieceView RaycastPieceView(Vector2 worldPosition)
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
            return hit.collider != null ? hit.collider.GetComponent<PieceView>() : null;
        }

        private bool IsPointerPressed()
        {
            if (Mouse.current != null && Mouse.current.leftButton.isPressed) return true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) return true;
            return false;
        }

        private bool TryGetPointerDownPosition(out Vector2 worldPosition)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                worldPosition = ScreenToWorld(Mouse.current.position.ReadValue());
                return true;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                worldPosition = ScreenToWorld(Touchscreen.current.primaryTouch.position.ReadValue());
                return true;
            }

            worldPosition = default;
            return false;
        }

        private bool TryGetPointerCurrentPosition(out Vector2 worldPosition)
        {
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                worldPosition = ScreenToWorld(Mouse.current.position.ReadValue());
                return true;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                worldPosition = ScreenToWorld(Touchscreen.current.primaryTouch.position.ReadValue());
                return true;
            }

            worldPosition = default;
            return false;
        }

        private Vector2 ScreenToWorld(Vector2 screenPosition) => mainCamera.ScreenToWorldPoint(screenPosition);

        private static SwipeDirection GetDirection(Vector2 delta)
        {
            return Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                ? delta.x >= 0 ? SwipeDirection.Right : SwipeDirection.Left
                : delta.y >= 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }
    }
}
