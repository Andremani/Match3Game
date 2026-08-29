using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Pieces
{
    [RequireComponent(typeof(Collider2D))]
    public class PieceView : MonoBehaviour
    {
        private const float NudgeFactor = 0.1f;
        private const int MovingSortingOrderOffset = 1;

        private SpriteRenderer spriteRenderer;
        private Vector3 homePosition;
        private float maxDragDistance;

        private bool isMoving;
        private int baseSortingOrder;
        private SpriteMaskInteraction baseMaskInteraction;

        public Piece Piece { get; set; }
        public SpriteMaskInteraction MaskInteraction 
        {
            get
            {
                return spriteRenderer.maskInteraction;
            }
            set
            {
                spriteRenderer.maskInteraction = value;
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseSortingOrder = spriteRenderer.sortingOrder;
            baseMaskInteraction = spriteRenderer.maskInteraction;
        }

        public void Initialize(Vector2 cellSize)
        {
            maxDragDistance = Mathf.Min(cellSize.x, cellSize.y) * NudgeFactor;
        }

        public void SetPosition(Vector3 worldPosition)
        {
            homePosition = worldPosition;
            transform.position = worldPosition;
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        public void SetNudgeOffset(Vector2 rawOffset)
        {
            SetMoving(true);
            transform.position = homePosition + (Vector3)Vector2.ClampMagnitude(rawOffset, maxDragDistance);
        }

        public UniTask ReturnHomeAsync(float duration) => MoveAsync(homePosition, duration);

        public async UniTask MoveAsync(Vector3 targetPosition, float duration, Ease ease = Ease.Linear)
        {
            homePosition = targetPosition;
            SetMoving(true);

            UniTaskCompletionSource completionSource = new();
            transform.DOMove(targetPosition, duration).SetEase(ease).OnComplete(() => completionSource.TrySetResult());
            await completionSource.Task;

            SetMoving(false);
        }

        private void SetMoving(bool moving)
        {
            if (isMoving == moving) return;

            isMoving = moving;
            spriteRenderer.sortingOrder = baseSortingOrder + (moving ? MovingSortingOrderOffset : 0);
            spriteRenderer.maskInteraction = moving ? SpriteMaskInteraction.None : baseMaskInteraction;
        }

        public UniTask PlayCollapseAsync(float duration)
        {
            UniTaskCompletionSource completionSource = new();

            Sequence sequence = DOTween.Sequence();
            sequence.Join(transform.DOScale(Vector3.zero, duration));

            if (spriteRenderer != null)
            {
                sequence.Join(spriteRenderer.DOFade(0f, duration));
            }

            sequence.OnComplete(() => completionSource.TrySetResult());

            return completionSource.Task;
        }
    }
}
