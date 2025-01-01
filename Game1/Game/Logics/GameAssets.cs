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
        new("Slime", 10, 100, 1),
        new("Rat", 40, 100, 1),
        new("Mother Slime", 15, 100, 1, MonsterType.Elite),
        new("King Slime", 20, 100, 1, MonsterType.Boss),
        new("Fish", 10, 100, 2),
        new("Killer Whale", 10, 100, 2, MonsterType.Elite),
        new("Poisedon", 10, 100, 2, MonsterType.Boss),
        new("Skeleton", 10, 100, 3),
        new("Gravedigger", 10, 100, 3, MonsterType.Elite),
        new("Undead king", 10, 100, 3, MonsterType.Boss),
    ];

    public static List<Monster> NormalMonsterList = MonsterList.FindAll(monster => monster.Type == MonsterType.Normal);
    public static List<Monster> EliteMonsterList = MonsterList.FindAll(monster => monster.Type == MonsterType.Elite);
    public static List<Monster> BossMonsterList = MonsterList.FindAll(monster => monster.Type == MonsterType.Boss);

    public static Equipment? GetEquipment(string name)
        => EquipList.Find(equip => equip.Name.Equals(name));

    public static Monster? GetMonster(string name)
        => MonsterList.Find(monster => monster.Name.Equals(name));
}