using System.Text.Json.Serialization;

enum MonsterType
{
    Normal, Elite, Boss
}

[Serializable]
class Monster : Entity
{
    public const int
        PowerATKPercentage = 500, PowerHPPercentage = 100,
        BaseATK = 1, BaseHP = 10;
        
    public readonly static int[][] IDTracker;

    public int ID { get; set; }
    public MonsterType Type { get; set; }
    public int Floor { get; set; } = 1;
    
    [JsonIgnore]
    public int Power { get => (ATK * PowerATKPercentage + MaxHP * PowerHPPercentage) / 100; }

    static Monster()
    {
        IDTracker = new int[GameProgress.MaxFloor][];
        for (int i = 0; i < GameProgress.MaxFloor; i++)
        {
            IDTracker[i] = [i * 1000 + 1, i * 1000 + 101, i * 1000 + 201];
        }
    }

    public Monster() {}

    public Monster(string name, int atk, int hp, int floor = 1, MonsterType type = MonsterType.Normal) : base(name, atk, hp, 0)
    {
        Type = type;
        Floor = floor;
        ID = IDTracker[floor - 1][(int) Type]++;
    }

    public Monster(Monster other, int targetPower = 0) : base(other.Name, other.ATK, other.HP, 0)
    {
        Type = other.Type;
        Floor = other.Floor;
        ID = other.ID;
        
        if (targetPower != 0 && targetPower != Power)
            ScaleStat(targetPower);
    }

    public void ScaleStat(int targetPower)
    {
        int ogPower = Power;
        ATK = ATK * targetPower / ogPower;
        MaxHP = MaxHP * targetPower / ogPower;
        HP = HP * targetPower / ogPower;
    }

    public override void Print()
    {
        int barLen = Type switch
        {
            MonsterType.Elite => GameUIHelper.UIConstants.EliteBarLen,
            MonsterType.Boss => GameUIHelper.UIConstants.BossBarLen,
            _ => GameUIHelper.UIConstants.MonsterBarLen
        };
        
        base.Print();
        Console.Write($"| ATK: {ATK,-3} ");
        GameUIHelper.DrawBar(HP, MaxHP, true, barLen, ConsoleColor.Red);
    }
}

class MonsterComparer : IComparer<Monster>
{
    public int Compare(Monster? x, Monster? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return 1;
        if (y == null) return -1;

        int idComparison = x.ID.CompareTo(y.ID);
        if (idComparison != 0) return idComparison;

        int floorComparison = x.Floor.CompareTo(y.Floor);
        if (floorComparison != 0) return floorComparison;

        int typeComparison = x.Type.CompareTo(y.Type);
        if (typeComparison != 0) return typeComparison;

        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}