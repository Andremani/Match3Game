using Cysharp.Threading.Tasks;

namespace Configuration
{
    public interface IConfigFileReader
    {
        UniTask<string> ReadTextAsync(string relativePath);
    }
}
