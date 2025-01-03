using System.Text.Json;

class AssetManager
{
    public const string DirPath = FileHelper.FileConstants.DirPath + @"Assets\";
    public const string VersionFile = "Version.json";
    public const string EquipmentsFile = "Equipments.json";
    public const string SkillsFile = "Skills.json";
    public const string MonstersFile = "Monsters.json";

    public string Version = "";

    public Dictionary<int, Equipment> Equipments = [];
    public Dictionary<int, Skill> Skills = [];
    public Dictionary<int, Monster> Monsters = [];

    public AssetManager(bool setup = true)
    {
        if (setup) Setup();
    }

    public void SerializeToFile()
    {
        Directory.CreateDirectory(DirPath);
        var jsonOption = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(DirPath + VersionFile, JsonSerializer.Serialize(Version, jsonOption));
        File.WriteAllText(DirPath + EquipmentsFile, JsonSerializer.Serialize(Equipments, jsonOption));
        File.WriteAllText(DirPath + SkillsFile, JsonSerializer.Serialize(Skills, jsonOption));
        File.WriteAllText(DirPath + MonstersFile, JsonSerializer.Serialize(Monsters, jsonOption));
    }
    
    private void Setup()
    {
        Version = JsonSerializer.Deserialize<string>(File.ReadAllText(DirPath + VersionFile)) ?? "";
        Equipments = JsonSerializer.Deserialize<Dictionary<int, Equipment>>(File.ReadAllText(DirPath + EquipmentsFile)) ?? [];
        Skills = JsonSerializer.Deserialize<Dictionary<int, Skill>>(File.ReadAllText(DirPath + SkillsFile)) ?? [];
        Monsters = JsonSerializer.Deserialize<Dictionary<int, Monster>>(File.ReadAllText(DirPath + MonstersFile)) ?? [];

        foreach(int id in Equipments.Keys)
            if (id >= Equipment.IDTracker[id / 100]) Equipment.IDTracker[id / 100] = id + 1;

        foreach(int id in Skills.Keys)
            if (id >= Skill.IDTracker[id / 100]) Skill.IDTracker[id / 100] = id + 1;

        foreach(int id in Monsters.Keys)
            if (id >= Monster.IDTracker[id / 1000][id % 1000 / 100]) Monster.IDTracker[id / 1000][id % 1000 / 100] = id + 1;
    }
}