using System.Text.Json;

enum SkillType
{
    Single, Random, All
}

[Serializable]
class Skill : Item
{
    public static readonly int[] IDTracker = [1, 101, 201, 301];
    
    public SkillType Type { get; set; }
    public int Damage { get; set; } = 0;
    public int Heal { get; set; } = 0;
    public int MPCost { get; set; } = 0;

    public Skill() {}

    public Skill(string name, int dmg, int heal, int mpcost, ItemRarity rarity = ItemRarity.Common, SkillType type = SkillType.Single, int price = -1)
        : base(name, rarity, price)
    {
        Type = type;
        Damage = dmg;
        Heal = heal;
        MPCost = mpcost;

        ID = IDTracker[(int) Rarity]++;
        Price = Price * (100 + SkillMultiplier) / 100;
    }

    public Skill(Skill other) : base(other.Name, other.Rarity, other.Price)
    {
        Damage = other.Damage;
        Heal = other.Heal;
        MPCost = other.MPCost;
        Type = other.Type;
        ID = other.ID;
    }
    public override string ToJson()
        => JsonSerializer.Serialize(this);
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

        int idComparison = x.ID.CompareTo(y.ID);
        if (idComparison != 0) return idComparison;

        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
}