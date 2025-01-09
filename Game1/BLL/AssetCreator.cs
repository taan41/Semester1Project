using static System.Console;

class AssetCreator
{
    public static void Start()
    {
        while (true)
        {
            Clear();
            WriteLine("Asset Manager");
            WriteLine("0 = exit, 1 = monster, 2 = equipment, 3 = skill, 4 = save & exit");
            Write("enter: ");

            switch(ReadLine())
            {
                case "1":
                    Monsters();
                    break;

                case "2":
                    Equipments();
                    break;

                case "3":
                    Skills();
                    break;

                case "4":
                    AssetManager.SerializeToFile();
                    return;

                case "0": return;

                default: continue;
            }
        }
    }

    private static void Monsters()
    {

        while (true)
        {
            Clear();
            WriteLine("Asset Manager");
            WriteLine("Monster");
            WriteLine("0 = exit, 1 = create, 2 = show list, 3 = clear list");
            Write("enter: ");

            try
            {
                switch(ReadLine())
                {
                    case "1":
                        Write("name (25 max): ");
                        string? name = ReadLine();
                        if (string.IsNullOrWhiteSpace(name) || name.Length > GameUIHelper.UIConstants.NameLen) continue;

                        Write("type (0 = normal, 1 = elite, 2 = boss): ");
                        MonsterType type = (MonsterType) Convert.ToInt32(ReadLine());

                        Write("floor: ");
                        int floor = Convert.ToInt32(ReadLine());

                        Write("atk: ");
                        int atk = Convert.ToInt32(ReadLine());

                        Write("hp: ");
                        int hp = Convert.ToInt32(ReadLine());

                        Monster monster = new(name, atk, hp, floor, type);
                        AssetManager.Monsters[monster.ID] = monster;
                        WriteLine("Added");
                        ReadKey(true);
                        continue;

                    case "2":
                        AssetManager.Monsters.Values.ToList().ForEach(monster => WriteLine($"{monster.ID:d4} {monster.Name, -25} {monster.Type, -6} floor: {monster.Floor}, atk: {monster.ATK}, hp: {monster.HP}"));
                        ReadKey(true);
                        continue;

                    case "3":
                        AssetManager.Monsters.Clear();
                        continue;

                    case "0":
                        return;

                    default: continue;
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }
    }

    private static void Equipments()
    {

        while (true)
        {
            Clear();
            WriteLine("Asset Manager");
            WriteLine("Equipment");
            WriteLine("0 = exit, 1 = create, 2 = show list, 3 = clear list");
            Write("enter: ");

            try
            {
                switch(ReadLine())
                {
                    case "1":
                        Write("name (25 max): ");
                        string? name = ReadLine();
                        if (string.IsNullOrWhiteSpace(name) || name.Length > GameUIHelper.UIConstants.NameLen) continue;

                        Write("type (0 = weapon, 1 = armor, 2 = ring): ");
                        EquipType type = (EquipType) Convert.ToInt32(ReadLine());

                        Write("rarity (0, 3: Common, Rare, Epic, Legendary): ");
                        ItemRarity rarity = (ItemRarity) Convert.ToInt32(ReadLine());
                        int point = rarity switch
                        {
                            ItemRarity.Rare => 6,
                            ItemRarity.Epic => 10,
                            ItemRarity.Legendary => 15,
                            _ => 3
                        };
                        WriteLine($"Recommend stat points : {point} (1 point = 2 atk = 10 hp = 5 mp)");

                        WriteLine($"Point left: {point}");
                        Write("atk point (1 pt = 2 atk): ");
                        int atk = Convert.ToInt32(ReadLine());
                        point -= atk;

                        WriteLine($"Point left: {point}");
                        Write("hp point (1 pt = 10 hp): ");
                        int hpPt = Convert.ToInt32(ReadLine());
                        point -= hpPt;

                        WriteLine($"Point left: {point}");
                        Write("mp point (1 pt = 5 mp): ");
                        int mpPt = Convert.ToInt32(ReadLine());
                        point -= mpPt;

                        Equipment equipment = new(name, atk * 2, hpPt * 10, mpPt * 5, rarity, type);
                        AssetManager.Equipments[equipment.ID] = equipment;
                        WriteLine("Added");
                        ReadKey(true);
                        continue;

                    case "2":
                        AssetManager.Equipments.Values.ToList().ForEach(equip => WriteLine($"{equip.ID:d3} {equip.Name, -25} {equip.Rarity, -10} {equip.Type, -6} {equip.BonusATK} atk, {equip.BonusHP} hp, {equip.BonusMP} mp, {equip.Price} g"));
                        ReadKey(true);
                        continue;

                    case "3":
                        AssetManager.Equipments.Clear();
                        continue;

                    case "0":
                        return;

                    default: continue;
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }
    }

    private static void Skills()
    {

        while (true)
        {
            Clear();
            WriteLine("Asset Manager");
            WriteLine("Equipment");
            WriteLine("0 = exit, 1 = create, 2 = show list, 3 = clear list");
            Write("enter: ");

            try
            {
                switch(ReadLine())
                {
                    case "1":
                        Write("name (25 max): ");
                        string? name = ReadLine();
                        if (string.IsNullOrWhiteSpace(name) || name.Length > GameUIHelper.UIConstants.NameLen) continue;

                        Write("type (0, 2: Single, Random, All): ");
                        SkillType type = (SkillType) Convert.ToInt32(ReadLine());

                        Write("rarity (0, 3: Common, Rare, Epic, Legendary): ");
                        ItemRarity rarity = (ItemRarity) Convert.ToInt32(ReadLine());
                        int multiplier = rarity switch
                        {
                            ItemRarity.Rare => 200,
                            ItemRarity.Epic => 300,
                            ItemRarity.Legendary => 450,
                            _ => 100
                        };
                        WriteLine($"Multiplier based on rairty : {multiplier}%");
                        WriteLine("1 mp = 1 heal = 2 damage");

                        Write("mp cost: ");
                        int mp = Convert.ToInt32(ReadLine());

                        WriteLine($"MP left: {mp}");
                        Write("dmg point (1 pt = 1 mp = 2 dmg): ");
                        int dmgPt = Convert.ToInt32(ReadLine());

                        WriteLine($"MP left: {mp - dmgPt}");
                        Write("heal point (1 pt = 1 mp = 1 heal): ");
                        int healPt = Convert.ToInt32(ReadLine());

                        Skill skill = new(name, dmgPt * 2 * multiplier / 100, healPt * multiplier / 100, mp, rarity, type);
                        AssetManager.Skills[skill.ID] = skill;
                        WriteLine("Added");
                        ReadKey(true);
                        continue;

                    case "2":
                        AssetManager.Skills.Values.ToList().ForEach(skill => WriteLine($"{skill.ID:d3} {skill.Name, -25} {skill.Rarity, -10} {skill.Type, -6} {skill.Damage} dmg, {skill.Heal} heal, {skill.MPCost} mp, {skill.Price} g"));
                        ReadKey(true);
                        continue;

                    case "3":
                        AssetManager.Skills.Clear();
                        continue;

                    case "0":
                        return;

                    default: continue;
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }
    }
}