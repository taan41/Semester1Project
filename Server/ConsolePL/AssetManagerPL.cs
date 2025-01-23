using BLL;
using DAL;
using DAL.Persistence.ConfigClasses;
using DAL.Persistence.GameComponents.EntityComponents;
using DAL.Persistence.GameComponents.ItemComponents;
using static System.Console;
using static ServerUIHelper;

class AssetManagerPL
{
    private static AssetManager Manager => AssetManager.Instance;
    private static AssetConfig AssetConfig => ConfigManager.Instance.AssetConfig;
    private static GameConfig GameConfig => ConfigManager.Instance.GameConfig;

    public static string Header { get; } = "Asset Manager";

    public static AssetManagerPL Intance { get; } = new();

    private AssetManagerPL() { }

    public async Task Start()
    {
        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" 1. Equipments");
            WriteLine(" 2. Skills");
            WriteLine(" 3. Monsters");
            WriteLine(" 0. Exit");
            DrawLine('-');
            Write(" Enter Choice: ");

            switch(ReadInput())
            {
                case "1":
                    await ManageEquip();
                    break;

                case "2":
                    await ManageSkill();
                    break;

                case "3":
                    await ManageMonster();
                    break;

                case "0": case null:
                    return;

                default: continue;
            }
        }
    }

    private async Task ManageEquip()
    {
        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" -- Equipments");
            Write(" - Stat Points Per Rarity:\n  ");
            for (int i = 0; i < Enum.GetValues(typeof(Item.Rarity)).Length; i++)
            {
                Write($" {(Item.Rarity) i}: {AssetConfig.EquipPtPerRarity[i]} |");
            }
            WriteLine();
            WriteLine($" - 1 Stat Point = {GameConfig.EquipATKPtPercentage / 100} ATK = {GameConfig.EquipDEFPtPercentage / 100} DEF = {GameConfig.EquipHPPtPercentage / 100} HP = {GameConfig.EquipMPPtPercentage / 100} MP");
            DrawLine('-');
            WriteLine(" 1. Create");
            WriteLine(" 2. Update");
            WriteLine(" 3. Delete");
            WriteLine(" 4. View List");
            WriteLine(" 5. Update Config");
            WriteLine(" 0. Back");
            DrawLine('-');
            Write(" Enter Choice: ");

            try
            {
                switch(ReadInput())
                {
                    case "1": // create
                        Clear();
                        DrawHeader(Header);
                        WriteLine(" -- Create Equipment");

                        Equipment? equipToAdd = EnterEquipInfo();
                        if (equipToAdd == null) continue;

                        equipToAdd.ID = Manager.EquipIDs[(int) equipToAdd.ItemRarity];
                        if (await Manager.Add(equipToAdd))
                        {
                            DrawLine('-');
                            WriteLine(" Added Successfully");
                        }
                        else
                            WriteLine(" Error while adding equipment");

                        ReadKey(true);
                        break;

                    case "2": // update
                        Equipment? equipToUpdate = null;
                        while (true)
                        {
                            Clear();
                            DrawHeader(Header);
                            WriteLine(" -- Update Equipment");
                            Write(" Enter Equipment ID: ");

                            int? id = EnterInt();
                            if (id == null) break;

                            if (!Manager.Equipments.ContainsKey((int) id))
                            {
                                WriteLine(" Equipment not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                equipToUpdate = Manager.Equipments[(int) id];

                            DrawLine('-');
                            WriteLine(" Equipment Info:");
                            WriteLine(" ID  | Name                      | Rarity     | Type   | ATK | DEF | HP | MP | Price");
                            WriteLine($" {equipToUpdate.ID, -3} | {equipToUpdate.Name, -25} | {equipToUpdate.ItemRarity, -10} | {equipToUpdate.EquipType, -6} | {equipToUpdate.BonusATKPoint, -3} | {equipToUpdate.BonusDEFPoint, -3} | {equipToUpdate.BonusHPPoint, -2} | {equipToUpdate.BonusMPPoint, -2} | {equipToUpdate.Price} G");
                            DrawLine('-');

                            Equipment? equipToReplace = EnterEquipInfo();
                            if (equipToReplace == null) continue;

                            equipToReplace.ID = equipToUpdate.ID;
                            if (await Manager.Add(equipToReplace))
                            {
                                DrawLine('-');
                                WriteLine(" Updated Successfully");
                            }
                            else
                                WriteLine(" Error while updating equipment");

                            ReadKey(true);
                            break;
                        }
                        break;

                    case "3": // delete
                        Equipment? equipToDelete = null;
                        while (true)
                        {
                            Clear();
                            DrawHeader(Header);
                            WriteLine(" -- Delete Equipment");
                            Write(" Enter Equipment ID: ");

                            int? id = EnterInt();
                            if (id == null) break;

                            if (!Manager.Equipments.ContainsKey((int) id))
                            {
                                WriteLine(" Equipment not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                equipToDelete = Manager.Equipments[(int) id];

                            DrawLine('-');
                            WriteLine(" Equipment Info:");
                            WriteLine(" ID  | Name                      | Rarity     | Type   | ATK | DEF | HP | MP | Price");
                            WriteLine($" {equipToDelete.ID, -3} | {equipToDelete.Name, -25} | {equipToDelete.ItemRarity, -10} | {equipToDelete.EquipType, -6} | {equipToDelete.BonusATKPoint, -3} | {equipToDelete.BonusDEFPoint, -3} | {equipToDelete.BonusHPPoint, -2} | {equipToDelete.BonusMPPoint, -2} | {equipToDelete.Price} G");
                            DrawLine('-');

                            Write(" Are you sure you want to delete this equipment? (Y/N): ");

                            var keyPressed = ReadKey(true).Key;
                            WriteLine();
                            if (keyPressed == ConsoleKey.Y || keyPressed == ConsoleKey.Enter)
                            {
                                if (await Manager.Remove(equipToDelete))
                                {
                                    DrawLine('-');
                                    WriteLine(" Deleted Successfully");
                                }
                                else
                                    WriteLine(" Error while deleting equipment");

                                ReadKey(true);
                            }
                            else
                            {
                                DrawLine('-');
                                WriteLine(" Deletion Cancelled");
                                ReadKey(true);
                            }
                            break;
                        }
                        break;


                    case "4": // view list
                        List<Equipment> equipmentList = [.. Manager.Equipments.Values];
                        int maxPage = equipmentList.Count / 15;
                        int page = 0;
                        bool exit = false;

                        while (!exit)
                        {
                            Clear();
                            DrawHeader(Header);
                            WriteLine($" -- Equipment List ({page + 1}/{maxPage + 1})");
                            WriteLine(" Arrow Keys To Turn Page, 'ESC' To Exit");
                            DrawLine('-');
                            WriteLine(" ID  | Name                      | Rarity     | Type   | ATK | DEF | HP | MP | Price");
                            DrawLine('-');
                            foreach (Equipment equip in equipmentList.Skip(page * 15).Take(15))
                            {
                                WriteLine($" {equip.ID, -3} | {equip.Name, -25} | {equip.ItemRarity, -10} | {equip.EquipType, -6} | {equip.BonusATKPoint, -3} | {equip.BonusDEFPoint, -3} | {equip.BonusHPPoint, -2} | {equip.BonusMPPoint, -2} | {equip.Price} G");
                            }
                            DrawLine('=');

                            switch (ReadKey(true).Key)
                            {
                                case ConsoleKey.Escape:
                                    exit = true;
                                    break;

                                case ConsoleKey.LeftArrow:
                                    if (page > 0) page--;
                                    continue;

                                case ConsoleKey.RightArrow:
                                    if (page < maxPage) page++;
                                    continue;

                                default: continue;
                            }
                        }
                        break;

                    case "5": // update config
                        int? intInput = null;

                        Clear();
                        DrawHeader(Header);
                        WriteLine(" -- Update Config");
                        DrawLine('-');

                        int[] newPtPerRarity = new int[Enum.GetValues(typeof(Item.Rarity)).Length];
                        WriteLine(" - Stat points per rarity:");

                        for (int i = 0; i < Enum.GetValues(typeof(Item.Rarity)).Length; i++)
                        {
                            Write($" {(Item.Rarity) i} (old: {AssetConfig.EquipPtPerRarity[i]}): ");
                            intInput = EnterInt();
                            if (intInput == null) break;
                            newPtPerRarity[i] = intInput.Value;
                        }
                        if (intInput == null) continue;

                        int[] newPtPercentage = new int[4];
                        WriteLine(" - Stat point percentage (%) (100% -> 1 stat per 1 point):");

                        Write($" ATK: (old: {GameConfig.EquipATKPtPercentage}): ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newPtPercentage[0] = intInput.Value;

                        Write($" DEF: (old: {GameConfig.EquipDEFPtPercentage}): ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newPtPercentage[1] = intInput.Value;

                        Write($" HP: (old: {GameConfig.EquipHPPtPercentage}): ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newPtPercentage[2] = intInput.Value;

                        Write($" MP: (old: {GameConfig.EquipMPPtPercentage}): ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newPtPercentage[3] = intInput.Value;

                        AssetConfig.EquipPtPerRarity = newPtPerRarity;
                        await AssetConfig.Save();

                        GameConfig.EquipATKPtPercentage = newPtPercentage[0];
                        GameConfig.EquipDEFPtPercentage = newPtPercentage[1];
                        GameConfig.EquipHPPtPercentage = newPtPercentage[2];
                        GameConfig.EquipMPPtPercentage = newPtPercentage[3];
                        await GameConfig.Save();

                        await ConfigManager.Instance.LoadConfig();

                        break;

                    case "0": case null:
                        return;

                    default: continue;
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
                ReadKey(true);
            }
        }
    }

    private static Equipment? EnterEquipInfo()
    {
        Write(" All Type:");
        foreach (Equipment.Type equipType in Enum.GetValues(typeof(Equipment.Type)))
        {
            Write($" {(int) equipType} = {equipType} |");
        }
        WriteLine();
        Write(" All Rarity:");
        foreach (Item.Rarity itemRarity in Enum.GetValues(typeof(Item.Rarity)))
        {
            Write($" {(int) itemRarity} = {itemRarity} |");
        }
        WriteLine();
        DrawLine('-');

        Write(" Name (25 max): ");
        string? name = ReadInput(false, 25);
        if (string.IsNullOrWhiteSpace(name))
            return null;

        Write(" Type: ");
        Equipment.Type? type = (Equipment.Type?) EnterInt();
        if (type == null)
            return null;

        Write(" Rarity: ");
        Item.Rarity? rarity = (Item.Rarity?) EnterInt();
        if (rarity == null)
            return null;

        int point = AssetConfig.EquipPtPerRarity[(int) rarity];

        WriteLine($" - Stat Point Left: {point}");
        Write($" Atk Point (1 Pt = {GameConfig.EquipATKPtPercentage / 100} ATK): ");
        int? atkPt = EnterInt();
        if (atkPt == null)
            return null;
        point -= atkPt.Value;

        WriteLine($" - Stat Point Left: {point}");
        Write($" DEF Point (1 Pt = {GameConfig.EquipDEFPtPercentage / 100} DEF): ");
        int? defPt = EnterInt();
        if (defPt == null)
            return null;
        point -= defPt.Value;

        WriteLine($" - Stat Point Left: {point}");
        Write($" HP Point (1 Pt = {GameConfig.EquipHPPtPercentage / 100} HP): ");
        int? hpPt = EnterInt();
        if (hpPt == null)
            return null;
        point -= hpPt.Value;

        WriteLine($" - Stat Point Left: {point}");
        Write($" MP Point (1 Pt = {GameConfig.EquipMPPtPercentage / 100} MP): ");
        int? mpPt = EnterInt();
        if (mpPt == null)
            return null;
        point -= mpPt.Value;

        Write(" Price (default -1 for auto-calc): ");
        int? price = EnterInt(-1);
        if (price == null)
            return null;

        return new Equipment()
        {
            Name = name,
            EquipType = type.Value,
            ItemRarity = rarity.Value,
            BonusATKPoint = atkPt.Value,
            BonusDEFPoint = defPt.Value,
            BonusHPPoint = hpPt.Value,
            BonusMPPoint = mpPt.Value,
            Price = price.Value
        };
    }

    private async Task ManageSkill()
    {
        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" -- Skills");
            Write(" - Stat Point Per MP Per Rarity:\n  ");
            for (int i = 0; i < Enum.GetValues(typeof(Item.Rarity)).Length; i++)
            {
                Write($" {(Item.Rarity) i}: {AssetConfig.SkillPtPerMPPerRarity[i]} |");
            }
            WriteLine();
            Write(" - Skill Type Damage Multiplier:\n  ");
            WriteLine($" Single: {GameConfig.SkillSinglePercentage}% | Random: {GameConfig.SkillRandomPercentage}% | All: {GameConfig.SkillAllPercentage}%");
            WriteLine($" - 1 MP = {(float) GameConfig.SkillDamagePtPercentage / 100:F1} Damage = {(float) GameConfig.SkillHealPtPercentage / 100:F1} Heal");
            DrawLine('-');
            WriteLine(" 1. Create");
            WriteLine(" 2. Update");
            WriteLine(" 3. Delete");
            WriteLine(" 4. View List");
            WriteLine(" 5. Update Config");
            WriteLine(" 0. Back");
            DrawLine('-');
            Write(" Enter Choice: ");

            try
            {
                switch(ReadInput())
                {
                    case "1":
                        Clear();
                        DrawHeader(Header);
                        WriteLine(" -- Create Skill");

                        Skill? skillToAdd = EnterSkillInfo();
                        if (skillToAdd == null) continue;

                        skillToAdd.ID = Manager.SkillIDs[(int) skillToAdd.ItemRarity]++;
                        if (await Manager.Add(skillToAdd))
                        {
                            DrawLine('-');
                            WriteLine(" Added Successfully");
                        }
                        else
                            WriteLine(" Error while adding skill");

                        ReadKey(true);
                        continue;

                    case "2":
                        Skill? skillToUpdate = null;
                        while (true)
                        {
                            Clear();
                            DrawHeader(Header);
                            WriteLine(" -- Update Skill");
                            Write(" Enter Skill ID: ");

                            int? id = EnterInt();
                            if (id == null) break;

                            if (!Manager.Skills.ContainsKey((int) id))
                            {
                                WriteLine(" Skill not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                skillToUpdate = Manager.Skills[(int) id];

                            DrawLine('-');
                            WriteLine(" Skill Info:");
                            WriteLine(" ID  | Name                      | Rarity     | Type   | MP | Dmg | Heal | Price");
                            WriteLine($" {skillToUpdate.ID, -3} | {skillToUpdate.Name, -25} | {skillToUpdate.ItemRarity, -10} | {skillToUpdate.SkillType, -6} | {skillToUpdate.MPCost, -2} | {skillToUpdate.DamagePoint, -3} | {skillToUpdate.HealPoint, -4} | {skillToUpdate.Price} G");
                            DrawLine('-');

                            Skill? skillToReplace = EnterSkillInfo();
                            if (skillToReplace == null) continue;

                            skillToReplace.ID = skillToUpdate.ID;
                            if (await Manager.Add(skillToReplace))
                            {
                                DrawLine('-');
                                WriteLine(" Updated Successfully");
                            }
                            else
                                WriteLine(" Error while updating skill");

                            ReadKey(true);
                            break;
                        }
                        continue;

                    case "3":
                        Skill? skillToDelete = null;
                        while (true)
                        {
                            Clear();
                            DrawHeader(Header);
                            WriteLine(" -- Delete Skill");
                            Write(" Enter Skill ID: ");

                            int? id = EnterInt();
                            if (id == null) break;

                            if (!Manager.Skills.ContainsKey((int) id))
                            {
                                WriteLine(" Skill not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                skillToDelete = Manager.Skills[(int) id];

                            DrawLine('-');
                            WriteLine(" Skill Info:");
                            WriteLine(" ID  | Name                      | Rarity     | Type   | MP | Dmg | Heal | Price");
                            WriteLine($" {skillToDelete.ID, -3} | {skillToDelete.Name, -25} | {skillToDelete.ItemRarity, -10} | {skillToDelete.SkillType, -6} | {skillToDelete.MPCost, -2} | {skillToDelete.DamagePoint, -3} | {skillToDelete.HealPoint, -4} | {skillToDelete.Price} G");
                            DrawLine('-');

                            Write(" Are you sure you want to delete this skill? (Y/N): ");

                            var keyPressed = ReadKey(true).Key;
                            WriteLine();
                            if (keyPressed == ConsoleKey.Y || keyPressed == ConsoleKey.Enter)
                            {
                                if (await Manager.Remove(skillToDelete))
                                {
                                    DrawLine('-');
                                    WriteLine(" Deleted Successfully");
                                }
                                else
                                    WriteLine(" Error while deleting skill");
                            }
                            else
                            {
                                DrawLine('-');
                                WriteLine(" Deletion Cancelled");
                                ReadKey(true);
                            }
                            break;
                        }
                        continue;

                    case "4":
                        List<Skill> skillList = [.. Manager.Skills.Values];
                        int maxPage = skillList.Count / 15;
                        int page = 0;
                        bool exit = false;

                        while (!exit)
                        {
                            Clear();
                            DrawHeader(Header);
                            WriteLine($" -- Skill List ({page + 1}/{maxPage + 1})");
                            WriteLine(" Arrow Keys To Turn Page, 'ESC' To Exit");
                            DrawLine('-');
                            WriteLine(" ID  | Name                      | Rarity     | Type   | MP | Dmg | Heal | Price");
                            DrawLine('-');
                            foreach (Skill skill in skillList.Skip(page * 15).Take(15))
                            {
                                WriteLine($" {skill.ID, -3} | {skill.Name, -25} | {skill.ItemRarity, -10} | {skill.SkillType, -6} | {skill.MPCost, -2} | {skill.DamagePoint, -3} | {skill.HealPoint, -4} | {skill.Price} G");
                            }
                            DrawLine('=');

                            switch (ReadKey(true).Key)
                            {
                                case ConsoleKey.Escape:
                                    exit = true;
                                    break;

                                case ConsoleKey.LeftArrow:
                                    if (page > 0) page--;
                                    continue;

                                case ConsoleKey.RightArrow:
                                    if (page < maxPage) page++;
                                    continue;

                                default: continue;
                            }
                        }
                        continue;

                    case "5":
                        int? intInput = null;

                        Clear();
                        DrawHeader(Header);
                        WriteLine(" -- Update Config");
                        DrawLine('-');

                        int[] newPtPerMPPerRarity = new int[Enum.GetValues(typeof(Item.Rarity)).Length];
                        WriteLine(" - Stat point per MP per Rarity:");

                        for (int i = 0; i < Enum.GetValues(typeof(Item.Rarity)).Length; i++)
                        {
                            Write($" {(Item.Rarity) i}: ");
                            intInput = EnterInt();
                            if (intInput == null) break;
                            newPtPerMPPerRarity[i] = intInput.Value;
                        }
                        if (intInput == null) continue;

                        int[] newStatPtPercentage = new int[2];
                        WriteLine(" - Stat point percentage (%) (100% -> 1 stat per point):");

                        Write($" Damage (old: {GameConfig.SkillDamagePtPercentage}): ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newStatPtPercentage[0] = intInput.Value;

                        Write($" Heal (old: {GameConfig.SkillHealPtPercentage}): ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newStatPtPercentage[1] = intInput.Value;

                        int[] newTypeDmgMultiplier = new int[Enum.GetValues(typeof(Skill.Type)).Length];
                        WriteLine(" - Skill type damage multiplier (%):");

                        Write($" Single (old: {GameConfig.SkillSinglePercentage}): ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newTypeDmgMultiplier[0] = intInput.Value;

                        Write($" Random (old: {GameConfig.SkillRandomPercentage}): ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newTypeDmgMultiplier[1] = intInput.Value;

                        Write($" All (old: {GameConfig.SkillAllPercentage}): ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newTypeDmgMultiplier[2] = intInput.Value;

                        AssetConfig.SkillPtPerMPPerRarity = newPtPerMPPerRarity;
                        await AssetConfig.Save();

                        GameConfig.SkillDamagePtPercentage = newStatPtPercentage[0];
                        GameConfig.SkillHealPtPercentage = newStatPtPercentage[1];

                        GameConfig.SkillSinglePercentage = newTypeDmgMultiplier[0];
                        GameConfig.SkillRandomPercentage = newTypeDmgMultiplier[1];
                        GameConfig.SkillAllPercentage = newTypeDmgMultiplier[2];
                        await GameConfig.Save();

                        await ConfigManager.Instance.LoadConfig();

                        continue;

                    case "0": case null:
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

    private static Skill? EnterSkillInfo()
    {
        Write(" All Type:");
        foreach (Skill.Type skillType in Enum.GetValues(typeof(Skill.Type)))
        {
            Write($" {(int) skillType} = {skillType} |");
        }
        WriteLine();
        Write(" All Rarity:");
        foreach (Item.Rarity itemRarity in Enum.GetValues(typeof(Item.Rarity)))
        {
            Write($" {(int) itemRarity} = {itemRarity} |");
        }
        WriteLine();
        DrawLine('-');

        Write(" Name (25 max): ");
        string? name = ReadInput(false, 25);
        if (string.IsNullOrWhiteSpace(name))
            return null;

        Write(" Type: ");
        Skill.Type? type = (Skill.Type?) EnterInt();
        if (type == null)
            return null;

        Write(" Rarity: ");
        Item.Rarity? rarity = (Item.Rarity?) EnterInt();
        if (rarity == null)
            return null;

        Write(" MP Cost: ");
        int? mpCost = EnterInt();
        if (mpCost == null)
            return null;

        WriteLine($" MP Point Left: {mpCost}");
        Write($" Damage Point (1 Pt = {(float) GameConfig.SkillDamagePtPercentage / 100:F1} Dmg): ");
        int? dmgPt = EnterInt();
        if (dmgPt == null)
            return null;

        WriteLine($" MP Point Left: {mpCost - dmgPt.Value}");
        Write($" Heal Point (1 Pt = {(float) GameConfig.SkillHealPtPercentage / 100:F1} Heal): ");
        int? healPt = EnterInt();
        if (healPt == null)
            return null;

        Write(" Price (default -1 for auto-calc): ");
        int? price = EnterInt(-1);
        if (price == null)
            return null;

        dmgPt = dmgPt.Value;
        healPt = healPt.Value;

        return new Skill()
        {
            Name = name,
            SkillType = type.Value,
            ItemRarity = rarity.Value,
            MPCost = mpCost.Value,
            DamagePoint = dmgPt.Value,
            HealPoint = healPt.Value,
            Price = price.Value
        };
    }

    private async Task ManageMonster()
    {
        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" -- Monsters");
            WriteLine(" 1. Create");
            WriteLine(" 2. Update");
            WriteLine(" 3. Delete");
            WriteLine(" 4. View List");
            WriteLine(" 0. Back");
            DrawLine('-');
            Write(" Enter Choice: ");

            try
            {
                switch(ReadInput())
                {
                    case "1":
                        Clear();
                        DrawHeader(Header);
                        WriteLine(" -- Create Monster");

                        Monster? monsterToAdd = EnterMonsterInfo();
                        if (monsterToAdd == null) continue;

                        monsterToAdd.ID = Manager.MonsterIDs[monsterToAdd.Floor - 1][(int) monsterToAdd.MonsterType]++;
                        if (await Manager.Add(monsterToAdd))
                        {
                            DrawLine('-');
                            WriteLine(" Added Successfully");
                        }
                        else
                            WriteLine(" Error while adding monster");

                        ReadKey(true);
                        break;

                    case "2":
                        Monster? monsterToUpdate = null;
                        while (true)
                        {
                            Clear();
                            DrawHeader(Header);
                            WriteLine(" -- Update Monster");
                            Write(" Enter Monster ID: ");

                            int? id = EnterInt();
                            if (id == null) break;

                            if (!Manager.Monsters.ContainsKey((int) id))
                            {
                                WriteLine(" Monster not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                monsterToUpdate = Manager.Monsters[(int) id];

                            DrawLine('-');
                            WriteLine(" Monster Info:");
                            WriteLine(" ID   | Name                      | Floor | Type   | ATK | DEF | HP");
                            WriteLine($" {monsterToUpdate.ID, -4} | {monsterToUpdate.Name, -25} | {monsterToUpdate.Floor, -5} | {monsterToUpdate.MonsterType, -6} | {monsterToUpdate.ATK, -3} | {monsterToUpdate.DEF, -3} | {monsterToUpdate.HP, -2}");
                            DrawLine('-');

                            Monster? monsterToReplace = EnterMonsterInfo();
                            if (monsterToReplace == null) continue;

                            monsterToReplace.ID = monsterToUpdate.ID;
                            if (await Manager.Add(monsterToReplace))
                            {
                                DrawLine('-');
                                WriteLine(" Updated Successfully");
                            }
                            else
                                WriteLine(" Error while updating monster");

                            ReadKey(true);
                            break;
                        }
                        break;

                    case "3":
                        Monster? monsterToDelete = null;
                        while (true)
                        {
                            Clear();
                            DrawHeader(Header);
                            WriteLine(" -- Delete Monster");
                            Write(" Enter Monster ID: ");

                            int? id = EnterInt();
                            if (id == null) break;

                            if (!Manager.Monsters.ContainsKey((int) id))
                            {
                                WriteLine(" Monster not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                monsterToDelete = Manager.Monsters[(int) id];

                            DrawLine('-');
                            WriteLine(" Monster Info:");
                            WriteLine(" ID   | Name                      | Floor | Type   | ATK | DEF | HP");
                            WriteLine($" {monsterToDelete.ID, -4} | {monsterToDelete.Name, -25} | {monsterToDelete.Floor, -5} | {monsterToDelete.MonsterType, -6} | {monsterToDelete.ATK, -3} | {monsterToDelete.DEF, -3} | {monsterToDelete.HP, -2}");
                            DrawLine('-');

                            Write(" Are you sure you want to delete this monster? (Y/N): ");

                            var keyPressed = ReadKey(true).Key;
                            WriteLine();
                            if (keyPressed == ConsoleKey.Y || keyPressed == ConsoleKey.Enter)
                            {
                                if (await Manager.Remove(monsterToDelete))
                                {
                                    DrawLine('-');
                                    WriteLine(" Deleted Successfully");
                                }
                                else
                                    WriteLine(" Error while deleting monster");

                                ReadKey(true);
                            }
                            else
                            {
                                DrawLine('-');
                                WriteLine(" Deletion Cancelled");
                                ReadKey(true);
                            }
                            break;
                        }
                        break;

                    case "4":
                        List<Monster> monsterList = [.. Manager.Monsters.Values];
                        int maxPage = monsterList.Count / 15;
                        int page = 0;
                        bool exit = false;

                        while (!exit)
                        {
                            Clear();
                            DrawHeader(Header);
                            WriteLine($" -- Monster List ({page + 1}/{maxPage + 1})");
                            WriteLine(" Arrow Keys To Turn Page, 'ESC' To Exit");
                            DrawLine('-');
                            WriteLine(" ID   | Name                      | Floor | Type   | ATK | DEF | HP");
                            DrawLine('-');
                            foreach (Monster monster in monsterList.Skip(page * 15).Take(15))
                            {
                                WriteLine($" {monster.ID, -4} | {monster.Name, -25} | {monster.Floor, -5} | {monster.MonsterType, -6} | {monster.ATK, -3} | {monster.DEF, -3} | {monster.HP, -2}");
                            }
                            DrawLine('=');

                            switch (ReadKey(true).Key)
                            {
                                case ConsoleKey.Escape:
                                    exit = true;
                                    break;

                                case ConsoleKey.LeftArrow:
                                    if (page > 0) page--;
                                    continue;

                                case ConsoleKey.RightArrow:
                                    if (page < maxPage) page++;
                                    continue;

                                default: continue;
                            }
                        }
                        break;

                    case "0": case null:
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

    private static Monster? EnterMonsterInfo()
    {
        Write(" All Type:");
        foreach (Monster.Type monsterType in Enum.GetValues(typeof(Monster.Type)))
        {
            Write($" {(int) monsterType} = {monsterType} |");
        }
        WriteLine();
        WriteLine($" Max Floor: {GameConfig.ProgressMaxFloor}");
        DrawLine('-');

        Write(" Name (25 max): ");
        string? name = ReadInput(false, 25);
        if (string.IsNullOrWhiteSpace(name))
            return null;

        Write(" Type: ");
        Monster.Type? type = (Monster.Type?) EnterInt();
        if (type == null)
            return null;

        Write(" Floor: ");
        int? floor = EnterInt();
        if (floor == null)
            return null;
        if (floor < 1 || floor > GameConfig.ProgressMaxFloor)
        {
            WriteLine("Invalid floor");
            ReadKey(true);
            return null;
        }

        Write(" ATK: ");
        int? atk = EnterInt();
        if (atk == null)
            return null;

        Write(" DEF: ");
        int? def = EnterInt();
        if (def == null)
            return null;

        Write(" HP: ");
        int? hp = EnterInt();
        if (hp == null)
            return null;

        return new Monster()
        {
            Name = name,
            MonsterType = type.Value,
            Floor = floor.Value,
            ATK = atk.Value,
            DEF = def.Value,
            MaxHP = hp.Value,
            HP = hp.Value,
        };
    }

    private static int? EnterInt(int defaultValue = 0)
    {
        string? numInput = ReadInput();
        if (numInput == null)
            return null;
        if (string.IsNullOrWhiteSpace(numInput))
            return defaultValue;
        if (int.TryParse(numInput, out int result))
            return result;
        else
        {
            WriteLine("Invalid input");
            ReadKey(true);
        }
        return null;
    }

    private static double? EnterDouble()
    {
        string? numInput = ReadInput();
        if (numInput == null)
            return null;
        if (string.IsNullOrWhiteSpace(numInput))
            return 0;
        if (double.TryParse(numInput, out double result))
            return result;
        else
        {
            WriteLine("Invalid input");
            ReadKey(true);
        }
        return null;
    }
}