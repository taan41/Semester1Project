using System.Text.Json;

[Serializable]
class Player : Entity
{
    public Equipment? EquippedWeapon { get; set; } = null;
    public Equipment? EquippedArmor { get; set; } = null;
    public Equipment? EquippedRing { get; set; } = null;
    public List<Equipment> EquipInventory { get; set; } = [];
    public List<Skill> Skills { get; set; } = [];
    public List<Skill> SkillInventory { get; set; } = [];
    public Gold PlayerGold { get; set; } = new(0);

    public Player() {}

    public Player(string name, int atk, int hp, int mp, int goldQuantity) : base(name, atk, hp, mp)
    {
        PlayerGold = new(goldQuantity);
    }

    public static Player DefaultPlayer() 
        => new("Player", 3, 25, 10, 100);

    public override string ToJson()
        => JsonSerializer.Serialize(this);
}