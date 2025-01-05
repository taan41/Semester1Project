using System.Text.Json;

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

    public Equipment(string name, int atk, int hp, int mp, ItemRarity rarity = ItemRarity.Common, EquipType type = EquipType.Weapon, int price = 0)
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
    public override string ToJson()
        => JsonSerializer.Serialize(this);
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