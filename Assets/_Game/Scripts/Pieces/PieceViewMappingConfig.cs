using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pieces
{
    [CreateAssetMenu(fileName = "PieceViewMappingConfig", menuName = "Match3Test/Piece View Mapping")]
    public class PieceViewMappingConfig : ScriptableObject
    {
        [SerializeField] private PieceView pieceViewPrefab;
        [SerializeField] private List<Entry> entries = new();

        private Dictionary<string, Sprite> spritesByKey;

        public PieceView PieceViewPrefab => pieceViewPrefab;

        public Sprite GetSprite(string key)
        {
            spritesByKey ??= entries.ToDictionary(entry => entry.Key, entry => entry.Sprite);

            if (!spritesByKey.TryGetValue(key, out Sprite sprite))
            {
                throw new KeyNotFoundException($"No sprite mapped for key '{key}'");
            }

            return sprite;
        }

        [Serializable]
        private struct Entry
        {
            public string Key;
            public Sprite Sprite;
        }
    }
}
