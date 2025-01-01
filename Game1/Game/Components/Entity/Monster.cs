enum MonsterType
{
    Normal, Elite, Boss
}

[Serializable]
class Monster : Entity
{
    public MonsterType Type { get; set; }
    public int Power { get; set; } = 1;
    // public double StatMultiplier { get; set; } = 1;

    public Monster() {}

    public Monster(string name, int atk, int hp, MonsterType type = MonsterType.Normal) : base(name, atk, hp, 0)
    {
        Type = type;
        Power = ATK * 5 + MaxHP;
    }

    public Monster(Monster other, int? targetPower = null) : base(other.Name, other.ATK, other.HP, 0)
    {
        Type = other.Type;
        Power = other.Power;
        if (targetPower != null)
            ScaleStat((int) targetPower);
    }

    public void ScaleStat(int targetPower)
    {
        double multiplier = (double) targetPower / Power;
        ATK = (int) (ATK * multiplier);
        MaxHP = (int) (MaxHP * multiplier);
        HP = (int) (HP * multiplier);
        Power = ATK * 5 + MaxHP;
    }

    public override void Print()
    {
        int barLen = Type switch
        {
            MonsterType.Elite => UIHelper.UIConstants.EliteBarLen,
            MonsterType.Boss => UIHelper.UIConstants.BossBarLen,
            _ => UIHelper.UIConstants.MonsterBarLen
        };
        
        base.Print();
        Console.Write($"| ATK: {ATK,-3} ");
        UIHelper.UIMisc.DrawBar(HP, MaxHP, true, barLen, ConsoleColor.Red);
    }
}