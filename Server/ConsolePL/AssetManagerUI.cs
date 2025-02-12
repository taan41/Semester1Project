using BLL;
using DAL.Config;
using DAL.GameComponents.Entity;
using DAL.GameComponents.Item;

using static System.Console;
using static ConsoleUtilities;

class AssetManagerUI
{
    private static AssetManager Manager => AssetManager.Instance;
    private static AssetConfig AssetConfig => ConfigManager.Instance.AssetConfig;
    private static GameConfig GameConfig => ConfigManager.Instance.GameConfig;

    public const string Header = "Asset Manager";

    private AssetManagerUI() { }

    public static async Task Start()
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

    #region Equipments
    private static async Task ManageEquip()
    {
        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" -- Equipments");
            WriteLine(" - Stat Points Per Rarity:");
            for (int i = 0; i < Enum.GetValues(typeof(Equipment.Type)).Length; i++)
            {
                WriteLine($" {(Equipment.Type) i}:");
                for (int j = 0; j < Enum.GetValues(typeof(GameItem.Rarity)).Length; j++)
                {
                    Write($" {(GameItem.Rarity) j}: {AssetConfig.EquipPtPerRarityPerType[i][j]} |");
                }
                WriteLine();
            }
            WriteLine(" - Stat Point Percentage:");
            WriteLine($" ATK: {GameConfig.EquipPtATKPercentage}% | DEF: {GameConfig.EquipPtDEFPercentage}% | HP: {GameConfig.EquipPtHPPercentage}% | MP: {GameConfig.EquipPtMPPercentage}%");
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
                            WriteEquipInfo(equipToUpdate);
                            DrawLine('-');

                            Equipment? equipToReplace = EnterEquipInfo(equipToUpdate);
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
                            WriteEquipInfo(equipToDelete);
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
                        equipmentList.Sort((a, b) => a.ID - b.ID);
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
                            WriteLine($" 1 stat point -> {GameConfig.EquipPtATKPercentage / 100} ATK | {GameConfig.EquipPtDEFPercentage / 100} DEF | {GameConfig.EquipPtHPPercentage / 100} HP | {GameConfig.EquipPtMPPercentage / 100} MP");
                            WriteLine(" ID  | Name                      | Rarity    | Type   | ATK | DEF | HP | MP | Price");
                            DrawLine('-');
                            foreach (Equipment equip in equipmentList.Skip(page * 15).Take(15))
                            {
                                WriteEquipInfo(equip, false);
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

                        int[][] newPtPerRarityPerType = new int[Enum.GetValues(typeof(Equipment.Type)).Length][];
                        WriteLine(" - Stat points per rarity:");

                        for (int i = 0; i < Enum.GetValues(typeof(Equipment.Type)).Length; i++)
                        {
                            for (int j = 0; i < Enum.GetValues(typeof(GameItem.Rarity)).Length; j++)
                            {
                                newPtPerRarityPerType[i] = new int[Enum.GetValues(typeof(GameItem.Rarity)).Length];
                                Write($" {(GameItem.Rarity) j} {(Equipment.Type) i} (old: {AssetConfig.EquipPtPerRarityPerType[i][j]}): ");
                                intInput = EnterInt(AssetConfig.EquipPtPerRarityPerType[i][j]);
                                if (intInput == null) break;
                                newPtPerRarityPerType[i][j] = intInput.Value;
                            }
                        }
                        if (intInput == null) continue;

                        int[] newPtPercentage = new int[4];
                        WriteLine(" - Stat point percentage (%) (100% -> 1 stat per 1 point):");

                        Write($" ATK: (old: {GameConfig.EquipPtATKPercentage}): ");
                        intInput = EnterInt(GameConfig.EquipPtATKPercentage);
                        if (intInput == null) continue;
                         newPtPercentage[0] = intInput.Value;

                        Write($" DEF: (old: {GameConfig.EquipPtDEFPercentage}): ");
                        intInput = EnterInt(GameConfig.EquipPtDEFPercentage);
                        if (intInput == null) continue;
                         newPtPercentage[1] = intInput.Value;

                        Write($" HP: (old: {GameConfig.EquipPtHPPercentage}): ");
                        intInput = EnterInt(GameConfig.EquipPtHPPercentage);
                        if (intInput == null) continue;
                         newPtPercentage[2] = intInput.Value;

                        Write($" MP: (old: {GameConfig.EquipPtMPPercentage}): ");
                        intInput = EnterInt(GameConfig.EquipPtMPPercentage);
                        if (intInput == null) continue;
                         newPtPercentage[3] = intInput.Value;

                        AssetConfig.EquipPtPerRarityPerType = newPtPerRarityPerType;
                        await AssetConfig.Save();

                        GameConfig.EquipPtATKPercentage = newPtPercentage[0];
                        GameConfig.EquipPtDEFPercentage = newPtPercentage[1];
                        GameConfig.EquipPtHPPercentage = newPtPercentage[2];
                        GameConfig.EquipPtMPPercentage = newPtPercentage[3];
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

    private static void WriteEquipInfo(Equipment equip, bool showAtributeName = true)
    {
        if (showAtributeName)
        {
            WriteLine(" - Equipment Info:");
            WriteLine(" ID  | Name                      | Rarity    | Type   | ATK | DEF | HP | MP | Price");
        }

        Write($" {equip.ID, -3} |");
        Write($" {equip.Name, -25} |");
        Write($" {equip.ItemRarity, -9} |");
        Write($" {equip.EquipType, -6} |");
        Write($" {equip.BonusATKPoint * GameConfig.EquipPtATKPercentage / 100, -3} |");
        Write($" {equip.BonusDEFPoint * GameConfig.EquipPtDEFPercentage / 100, -3} |");
        Write($" {equip.BonusHPPoint * GameConfig.EquipPtHPPercentage / 100, -2} |");
        Write($" {equip.BonusMPPoint * GameConfig.EquipPtMPPercentage / 100, -2} |");
        WriteLine($" {equip.Price} G");
    }

    private static Equipment? EnterEquipInfo(Equipment? oldEquip = null)
    {
        Write(" - All Type:");
        foreach (Equipment.Type equipType in Enum.GetValues(typeof(Equipment.Type)))
        {
            Write($" {(int) equipType} = {equipType} |");
        }
        WriteLine();
        Write(" - All Rarity:");
        foreach (GameItem.Rarity itemRarity in Enum.GetValues(typeof(GameItem.Rarity)))
        {
            Write($" {(int) itemRarity} = {itemRarity} |");
        }
        WriteLine();
        if (oldEquip != null)
        {
            WriteLine(" (Leave blank to keep old value)");
        }
        DrawLine('-');

        Write(" Name (25 max): ");
        string? name = ReadInput(false, 25);
        if (name == null)
            return null;
        if (string.IsNullOrWhiteSpace(name))
            if (oldEquip != null)
                name = oldEquip.Name;
            else
                return null;

        Write(" Type: ");
        Equipment.Type? type = (Equipment.Type?) EnterInt((int?) oldEquip?.EquipType);
        if (type == null)
            return null;

        Write(" Rarity: ");
        GameItem.Rarity? rarity = (GameItem.Rarity?) EnterInt((int?) oldEquip?.ItemRarity);
        if (rarity == null)
            return null;

        int point = AssetConfig.EquipPtPerRarityPerType[(int) type][(int) rarity];

        Write($" Atk Point ({GameConfig.EquipPtATKPercentage}%) ({point} Pt Left): ");
        int? atkPt = EnterInt(oldEquip?.BonusATKPoint);
        if (atkPt == null)
            return null;
        point -= atkPt.Value;

        Write($" DEF Point ({GameConfig.EquipPtDEFPercentage}%) ({point} Pt Left): ");
        int? defPt = EnterInt(oldEquip?.BonusDEFPoint);
        if (defPt == null)
            return null;
        point -= defPt.Value;

        Write($" HP Point ({GameConfig.EquipPtHPPercentage}%) ({point} Pt Left): ");
        int? hpPt = EnterInt(oldEquip?.BonusHPPoint);
        if (hpPt == null)
            return null;
        point -= hpPt.Value;

        Write($" MP Point ({GameConfig.EquipPtMPPercentage}%) ({point} Pt Left): ");
        int? mpPt = EnterInt(oldEquip?.BonusMPPoint);
        if (mpPt == null)
            return null;
        point -= mpPt.Value;

        Write(" Price (default (-1) for auto-calc): ");
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
    #endregion

    #region Skills
    private static async Task ManageSkill()
    {
        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" -- Skills");
            WriteLine(" - Stat Point Percentage:");
            WriteLine($" Damage: {GameConfig.SkillPtDmgPercentage}% | Heal: {GameConfig.SkillPtHealPercentage}%");
            WriteLine(" - Stat Point Percentage Per Rarity:");
            WriteLine($" Common: {GameConfig.SkillRarityNormalPercentage}% | Rare: {GameConfig.SkillRarityRarePercentage}% | Epic: {GameConfig.SkillRarityEpicPercentage}% | Legendary: {GameConfig.SkillRarityLegendaryPercentage}%");
            WriteLine();
            WriteLine(" - Skill Type Damage Multiplier:");
            WriteLine($" Single: {GameConfig.SkillTypeSinglePercentage}% | Random: {GameConfig.SkillTypeRandomPercentage}% | All: {GameConfig.SkillTypeAllPercentage}%");
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
                            WriteSkillInfo(skillToUpdate);
                            DrawLine('-');

                            Skill? skillToReplace = EnterSkillInfo(skillToUpdate);
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
                            WriteSkillInfo(skillToDelete);
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
                        skillList.Sort((a, b) => a.ID - b.ID);
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
                            WriteLine(" ID  | Name                      | Rarity    | Type   | MP | Dmg | Heal | Price");
                            DrawLine('-');
                            foreach (Skill skill in skillList.Skip(page * 15).Take(15))
                            {
                                WriteSkillInfo(skill, false);
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

                        int[] newStatPtPercentage = new int[2];
                        WriteLine(" - Stat point percentage (%) (100% -> 1 stat per point):");

                        Write($" Damage (old: {GameConfig.SkillPtDmgPercentage}): ");
                        intInput = EnterInt(GameConfig.SkillPtDmgPercentage);
                        if (intInput == null) continue;
                        newStatPtPercentage[0] = intInput.Value;

                        Write($" Heal (old: {GameConfig.SkillPtHealPercentage}): ");
                        intInput = EnterInt(GameConfig.SkillPtHealPercentage);
                        if (intInput == null) continue;
                        newStatPtPercentage[1] = intInput.Value;

                        int[] newRarityPercentage = new int[Enum.GetValues(typeof(GameItem.Rarity)).Length];
                        WriteLine(" - Stat point percentage per rarity (%) (200% -> 2 stats per point):");
                        
                        Write($" Common (old: {GameConfig.SkillRarityNormalPercentage}): ");
                        intInput = EnterInt(GameConfig.SkillRarityNormalPercentage);
                        if (intInput == null) continue;
                        newRarityPercentage[0] = intInput.Value;

                        Write($" Rare (old: {GameConfig.SkillRarityRarePercentage}): ");
                        intInput = EnterInt(GameConfig.SkillRarityRarePercentage);
                        if (intInput == null) continue;
                        newRarityPercentage[1] = intInput.Value;

                        Write($" Epic (old: {GameConfig.SkillRarityEpicPercentage}): ");
                        intInput = EnterInt(GameConfig.SkillRarityEpicPercentage);
                        if (intInput == null) continue;
                        newRarityPercentage[2] = intInput.Value;

                        Write($" Legendary (old: {GameConfig.SkillRarityLegendaryPercentage}): ");
                        intInput = EnterInt(GameConfig.SkillRarityLegendaryPercentage);
                        if (intInput == null) continue;
                        newRarityPercentage[3] = intInput.Value;

                        int[] newTypeDmgMultiplier = new int[Enum.GetValues(typeof(Skill.Type)).Length];
                        WriteLine(" - Skill type damage multiplier (%):");

                        Write($" Single (old: {GameConfig.SkillTypeSinglePercentage}): ");
                        intInput = EnterInt(GameConfig.SkillTypeSinglePercentage);
                        if (intInput == null) continue;
                        newTypeDmgMultiplier[0] = intInput.Value;

                        Write($" Random (old: {GameConfig.SkillTypeRandomPercentage}): ");
                        intInput = EnterInt(GameConfig.SkillTypeRandomPercentage);
                        if (intInput == null) continue;
                        newTypeDmgMultiplier[1] = intInput.Value;

                        Write($" All (old: {GameConfig.SkillTypeAllPercentage}): ");
                        intInput = EnterInt(GameConfig.SkillTypeAllPercentage);
                        if (intInput == null) continue;
                        newTypeDmgMultiplier[2] = intInput.Value;

                        GameConfig.SkillPtDmgPercentage = newStatPtPercentage[0];
                        GameConfig.SkillPtHealPercentage = newStatPtPercentage[1];

                        GameConfig.SkillRarityNormalPercentage = newRarityPercentage[0];
                        GameConfig.SkillRarityRarePercentage = newRarityPercentage[1];
                        GameConfig.SkillRarityEpicPercentage = newRarityPercentage[2];
                        GameConfig.SkillRarityLegendaryPercentage = newRarityPercentage[3];

                        GameConfig.SkillTypeSinglePercentage = newTypeDmgMultiplier[0];
                        GameConfig.SkillTypeRandomPercentage = newTypeDmgMultiplier[1];
                        GameConfig.SkillTypeAllPercentage = newTypeDmgMultiplier[2];
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

    private static void WriteSkillInfo(Skill skill, bool showAtributeName = true)
    {
        if (showAtributeName)
        {
            WriteLine(" - Skill Info:");
            WriteLine(" ID  | Name                      | Rarity    | Type   | MP | Dmg | Heal | Price");
        }

        Write($" {skill.ID, -3} |");
        Write($" {skill.Name, -25} |");
        Write($" {skill.ItemRarity, -9} |");
        Write($" {skill.SkillType, -6} |");
        Write($" {skill.MPCost, -2} |");

        int typePercentage = skill.SkillType switch
        {
            Skill.Type.Single => GameConfig.SkillTypeSinglePercentage,
            Skill.Type.Random => GameConfig.SkillTypeRandomPercentage,
            Skill.Type.All => GameConfig.SkillTypeAllPercentage,
            _ => 0
        };

        Write($" {skill.DamagePoint * GameConfig.SkillPtDmgPercentage * typePercentage / 10000, -3} |");
        Write($" {skill.HealPoint * GameConfig.SkillPtHealPercentage / 100, -4} |");
        WriteLine($" {skill.Price} G");
    }

    private static Skill? EnterSkillInfo(Skill? oldSkill = null)
    {
        Write(" - All Type:");
        foreach (Skill.Type skillType in Enum.GetValues(typeof(Skill.Type)))
        {
            Write($" {(int) skillType} = {skillType} |");
        }
        WriteLine();
        Write(" - All Rarity:");
        foreach (GameItem.Rarity itemRarity in Enum.GetValues(typeof(GameItem.Rarity)))
        {
            Write($" {(int) itemRarity} = {itemRarity} |");
        }
        WriteLine();
        if (oldSkill != null)
        {
            WriteLine(" (Leave blank to keep old value)");
        }
        DrawLine('-');

        Write(" Name (25 max): ");
        string? name = ReadInput(false, 25);
        if (name == null)
            return null;
        if (string.IsNullOrWhiteSpace(name))
            if (oldSkill != null)
                name = oldSkill.Name;
            else
                return null;

        Write(" Type: ");
        Skill.Type? type = (Skill.Type?) EnterInt((int?) oldSkill?.SkillType);
        if (type == null)
            return null;

        Write(" Rarity: ");
        GameItem.Rarity? rarity = (GameItem.Rarity?) EnterInt((int?) oldSkill?.ItemRarity);
        if (rarity == null)
            return null;

        Write(" MP Cost (1 MP Cost = 1 Stat Pt): ");
        int? mpCost = EnterInt(oldSkill?.MPCost);
        if (mpCost == null)
            return null;

        Write($" Damage Point ({GameConfig.SkillPtDmgPercentage}%) ({mpCost} Pt Left): ");
        int? dmgPt = EnterInt(oldSkill?.DamagePoint);
        if (dmgPt == null)
            return null;

        Write($" Heal Point ({GameConfig.SkillPtHealPercentage}%) ({mpCost - dmgPt.Value} Pt Left): ");
        int? healPt = EnterInt(oldSkill?.HealPoint);
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
    #endregion

    #region Monsters
    private static async Task ManageMonster()
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
                            WriteMonsterInfo(monsterToUpdate);
                            DrawLine('-');

                            Monster? monsterToReplace = EnterMonsterInfo(monsterToUpdate);
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
                            WriteMonsterInfo(monsterToDelete);
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
                        monsterList.Sort((a, b) => a.ID - b.ID);
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
                            WriteLine(" (Monster's ATK and HP will scale with progress)");
                            WriteLine(" ID   | Name                      | Floor | Type   | ATK | DEF | HP");
                            DrawLine('-');
                            foreach (Monster monster in monsterList.Skip(page * 15).Take(15))
                            {
                                WriteMonsterInfo(monster, false);
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

    private static void WriteMonsterInfo(Monster monster, bool showAtributeName = true)
    {
        if (showAtributeName)
        {
            WriteLine(" - Monster Info:");
            WriteLine(" (Monster's ATK and HP will scale with progress)");
            WriteLine(" ID   | Name                      | Floor | Type   | ATK | DEF | HP");
        }

        Write($" {monster.ID, -4} |");
        Write($" {monster.Name, -25} |");
        Write($" {monster.Floor, -5} |");
        Write($" {monster.MonsterType, -6} |");
        Write($" {monster.ATK, -3} |");
        Write($" {monster.DEF, -3} |");
        WriteLine($" {monster.HP, -2}");
    }

    private static Monster? EnterMonsterInfo(Monster? oldMonster = null)
    {
        Write(" - All Type:");
        foreach (Monster.Type monsterType in Enum.GetValues(typeof(Monster.Type)))
        {
            Write($" {(int) monsterType} = {monsterType} |");
        }
        WriteLine();
        WriteLine($" - Max Floor: {GameConfig.ProgressMaxFloor}");
        if (oldMonster != null)
        {
            WriteLine(" (Leave blank to keep old value)");
        }
        DrawLine('-');

        Write(" Name (25 max): ");
        string? name = ReadInput(false, 25);
        if (name == null)
            return null;
        if (string.IsNullOrWhiteSpace(name))
            if (oldMonster != null)
                name = oldMonster.Name;
            else
                return null;

        Write(" Type: ");
        Monster.Type? type = (Monster.Type?) EnterInt((int?) oldMonster?.MonsterType);
        if (type == null)
            return null;

        Write(" Floor: ");
        int? floor = EnterInt(oldMonster?.Floor);
        if (floor == null)
            return null;
        if (floor < 1 || floor > GameConfig.ProgressMaxFloor)
        {
            WriteLine("Invalid floor");
            ReadKey(true);
            return null;
        }

        Write(" ATK: ");
        int? atk = EnterInt(oldMonster?.ATK);
        if (atk == null)
            return null;

        Write(" DEF: ");
        int? def = EnterInt(oldMonster?.DEF);
        if (def == null)
            return null;

        Write(" HP: ");
        int? hp = EnterInt(oldMonster?.MaxHP);
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

    private static int? EnterInt(int? defaultValue = null)
    {
        defaultValue ??= 0;
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
    #endregion
}