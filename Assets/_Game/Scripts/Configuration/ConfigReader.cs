using Cysharp.Threading.Tasks;

namespace Configuration
{
    public class ConfigReader
    {
        private readonly IConfigFileReader fileReader;
        private readonly ConfigParser parser;

        public ConfigReader(IConfigFileReader fileReader, ConfigParser parser)
        {
            this.fileReader = fileReader;
            this.parser = parser;
        }

        public async UniTask<LevelsListConfig> LoadLevelsListAsync()
        {
            string raw = await fileReader.ReadTextAsync(ConfigPaths.LevelsList);
            return parser.ParseLevelsList(raw);
        }

        public async UniTask<PieceMappingsConfig> LoadPieceMappingsAsync()
        {
            string raw = await fileReader.ReadTextAsync(ConfigPaths.PieceMappings);
            return parser.ParsePieceMappings(raw);
        }

        public async UniTask<LevelConfig> LoadLevelConfigAsync(string levelName)
        {
            string raw = await fileReader.ReadTextAsync(ConfigPaths.Level(levelName));
            return parser.ParseLevelConfig(raw);
        }
    }
}
