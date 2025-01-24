using System.Text.Json;

namespace DAL
{
    public class FileManager
    {
        public static class FolderNames
        {
            public const string GameData = "GameData";
            public const string Saves = "Saves";
            public const string Assets = "Assets";
            public const string Configs = "Configs";
        }

        public static class FileNames
        {
            // public const string Version = "Version";
            public const string Equips = "Equipments";
            public const string Skills = "Skills";
            public const string Monsters = "Monsters";

            public const string GameConfig = "GameConfig";
            public const string ServerConfig = "ServerConfig";
            public const string DatabaseConfig = "DatabaseConfig";
        }

        private static readonly string[] UnwantedPaths = ["ConsolePL", "bin", "obj", "Debug"];

        public static readonly string DirPath = RemoveUnwantedParts(Directory.GetCurrentDirectory());
        
        private static readonly JsonSerializerOptions _toJsonOption = new()
        {
            WriteIndented = true
        };

        private static string RemoveUnwantedParts(string path)
        {
            foreach (string unwanted in UnwantedPaths)
            {
                while (path.Contains(unwanted, StringComparison.OrdinalIgnoreCase))
                {
                    path = Path.GetDirectoryName(path)!;
                }
            }

            return Path.Combine(path, FolderNames.GameData);
        }

        public static void WriteJson(string folderName, string fileName, object obj)
        {
            string dirPath = Path.Combine(DirPath, folderName);
            Directory.CreateDirectory(dirPath);
            File.WriteAllText(Path.Combine(dirPath, fileName + ".json"), JsonSerializer.Serialize(obj, _toJsonOption));
        }

        public static string? ReadJson(string folderName, string fileName)
        {
            string dirPath = Path.Combine(DirPath, folderName);
            try
            {
                return File.ReadAllText(Path.Combine(dirPath, fileName + ".json"));
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
            {
                return null;
            }
        }

        public static T? ReadJson<T>(string folderName, string fileName)
        {
            string? data = ReadJson(folderName, fileName);
            return data != null ? JsonSerializer.Deserialize<T>(data) : default;
        }

        public static IEnumerable<string> ReadAllJson(string folderName)
        {
            string dirPath = Path.Combine(DirPath, folderName);
            
            if (!Directory.Exists(dirPath))
                throw new DirectoryNotFoundException();

            foreach (string file in Directory.EnumerateFiles(dirPath, "*.json"))
            {
                yield return File.ReadAllText(file);
            }
        }
    }
}