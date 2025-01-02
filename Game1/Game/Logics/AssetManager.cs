using System.Text.Json;

class AssetManager
{
    public const string DirPath = FileHelper.FileConstants.DirPath + @"Assets\";
    public const string VersionFile = "Version.json";
    public const string EquipmentsFile = "Equipments.json";
    public const string SkillsFile = "Skills.json";
    public const string MonstersFile = "Monsters.json";

    public string Version = "";

    public List<Equipment> Equipments = [];
    public List<Equipment> CommonEquipments = [];
    public List<Equipment> RareEquipments = [];
    public List<Equipment> EpicEquipments = [];
    public List<Equipment> LegendaryEquipments = [];

    public List<Skill> Skills = [];
    public List<Skill> CommonSkills = [];
    public List<Skill> RareSkills = [];
    public List<Skill> EpicSkills = [];
    public List<Skill> LegendarySkills = [];

    public List<Monster> Monsters = [];
    public List<Monster> NormalMonsters = [];
    public List<Monster> EliteMonsters = [];
    public List<Monster> BossMonsters = [];

    public AssetManager(bool setup = true)
    {
        if (setup) Setup();
    }

    public Equipment? GetEquipment(string name)
        => Equipments.Find(equip => equip.Name.Equals(name));

    public Monster? GetMonster(string name)
        => Monsters.Find(monster => monster.Name.Equals(name));

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
        Equipments = JsonSerializer.Deserialize<List<Equipment>>(File.ReadAllText(DirPath + EquipmentsFile)) ?? [];
        Skills = JsonSerializer.Deserialize<List<Skill>>(File.ReadAllText(DirPath + SkillsFile)) ?? [];
        Monsters = JsonSerializer.Deserialize<List<Monster>>(File.ReadAllText(DirPath + MonstersFile)) ?? [];
        
        CommonEquipments.AddRange(Equipments.FindAll(equip => equip.Rarity == ItemRarity.Common));
        RareEquipments.AddRange(Equipments.FindAll(equip => equip.Rarity == ItemRarity.Rare));
        EpicEquipments.AddRange(Equipments.FindAll(equip => equip.Rarity == ItemRarity.Epic));
        LegendaryEquipments.AddRange(Equipments.FindAll(equip => equip.Rarity == ItemRarity.Legendary));
        
        CommonSkills.AddRange(Skills.FindAll(skill => skill.Rarity == ItemRarity.Common));
        RareSkills.AddRange(Skills.FindAll(skill => skill.Rarity == ItemRarity.Rare));
        EpicSkills.AddRange(Skills.FindAll(skill => skill.Rarity == ItemRarity.Epic));
        LegendarySkills.AddRange(Skills.FindAll(skill => skill.Rarity == ItemRarity.Legendary));

        NormalMonsters.AddRange(Monsters.FindAll(monster => monster.Type == MonsterType.Normal));
        EliteMonsters.AddRange(Monsters.FindAll(monster => monster.Type == MonsterType.Elite));
        BossMonsters.AddRange(Monsters.FindAll(monster => monster.Type == MonsterType.Boss));
    }
}