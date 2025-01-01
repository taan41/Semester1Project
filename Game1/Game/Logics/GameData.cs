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
        for (int i = 0; i < 15; i++)
            Player.AddSkill(new(GameAssets.SkillList.ElementAt(0)){ Damage = i, Rarity = (ItemRarity) (i % 4) });
    }
}