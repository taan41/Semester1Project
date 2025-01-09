[Serializable]
class GameSave : Component
{
    public DateTime SaveTime { get; set; } = DateTime.Now;
    public GameData GameData { get; set; } = new();

    public GameSave() {}

    public GameSave(GameData gameData, string name = "New Save", DateTime? saveTime = null) : base(name)
    {
        GameData = gameData;
        SaveTime = saveTime ?? DateTime.Now;
    }
    
    public void Save(string saveAs)
    {
        Name = saveAs;
        GameData.SaveTime();
        FileManager.WriteSave(this);
    }
    
    public static void Load(out List<GameSave> loadedSaves, out string? error)
        => FileManager.LoadSaves(out loadedSaves, out error);

    public override void Print()
    {
        Console.WriteLine($" {Name} | Saved at: {SaveTime:G}  ");
        Console.WriteLine($" Progress: {GameData.Progress}  ");
    }
}