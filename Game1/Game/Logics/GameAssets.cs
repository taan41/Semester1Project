class GameAssets
{
    public static List<Equipment> EquipList = [
        new("Starter Sword", 2, 10, 0),
        new("Starter Bow", 3, 0, 0),
        new("Starter Staff", 1, 0, 10),
    ];

    public static List<Skill> SkillList = [
        new("Heal", 0, 5, 5)
    ];

    public static List<Monster> MonsterList = [
        new("Slime", 1, 10),
        new("Rat", 2, 5)
    ];

    public static Equipment? GetEquipment(string name)
        => EquipList.Find(equip => equip.Name.Equals(name));
}