class AssetManager
{
    public static Dictionary<int, Equipment> Equipments = [];
    public static Dictionary<int, Skill> Skills = [];
    public static Dictionary<int, Monster> Monsters = [];
    
    static AssetManager()
        => LoadAsset();

    public static void LoadAsset()
    {
        FileManager.LoadAsset(out Equipments, out Skills, out Monsters);

        foreach(int id in Equipments.Keys)
            if (id >= Equipment.IDTracker[id / 100]) Equipment.IDTracker[id / 100] = id + 1;

        foreach(int id in Skills.Keys)
            if (id >= Skill.IDTracker[id / 100]) Skill.IDTracker[id / 100] = id + 1;

        foreach(int id in Monsters.Keys)
            if (id >= Monster.IDTracker[id / 1000][id % 1000 / 100]) Monster.IDTracker[id / 1000][id % 1000 / 100] = id + 1;
    }

    public static void SaveAsset()
        => FileManager.SaveAsset();
}