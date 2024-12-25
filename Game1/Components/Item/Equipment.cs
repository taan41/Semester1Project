enum EquipType
{
    Weapon, Armor, Ring
}

[Serializable]
class Equipment : Item
{
    public EquipType Type { get; set; }
    public int BonusAtk { get; set; }
    public int BonusMaxHP { get; set; }
    public int BonusMaxMP { get; set; }
}