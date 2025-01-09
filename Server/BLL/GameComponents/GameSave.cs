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
}