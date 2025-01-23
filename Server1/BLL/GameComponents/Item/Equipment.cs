using System.Text.Json;

enum EquipType
{
    Weapon, Armor, Ring
}

[Serializable]
class Equipment : Item
{
    // public static readonly int[] IDTracker = [1, 101, 201, 301];

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
    //     Price = Price * (100 + EquipPriceMultiplier) / 100;
    // }

    public Equipment(Equipment other) : base(other.Name, other.Rarity, other.Price)
    {
        ID = other.ID;
        Type = other.Type;
        BonusATK = other.BonusATK;
        BonusHP = other.BonusHP;
        BonusMP = other.BonusMP;
    }
    
    public override string ToJson()
        => JsonSerializer.Serialize(this);

    public static int CalcPrice(ItemRarity rarity)
        => BasePrice * (100 + (int) rarity * RarityPriceMultiplier + EquipPriceMultiplier) / 100;
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