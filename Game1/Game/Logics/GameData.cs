[Serializable]
class GameData
{
    public int Seed { get; set; } = 42;
    public GameProgress Progress { get; set; }
    public Player Player { get; set; }

    public GameData()
    {
        Progress = new();
        Player = new("Hero", 3, 25, 10, 100);
        Player.AddSkill(new(GameAssets.SkillList.ElementAt(0)));
    }
}