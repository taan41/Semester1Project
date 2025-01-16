using System.Text.Json;

using static FileManager.FileConstants;

class FileManager
{
    public static class FileConstants
    {
        public const string DirPath = "GameData";

        public const string SaveFolder = "GameSaves";

        public const string AssetFolder = "GameAssets";
        public const string VersionFile = "Version.json";
        public const string EquipmentsFile = "Equipments.json";
        public const string SkillsFile = "Skills.json";
        public const string MonstersFile = "Monsters.json";
    }

    private static readonly JsonSerializerOptions _fromJsonOption = new()
    {
        IncludeFields = true
    };
    private static readonly JsonSerializerOptions _toJsonOption = new()
    {
        WriteIndented = true
    };
    
    public static void LoadSaves(out List<GameSave> loadedSaves, out string? error)
    {
        loadedSaves = [];
        error = null;
        string dataDirPath = Path.Combine(DirPath, SaveFolder);

        try
        {
            foreach (string file in Directory.EnumerateFiles(dataDirPath, "*.json"))
            {
                var loadedSave = JsonSerializer.Deserialize<GameSave>(File.ReadAllText(file), _fromJsonOption);

                if (loadedSave != null)
                    loadedSaves.Add(loadedSave);
                else
                    throw new JsonException();
            }
        }
        catch (JsonException)
        {
            error = "Corrupted saves";
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
        {
            error = "No save found";
        }
    }

    public static void WriteSave(GameSave gameSave)
    {
        string dataDirPath = Path.Combine(DirPath, SaveFolder);
        Directory.CreateDirectory(dataDirPath);
        File.WriteAllText(Path.Combine(dataDirPath, gameSave.Name + ".json"), JsonSerializer.Serialize(gameSave, _toJsonOption));
    }

    public static void LoadAsset(out Dictionary<int, Equipment> equipments, out Dictionary<int, Skill> skills, out Dictionary<int, Monster> monsters)
    {
        equipments = [];
        skills = [];
        monsters = [];
        
        string assetDirPath = Path.Combine(DirPath, AssetFolder);

        try
        {
            equipments = JsonSerializer.Deserialize<Dictionary<int, Equipment>>(File.ReadAllText(Path.Combine(assetDirPath, EquipmentsFile))) ?? [];
            skills = JsonSerializer.Deserialize<Dictionary<int, Skill>>(File.ReadAllText(Path.Combine(assetDirPath, SkillsFile))) ?? [];
            monsters = JsonSerializer.Deserialize<Dictionary<int, Monster>>(File.ReadAllText(Path.Combine(assetDirPath, MonstersFile))) ?? [];
        }
        catch (Exception ex) when (ex is JsonException || ex is DirectoryNotFoundException || ex is FileNotFoundException)
        { /* Ignores */ }
    }

    public static void SaveAsset()
    {
        string assetDirPath = Path.Combine(DirPath, AssetFolder);
        Directory.CreateDirectory(assetDirPath);

        File.WriteAllText(Path.Combine(assetDirPath, EquipmentsFile), JsonSerializer.Serialize(AssetManager.Equipments, _toJsonOption));
        File.WriteAllText(Path.Combine(assetDirPath, SkillsFile), JsonSerializer.Serialize(AssetManager.Skills, _toJsonOption));
        File.WriteAllText(Path.Combine(assetDirPath, MonstersFile), JsonSerializer.Serialize(AssetManager.Monsters, _toJsonOption));
    }
}