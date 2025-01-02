using static System.Console;

class Program
{
    public static void Main()
    {
        try
        {
            CursorVisible = false;
            Game.Start();
            // AssetCreator.Start();
        }
        catch (Exception ex)
        {
            WriteLine(ex);
        }
    }

    private class AssetCreator
    {
        public static void Start()
        {
            AssetManager manager = new();

            while (true)
            {
                Clear();
                WriteLine("Assets manager");
                WriteLine("0 = exit, 1 = monster, 2 = equipment, 3 = skill, 4 = save & exit");
                Write("enter: ");

                switch(ReadLine())
                {
                    case "1":
                        Monsters(manager);
                        break;

                    case "2":
                        Equipments(manager);
                        break;

                    case "3":
                        Skills(manager);
                        break;

                    case "4":
                        manager.SerializeToFile();
                        return;

                    case "0": return;

                    default: continue;
                }
            }
        }

        private static void Monsters(AssetManager manager)
        {

            while (true)
            {
                Clear();
                WriteLine("Assets manager");
                WriteLine("Monster");
                WriteLine("0 = exit, 1 = create, 2 = show list, 3 = clear list");
                Write("enter: ");

                try
                {
                    switch(ReadLine())
                    {
                        case "1":
                            Write("name: ");
                            string? name = ReadLine();
                            if (string.IsNullOrWhiteSpace(name)) continue;

                            Write("type (0 = normal, 1 = elite, 2 = boss): ");
                            MonsterType type = (MonsterType) Convert.ToInt32(ReadLine());

                            Write("floor: ");
                            int floor = Convert.ToInt32(ReadLine());

                            Write("atk: ");
                            int atk = Convert.ToInt32(ReadLine());

                            Write("hp: ");
                            int hp = Convert.ToInt32(ReadLine());

                            manager.Monsters.Add(new(name, atk, hp, floor, type));
                            WriteLine("Added");
                            ReadKey(true);
                            continue;

                        case "2":
                            manager.Monsters.ForEach(monster => WriteLine($"{monster.Name, -25} {monster.Type, -6} floor: {monster.Floor}, atk: {monster.ATK}, hp: {monster.HP}"));
                            ReadKey(true);
                            continue;

                        case "3":
                            manager.Monsters.Clear();
                            continue;

                        case "0":
                            manager.Monsters.Sort(new MonsterComparer());
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

        private static void Equipments(AssetManager manager)
        {

            while (true)
            {
                Clear();
                WriteLine("Assets manager");
                WriteLine("Equipment");
                WriteLine("0 = exit, 1 = create, 2 = show list, 3 = clear list");
                Write("enter: ");

                try
                {
                    switch(ReadLine())
                    {
                        case "1":
                            Write("name: ");
                            string? name = ReadLine();
                            if (string.IsNullOrWhiteSpace(name)) continue;

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

                            manager.Equipments.Add(new(name, atk * 2, hpPt * 10, mpPt * 5, rarity, type));
                            WriteLine("Added");
                            ReadKey(true);
                            continue;

                        case "2":
                            manager.Equipments.ForEach(equip => WriteLine($"{equip.Name, -25} {equip.Rarity, -10} {equip.Type, -6} {equip.BonusATK} atk, {equip.BonusMaxHP} hp, {equip.BonusMaxMP} mp, {equip.Price} g"));
                            ReadKey(true);
                            continue;

                        case "3":
                            manager.Equipments.Clear();
                            continue;

                        case "0":
                            manager.Equipments.Sort(new EquipmentComparer());
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

        private static void Skills(AssetManager manager)
        {

            while (true)
            {
                Clear();
                WriteLine("Assets manager");
                WriteLine("Equipment");
                WriteLine("0 = exit, 1 = create, 2 = show list, 3 = clear list");
                Write("enter: ");

                try
                {
                    switch(ReadLine())
                    {
                        case "1":
                            Write("name: ");
                            string? name = ReadLine();
                            if (string.IsNullOrWhiteSpace(name)) continue;

                            Write("type (0, 2: Single, Random, All): ");
                            TargetType type = (TargetType) Convert.ToInt32(ReadLine());

                            Write("rarity (0, 3: Common, Rare, Epic, Legendary): ");
                            ItemRarity rarity = (ItemRarity) Convert.ToInt32(ReadLine());
                            int multiplier = rarity switch
                            {
                                ItemRarity.Rare => 150,
                                ItemRarity.Epic => 250,
                                ItemRarity.Legendary => 400,
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

                            manager.Skills.Add(new(name, dmgPt * 2 * multiplier / 100, healPt * multiplier / 100, mp, rarity, type));
                            WriteLine("Added");
                            ReadKey(true);
                            continue;

                        case "2":
                            manager.Skills.ForEach(skill => WriteLine($"{skill.Name, -25} {skill.Rarity, -10} {skill.Type, -6} {skill.Damage} dmg, {skill.Heal} heal, {skill.MPCost} mp, {skill.Price} g"));
                            ReadKey(true);
                            continue;

                        case "3":
                            manager.Skills.Clear();
                            continue;

                        case "0":
                            manager.Skills.Sort(new SkillComparer());
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
}