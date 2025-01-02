enum EquipType
{
    Weapon, Armor, Ring
}

[Serializable]
class Equipment : Item
{

    public EquipType Type { get; set; } = EquipType.Weapon;
    public int BonusATK { get; set; } = 0;
    public int BonusMaxHP { get; set; } = 0;
    public int BonusMaxMP { get; set; } = 0;

    public Equipment() {}

    public Equipment(string name, int atk, int hp, int mp, ItemRarity rarity = ItemRarity.Common, EquipType type = EquipType.Weapon)
        : base(name, rarity)
    {
        BonusATK = atk;
        BonusMaxHP = hp;
        BonusMaxMP = mp;
        Type = type;
    }

    public Equipment(Equipment other) : base(other.Name, other.Rarity)
    {
        BonusATK = other.BonusATK;
        BonusMaxHP = other.BonusMaxHP;
        BonusMaxMP = other.BonusMaxMP;
        Type = other.Type;
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

        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}