using System.Text.Json;
using System.Text.Json.Serialization;

[Serializable]
class AssetMetadata
{
    private const string FileName = "AssetMetadata.json";
    private const string DirPath = "Metadata";

    private static readonly JsonSerializerOptions toJsonOption = new()
    {
        WriteIndented = true
    };

    public static AssetMetadata Instance { get; } = new(true);
    
    [JsonIgnore]
    public int[] EquipIDTracker { get; set; } = new int[Enum.GetValues(typeof(ItemRarity)).Length];
    [JsonIgnore]
    public int[] SkillIDTracker { get; set; } = new int[Enum.GetValues(typeof(ItemRarity)).Length];
    [JsonIgnore]
    public int[][] MonsterIDTracker { get; set; } = new int[GameProgress.MaxFloor][];

    public int[] EquipRarityPoint { get; set; } = new int[Enum.GetValues(typeof(ItemRarity)).Length];
    public int[] EquipPointMultiplier { get; set; } = new int[4];

    public int[] SkillRarityMultiplier { get; set; } = new int[Enum.GetValues(typeof(ItemRarity)).Length];
    public double[] SkillMPMultiplier { get; set; } = new double[2];
    public double[] SkillTypeDmgMultiplier { get; set; } = new double[Enum.GetValues(typeof(SkillType)).Length];

    public AssetMetadata() : this(false) { }

    private AssetMetadata(bool load)
    {
        for (int i = 0; i < EquipIDTracker.Length; i++)
        {
            EquipIDTracker[i] = i * 100 + 1;
            SkillIDTracker[i] = i * 100 + 1;
        }
        
        for (int i = 0; i < MonsterIDTracker.Length; i++)
        {
            MonsterIDTracker[i] = new int[Enum.GetValues(typeof(MonsterType)).Length];

            for (int j = 0; j < MonsterIDTracker[i].Length; j++)
            {
                MonsterIDTracker[i][j] = i * 1000 + j * 100 + 1;
            }
        }

        if (load) Load();
    }

    public void Save()
    {
        Directory.CreateDirectory(DirPath);
        File.WriteAllText(Path.Combine(DirPath, FileName), JsonSerializer.Serialize(this, toJsonOption));
    }

    public void Load()
    {
        AssetMetadata? metadata = null;

        string path = Path.Combine(DirPath, FileName);
        if (!File.Exists(path)) return;

        try
        {
            metadata = JsonSerializer.Deserialize<AssetMetadata>(File.ReadAllText(path));
        }
        catch (JsonException) { }

        if (metadata == null) return;

        EquipIDTracker = metadata.EquipIDTracker;
        SkillIDTracker = metadata.SkillIDTracker;
        MonsterIDTracker = metadata.MonsterIDTracker;

        EquipRarityPoint = metadata.EquipRarityPoint;
        EquipPointMultiplier = metadata.EquipPointMultiplier;
        
        SkillRarityMultiplier = metadata.SkillRarityMultiplier;
        SkillMPMultiplier = metadata.SkillMPMultiplier;
        SkillTypeDmgMultiplier = metadata.SkillTypeDmgMultiplier;
    }
}
