enum MonsterType
{
    Normal, Elite, Boss
}

[Serializable]
class Monster : Entity
{
    public MonsterType Type { get; set; }
    public List<Item> Rewards { get; set; } = [];
    public double StatMultiplier { get; set; } = 1;

    public override void Print()
    {
        int barLen = Type switch
        {
            MonsterType.Elite => UINumbers.EliteBarLen,
            MonsterType.Boss => UINumbers.BossBarLen,
            _ => UINumbers.MonsterBarLen
        };
        
        Console.Write($" {Name,-UINumbers.NameLen} | ATK: {Attack,-3} ");
        UIHandler.DrawBar(Health, MaxHealth, true, barLen, ConsoleColor.Red);
    }
}