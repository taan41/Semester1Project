using System.Text.Json;
using System.Text.Json.Serialization;

enum MonsterType
{
    Normal, Elite, Boss
}

[Serializable]
class Monster : Entity
{
    public int ID { get; set; }
    public MonsterType Type { get; set; }
    public int Floor { get; set; } = 1;
    
    [JsonIgnore]
    public int Power { get => ATK * 5 + MaxHP; }

    public Monster() {}

    // public Monster(string name, int atk, int def, int hp, int floor = 1, MonsterType type = MonsterType.Normal) : base(name, atk, def, hp, 0)
    // {
    //     Type = type;
    //     Floor = floor;
    // }

    public Monster(Monster other) : base(other.Name, other.ATK, other.DEF, other.HP, 0)
    {
        ID = other.ID;
        Type = other.Type;
        Floor = other.Floor;
    }

    public override string ToJson()
        => JsonSerializer.Serialize(this);
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