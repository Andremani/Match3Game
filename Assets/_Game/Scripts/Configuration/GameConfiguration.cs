using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Configuration
{
    public class GameConfiguration
    {
        private readonly ConfigReader configReader;

        public PieceMappingsConfig PieceMappingsConfig { get; private set; }
        public LevelsListConfig LevelsListConfig { get; private set; }
        public Dictionary<string, LevelConfig> LevelConfigs { get; private set; } = new();

        public GameConfiguration(ConfigReader configReader)
        {
            this.configReader = configReader;
        }

        public async UniTask LoadAllConfigsAsync()
        {
            PieceMappingsConfig = await configReader.LoadPieceMappingsAsync();
            LevelsListConfig = await configReader.LoadLevelsListAsync();
            
            LevelConfigs.Clear();
            foreach(var levelName in LevelsListConfig.LevelNames)
            {
                LevelConfig levelConfig = await configReader.LoadLevelConfigAsync(levelName);
                LevelConfigs.Add(levelName, levelConfig);
            }
        }
    }
}
