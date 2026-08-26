using System;
using System.Collections.Generic;

namespace Configuration
{
    public class ConfigParser
    {
        private const string LevelMapStartMarker = "LevelMap:";
        private const string LevelMapEndMarker = "LevelMapEnd";

        public LevelsListConfig ParseLevelsList(string raw)
        {
            List<string> levelNames = new();

            foreach (var line in SplitNonEmptyLines(raw))
            {
                levelNames.Add(line);
            }

            return new LevelsListConfig(levelNames);
        }

        public PieceMappingsConfig ParsePieceMappings(string raw)
        {
            Dictionary<char, string> pieceKeysBySymbol = new();

            foreach (var line in SplitNonEmptyLines(raw))
            {
                (string key, string value) = ParseKeyValueLine(line);
                pieceKeysBySymbol[key[0]] = value;
            }

            return new PieceMappingsConfig(pieceKeysBySymbol);
        }

        public LevelConfig ParseLevelConfig(string raw)
        {
            Dictionary<string, string> headers = new();
            List<string> mapRows = new();
            bool inMap = false;

            foreach (var line in SplitNonEmptyLines(raw))
            {
                if (!inMap)
                {
                    if (line == LevelMapStartMarker)
                    {
                        inMap = true;
                        continue;
                    }

                    (string key, string value) = ParseKeyValueLine(line);
                    headers[key] = value;
                }
                else
                {
                    if (line == LevelMapEndMarker)
                    {
                        break;
                    }

                    mapRows.Add(line);
                }
            }

            int horizontalSize = int.Parse(headers["HorizontalSize"]);
            int verticalSize = int.Parse(headers["VerticalSize"]);

            if (mapRows.Count != verticalSize)
            {
                throw new FormatException(
                    $"Level map has {mapRows.Count} rows, expected VerticalSize={verticalSize}");
            }

            char[,] map = new char[horizontalSize, verticalSize];

            for (int y = 0; y < verticalSize; y++)
            {
                string row = mapRows[y];

                if (row.Length != horizontalSize)
                {
                    throw new FormatException(
                        $"Level map row {y} has length {row.Length}, expected HorizontalSize={horizontalSize}");
                }

                for (int x = 0; x < horizontalSize; x++)
                {
                    map[x, y] = row[x];
                }
            }

            return new LevelConfig(
                horizontalSize,
                verticalSize,
                int.Parse(headers["MaxRegularPieceTypes"]),
                headers["WinPieceType"][0],
                int.Parse(headers["WinPieceAmount"]),
                int.Parse(headers["MaxTurns"]),
                map);
        }

        private static (string Key, string Value) ParseKeyValueLine(string line)
        {
            int separatorIndex = line.IndexOf(':');

            if (separatorIndex < 0)
            {
                throw new FormatException($"Expected a 'key: value' line, got '{line}'");
            }

            string key = line.Substring(0, separatorIndex).Trim();
            string value = line.Substring(separatorIndex + 1).Trim();
            return (key, value);
        }

        private static IEnumerable<string> SplitNonEmptyLines(string raw)
        {
            string[] lines = raw.Split('\n');

            foreach (var rawLine in lines)
            {
                string line = rawLine.TrimEnd('\r').Trim();

                if (line.Length > 0)
                {
                    yield return line;
                }
            }
        }
    }
}
