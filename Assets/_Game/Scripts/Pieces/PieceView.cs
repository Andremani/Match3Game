using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Pieces
{
    [RequireComponent(typeof(Collider2D))]
    public class PieceView : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        public Piece Piece { get; set; }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        public UniTask MoveAsync(Vector3 targetPosition, float duration)
        {
            UniTaskCompletionSource completionSource = new();
            transform.DOMove(targetPosition, duration).OnComplete(() => completionSource.TrySetResult());
            return completionSource.Task;
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
