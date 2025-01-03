enum EquipType
{
    Weapon, Armor, Ring
}

[Serializable]
class Equipment : Item
{
    public static readonly int[] IDTracker = [1, 101, 201, 301];

    public EquipType Type { get; set; } = EquipType.Weapon;
    public int BonusATK { get; set; } = 0;
    public int BonusMaxHP { get; set; } = 0;
    public int BonusMaxMP { get; set; } = 0;

    public Equipment() {}

    public Equipment(string name, int atk, int hp, int mp, ItemRarity rarity = ItemRarity.Common, EquipType type = EquipType.Weapon, int price = -1)
        : base(name, rarity, price)
    {
        Type = type;
        BonusATK = atk;
        BonusMaxHP = hp;
        BonusMaxMP = mp;

        ID = IDTracker[(int) Rarity]++;
        Price = Price * (100 + EquipMultiplier) / 100;
    }

    public Equipment(Equipment other) : base(other.Name, other.Rarity, other.Price)
    {
        BonusATK = other.BonusATK;
        BonusMaxHP = other.BonusMaxHP;
        BonusMaxMP = other.BonusMaxMP;
        Type = other.Type;
        ID = other.ID;
    }

    public override void Print()
    {
        base.Print();
        Console.Write($"| {Type} |");

        if (BonusATK != 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($" [ {(BonusATK > 0 ? "+" : "-")}{BonusATK} ATK]");
        }

        if (BonusMaxHP != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($" [ {(BonusMaxHP > 0 ? "+" : "-")}{BonusMaxHP} HP]");
        }

        if (BonusMaxMP != 0)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($" [ {(BonusMaxMP > 0 ? "+" : "-")}{BonusMaxMP} MP]");
        }
        
        Console.ResetColor();
        Console.WriteLine("  ");
    }

    public override void PrintPrice(bool buying)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($" ({Price * (buying ? 100 : SellPricePercentage) / 100} G)");
        Console.ResetColor();

        base.Print();
        Console.Write($"| {Type} |");

        if (BonusATK != 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($" [{(BonusATK > 0 ? "+" : "-")}{BonusATK}]");
        }

        if (BonusMaxHP != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($" [{(BonusMaxHP > 0 ? "+" : "-")}{BonusMaxHP}]");
        }

        if (BonusMaxMP != 0)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($" [{(BonusMaxMP > 0 ? "+" : "-")}{BonusMaxMP}]");
        }
        
        Console.ResetColor();
        Console.WriteLine("  ");
    }
}

class EquipmentComparer : IComparer<Equipment>
{
    public int Compare(Equipment? x, Equipment? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return 1;
        if (y == null) return -1;
        
        int rarityComparison = y.Rarity.CompareTo(x.Rarity);
        if (rarityComparison != 0) return rarityComparison;

        int typeComparison = x.Type.CompareTo(y.Type);
        if (typeComparison != 0) return typeComparison;

        int idComparison = x.ID.CompareTo(y.ID);
        if (idComparison != 0) return idComparison;

        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}