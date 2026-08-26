using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Configuration
{
    public class WebGLConfigFileReader : IConfigFileReader
    {
        public async UniTask<string> ReadTextAsync(string relativePath)
        {
            string url = System.IO.Path.Combine(Application.streamingAssetsPath, relativePath);
            using UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException($"Failed to read config '{relativePath}': {request.error}");
            }

            return request.downloadHandler.text;
        }
    }
}
