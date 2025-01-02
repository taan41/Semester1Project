using System.Text.Json.Serialization;

enum MonsterType
{
    Normal, Elite, Boss
}

[Serializable]
class Monster : Entity
{
    public MonsterType Type { get; set; }
    public int Floor { get; set; } = 1;
    
    [JsonIgnore]
    public int Power { get; set; } = 1;

    public Monster() {}

    public Monster(string name, int atk, int hp, int floor = 1, MonsterType type = MonsterType.Normal) : base(name, atk, hp, 0)
    {
        Type = type;
        Floor = floor;
        Power = ATK * 5 + MaxHP;
    }

    public Monster(Monster other, int? targetPower = null) : base(other.Name, other.ATK, other.HP, 0)
    {
        Type = other.Type;
        Floor = other.Floor;
        Power = ATK * 5 + MaxHP;
        
        if (targetPower != null)
            ScaleStat((int) targetPower);
    }

    public void ScaleStat(int targetPower)
    {
        ATK = ATK * targetPower / Power;
        MaxHP = MaxHP * targetPower / Power;
        HP = HP * targetPower / Power;
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

class MonsterComparer : IComparer<Monster>
{
    public int Compare(Monster? x, Monster? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return 1;
        if (y == null) return -1;

        int floorComparison = x.Floor.CompareTo(y.Floor);
        if (floorComparison != 0) return floorComparison;

        int typeComparison = x.Type.CompareTo(y.Type);
        if (typeComparison != 0) return typeComparison;

        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}