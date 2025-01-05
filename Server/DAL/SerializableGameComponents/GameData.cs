using System.Text.Json;

[Serializable]
class GameData
{
    public int Seed { get; set; } = 42;
    public GameProgress Progress { get; set; } = new();
    public Player Player { get; set; } = Player.DefaultPlayer();
    public TimeSpan SavedTime { get; set; } = new(0);

    public GameData() {}

    public string ToJson()
        => JsonSerializer.Serialize(this);
}