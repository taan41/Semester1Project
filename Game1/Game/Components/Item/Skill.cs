enum TargetType
{
    Single, Random, All
}

[Serializable]
class Skill : Item
{
    public TargetType Type { get; set; }
    public int Damage { get; set; } = 0;
    public int Heal { get; set; } = 0;
    public int MPCost { get; set; } = 0;

    public Skill() {}

    public Skill(string name, int dmg, int heal, int mpcost, ItemRarity rarity = ItemRarity.Common, TargetType type = TargetType.Single, int price = -1)
        : base(name, rarity, price)
    {
        Damage = dmg;
        Heal = heal;
        MPCost = mpcost;
        Type = type;
        Price *= (100 + SkillMultiplier) / 100;
    }

    public Skill(Skill other) : base(other.Name, other.Rarity, other.Price)
    {
        Damage = other.Damage;
        Heal = other.Heal;
        MPCost = other.MPCost;
        Type = other.Type;
    }

    public override void Print()
    {
        base.Print();
        Console.Write($"| {Type} |");

        if (Damage > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($" [ ▲ {Damage} ]");
        }

        if (Heal > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($" [ + {Heal} ]");
        }

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($" ({MPCost} MP)  ");
        Console.ResetColor();
    }

    public override void PrintPrice(bool buying)
    {
        base.PrintPrice(buying);
        Console.Write($"| Skill | {Type} |");

        if (Damage > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($" [{Damage}]");
        }

        if (Heal > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($" [{Heal}]");
        }

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($" ({MPCost} MP)  ");
        Console.ResetColor();
    }
}

class SkillComparer : IComparer<Skill>
{
    public int Compare(Skill? x, Skill? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return 1;
        if (y == null) return -1;
        
        int rarityComparison = y.Rarity.CompareTo(x.Rarity);
        if (rarityComparison != 0) return rarityComparison;

        int typeComparison = x.Type.CompareTo(y.Type);
        if (typeComparison != 0) return typeComparison;

        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}