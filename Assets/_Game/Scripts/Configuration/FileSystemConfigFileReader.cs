using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Configuration
{
    public class FileSystemConfigFileReader : IConfigFileReader
    {
        public async UniTask<string> ReadTextAsync(string relativePath)
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            return await UniTask.RunOnThreadPool(() => File.ReadAllText(fullPath));
        }
    }
}
