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
            MonsterType.Elite => UIHandler.Numbers.EliteBarLen,
            MonsterType.Boss => UIHandler.Numbers.BossBarLen,
            _ => UIHandler.Numbers.MonsterBarLen
        };
        
        base.Print();
        Console.Write($"| ATK: {Attack,-3} ");
        UIHandler.Misc.DrawBar(Health, MaxHealth, true, barLen, ConsoleColor.Red);
    }
}