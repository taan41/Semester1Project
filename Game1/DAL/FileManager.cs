using System.Text.Json;

using static FileManager.FileConstants;

class FileManager
{
    public static class FileConstants
    {
        public const string DirPath = "Persistence";

        public const string DataFolder = "GameSaves";
        public const string DataFile = "GameData.json";

        public const string AssetFolder = "GameAssets";
        public const string VersionFile = "Version.json";
        public const string EquipmentsFile = "Equipments.json";
        public const string SkillsFile = "Skills.json";
        public const string MonstersFile = "Monsters.json";
    }

    
    private static  readonly JsonSerializerOptions _jsonOption = new(){ WriteIndented = true };
    
    public static bool LoadData(out GameData? loadedData)
    {
        loadedData = null;
        string dataDirPath = Path.Combine(DirPath, DataFolder);

        try
        {
            loadedData = JsonSerializer.Deserialize<GameData>(File.ReadAllText(Path.Combine(dataDirPath, DataFile)));
        }
        catch (JsonException)
        {
            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
        {
            return false;
        }

        return true;
    }

    public static void SaveData(GameData gameData)
    {
        string dataDirPath = Path.Combine(DirPath, DataFolder);
        Directory.CreateDirectory(dataDirPath);
        File.WriteAllText(Path.Combine(dataDirPath, DataFile), JsonSerializer.Serialize(gameData, _jsonOption));
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

        File.WriteAllText(Path.Combine(assetDirPath, EquipmentsFile), JsonSerializer.Serialize(AssetManager.Equipments, _jsonOption));
        File.WriteAllText(Path.Combine(assetDirPath, SkillsFile), JsonSerializer.Serialize(AssetManager.Skills, _jsonOption));
        File.WriteAllText(Path.Combine(assetDirPath, MonstersFile), JsonSerializer.Serialize(AssetManager.Monsters, _jsonOption));
    }
}