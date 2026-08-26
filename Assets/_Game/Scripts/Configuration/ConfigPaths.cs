namespace Configuration
{
    public static class ConfigPaths
    {
        private const string Root = "_Game/Configuration";

        public const string LevelsList = Root + "/LevelsList.txt";
        public const string PieceMappings = Root + "/LevelPieceMappings.txt";

        public static string Level(string levelName) => $"{Root}/Levels/{levelName}.txt";
    }
}
