using System.Collections.Generic;

namespace Configuration
{
    public class LevelsListConfig
    {
        public LevelsListConfig(List<string> levelNames)
        {
            LevelNames = levelNames;
        }

        public List<string> LevelNames { get; }
    }
}
