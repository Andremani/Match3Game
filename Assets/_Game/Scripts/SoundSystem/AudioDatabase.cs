using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "Match3Test/Audio Database")]
public class AudioDatabase : ScriptableObject, ISerializationCallbackReceiver
{
    [Serializable]
    public struct DatabaseEntry
    {
        public string key;
        public SoundConfig config;
    }

    [SerializeField] private List<DatabaseEntry> soundEntries = new List<DatabaseEntry>();

    private readonly Dictionary<string, SoundConfig> _database = new Dictionary<string, SoundConfig>();

    public SoundConfig GetSound(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        if (_database.TryGetValue(key, out var config))
        {
            return config;
        }

        Debug.LogWarning($"[AudioDatabase] Config with key '{key}' not found!");
        return null;
    }

    public void OnAfterDeserialize()
    {
        _database.Clear();
        foreach (var entry in soundEntries)
        {
            if (string.IsNullOrEmpty(entry.key)) continue;

            if (_database.ContainsKey(entry.key))
            {
                Debug.LogError($"[AudioDatabase] Key dublicate '{entry.key}' audio database!");
                continue;
            }

            _database.Add(entry.key, entry.config);
        }
    }

    public void OnBeforeSerialize() {}
}