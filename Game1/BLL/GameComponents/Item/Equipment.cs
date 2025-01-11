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
    public int BonusDEF { get; set; } = 0;
    public int BonusHP { get; set; } = 0;
    public int BonusMP { get; set; } = 0;

    public Equipment() {}

    // public Equipment(string name, int atk, int hp, int mp, ItemRarity rarity = ItemRarity.Common, EquipType type = EquipType.Weapon, int price = 0)
    //     : base(name, rarity, price)
    // {
    //     Type = type;
    //     BonusATK = atk;
    //     BonusHP = hp;
    //     BonusMP = mp;

    //     ID = IDTracker[(int) Rarity]++;
    //     Price = Price * (100 + EquipMultiplier) / 100;
    // }

    public Equipment(Equipment other) : base(other.Name, other.Rarity, other.Price)
    {
        ID = other.ID;
        Type = other.Type;
        BonusATK = other.BonusATK;
        BonusDEF = other.BonusDEF;
        BonusHP = other.BonusHP;
        BonusMP = other.BonusMP;
    }

    public override void Print()
    {
        base.Print();
        Console.Write($"| {Type} |");

        if (BonusATK != 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($" [{(BonusATK > 0 ? "" : "-")}{BonusATK} ATK]");
        }

        if (BonusDEF != 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($" [{(BonusDEF > 0 ? "" : "-")}{BonusDEF} DEF]");
        }

        if (BonusHP != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($" [{(BonusHP > 0 ? "" : "-")}{BonusHP} HP]");
        }

        if (BonusMP != 0)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($" [{(BonusMP > 0 ? "" : "-")}{BonusMP} MP]");
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
        Console.Write($"| {Type, -6} |");

        if (BonusATK != 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($" [{(BonusATK > 0 ? "" : "-")}{BonusATK}]");
        }

        if (BonusDEF != 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($" [{(BonusDEF > 0 ? "" : "-")}{BonusDEF}]");
        }

        if (BonusHP != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($" [{(BonusHP > 0 ? "" : "-")}{BonusHP}]");
        }

        if (BonusMP != 0)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($" [{(BonusMP > 0 ? "" : "-")}{BonusMP}]");
        }
        
        Console.ResetColor();
        Console.WriteLine("    ");
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