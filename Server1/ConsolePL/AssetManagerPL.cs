using static System.Console;
using static ServerUIHelper;

class AssetManagerPL
{
    public static string Header { get; } = "Asset Manager";

    public static AssetManagerPL Intance { get; } = new();

    private readonly AssetMetadata _metadata = AssetMetadata.Instance;

    private Dictionary<int, Equipment> Equipments = [];
    private Dictionary<int, Skill> Skills  = [];
    private Dictionary<int, Monster> Monsters = [];

    private AssetManagerPL() { }

    public async Task Start()
    {
        await Initialize();

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
                    _metadata.Save();
                    return;

                default: continue;
            }
        }
    }

    private async Task Initialize()
    {
        await LoadEquipments();
        await LoadSkills();
        await LoadMonsters();
    }

    private async Task LoadEquipments()
    {
        var (equipDB, _) = await EquipmentDB.GetAll();

        if (equipDB != null)
        {
            Equipments = equipDB;

            foreach (var equip in Equipments.Values)
                if (equip.ID >= _metadata.EquipIDTracker[(int) equip.Rarity])
                    _metadata.EquipIDTracker[(int) equip.Rarity] = equip.ID + 1;
        }
    }

    private async Task LoadSkills()
    {
        var (skillDB, _) = await SkillDB.GetAll();

        if (skillDB != null)
        {
            Skills = skillDB;

            foreach (var skill in Skills.Values)
                if (skill.ID >= _metadata.SkillIDTracker[(int) skill.Rarity])
                    _metadata.SkillIDTracker[(int) skill.Rarity] = skill.ID + 1;
        }
    }

    private async Task LoadMonsters()
    {
        var (monsterDB, _) = await MonsterDB.GetAll();

        if (monsterDB != null)
        {
            Monsters = monsterDB;

            foreach (var monster in Monsters.Values)
                if (monster.ID >= _metadata.MonsterIDTracker[monster.Floor - 1][(int) monster.Type])
                    _metadata.MonsterIDTracker[monster.Floor - 1][(int) monster.Type] = monster.ID + 1;
        }
    }

    private async Task ManageEquip()
    {
        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" -- Equipments");
            Write(" Stat Points Per Rarity:\n  ");
            for (int i = 0; i < Enum.GetValues(typeof(ItemRarity)).Length; i++)
            {
                Write($" {(ItemRarity) i}: {_metadata.EquipRarityPoint[i]} |");
            }
            WriteLine();
            WriteLine($" 1 Stat Point = {_metadata.EquipPointMultiplier[0]} ATK = {_metadata.EquipPointMultiplier[1]} DEF = {_metadata.EquipPointMultiplier[2]} HP = {_metadata.EquipPointMultiplier[3]} MP");
            DrawLine('-');
            WriteLine(" 1. Create");
            WriteLine(" 2. Update");
            WriteLine(" 3. Delete");
            WriteLine(" 4. View List");
            WriteLine(" 5. Update Metadata");
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

                        equipToAdd.ID = _metadata.EquipIDTracker[(int) equipToAdd.Rarity]++;
                        Equipments[equipToAdd.ID] = equipToAdd;
                        await EquipmentDB.Add(equipToAdd);

                        DrawLine('-');
                        WriteLine(" Added Successfully");
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

                            if (!Equipments.ContainsKey((int) id))
                            {
                                WriteLine(" Equipment not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                equipToUpdate = Equipments[(int) id];

                            DrawLine('-');
                            WriteLine(" Equipment Info:");
                            WriteLine(" ID  | Name                      | Rarity     | Type   | ATK | DEF | HP | MP | Price");
                            WriteLine($" {equipToUpdate.ID, -3} | {equipToUpdate.Name, -25} | {equipToUpdate.Rarity, -10} | {equipToUpdate.Type, -6} | {equipToUpdate.BonusATK, -3} | {equipToUpdate.BonusDEF, -3} | {equipToUpdate.BonusHP, -2} | {equipToUpdate.BonusMP, -2} | {equipToUpdate.Price} G");
                            DrawLine('-');

                            Equipment? equipToReplace = EnterEquipInfo();
                            if (equipToReplace == null) continue;

                            equipToReplace.ID = equipToUpdate.ID;
                            Equipments[equipToReplace.ID] = equipToReplace;
                            await EquipmentDB.Update(equipToReplace);

                            DrawLine('-');
                            WriteLine(" Updated Successfully");
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

                            if (!Equipments.ContainsKey((int) id))
                            {
                                WriteLine(" Equipment not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                equipToDelete = Equipments[(int) id];

                            DrawLine('-');
                            WriteLine(" Equipment Info:");
                            WriteLine(" ID  | Name                      | Rarity     | Type   | ATK | DEF | HP | MP | Price");
                            WriteLine($" {equipToDelete.ID, -3} | {equipToDelete.Name, -25} | {equipToDelete.Rarity, -10} | {equipToDelete.Type, -6} | {equipToDelete.BonusATK, -3} | {equipToDelete.BonusDEF, -3} | {equipToDelete.BonusHP, -2} | {equipToDelete.BonusMP, -2} | {equipToDelete.Price} G");
                            DrawLine('-');

                            Write(" Are you sure you want to delete this equipment? (Y/N): ");

                            var keyPressed = ReadKey(true).Key;
                            WriteLine();
                            if (keyPressed == ConsoleKey.Y || keyPressed == ConsoleKey.Enter)
                            {
                                Equipments.Remove(equipToDelete.ID);
                                await EquipmentDB.Delete(equipToDelete.ID);

                                int deletedID = equipToDelete.ID;
                                for (int i = deletedID; i < _metadata.EquipIDTracker[deletedID / 100]; i++)
                                {
                                    if (Equipments.ContainsKey(i + 1))
                                    {
                                        Equipments[i] = Equipments[i + 1];
                                        Equipments[i].ID = i;
                                        Equipments.Remove(i + 1);
                                        await EquipmentDB.UpdateID(i + 1, i);
                                    }
                                }
                                _metadata.EquipIDTracker[deletedID / 100]--;

                                DrawLine('-');
                                WriteLine(" Deleted Successfully");
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
                        List<Equipment> equipmentList = [.. Equipments.Values];
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
                                WriteLine($" {equip.ID, -3} | {equip.Name, -25} | {equip.Rarity, -10} | {equip.Type, -6} | {equip.BonusATK, -3} | {equip.BonusDEF, -3} | {equip.BonusHP, -2} | {equip.BonusMP, -2} | {equip.Price} G");
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

                    case "5": // update metadata
                        int? intInput = null;

                        Clear();
                        DrawHeader(Header);
                        WriteLine(" -- Update Metadata");
                        DrawLine('-');

                        int[] newEquipRarityPoint = new int[Enum.GetValues(typeof(ItemRarity)).Length];
                        WriteLine(" - Stat points per rarity:");

                        for (int i = 0; i < Enum.GetValues(typeof(ItemRarity)).Length; i++)
                        {
                            Write($" {(ItemRarity) i}: ");
                            intInput = EnterInt();
                            if (intInput == null) break;
                            newEquipRarityPoint[i] = intInput.Value;
                        }
                        if (intInput == null) continue;

                        int[] newEquipPointMultiplier = new int[3];
                        WriteLine(" - Stat point multiplier:");

                        Write(" ATK: ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newEquipPointMultiplier[0] = intInput.Value;

                        Write(" DEF: ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newEquipPointMultiplier[1] = intInput.Value;

                        Write(" HP: ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newEquipPointMultiplier[2] = intInput.Value;

                        Write(" MP: ");
                        intInput = EnterInt();
                        if (intInput == null) continue;
                        newEquipPointMultiplier[3] = intInput.Value;

                        _metadata.EquipRarityPoint = newEquipRarityPoint;
                        _metadata.EquipPointMultiplier = newEquipPointMultiplier;

                        _metadata.Save();
                        await LoadEquipments();
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

    private Equipment? EnterEquipInfo()
    {
        Write(" All Type:");
        foreach (EquipType equipType in Enum.GetValues(typeof(EquipType)))
        {
            Write($" {(int) equipType} = {equipType} |");
        }
        WriteLine();
        Write(" All Rarity:");
        foreach (ItemRarity itemRarity in Enum.GetValues(typeof(ItemRarity)))
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
        EquipType? type = (EquipType?) EnterInt();
        if (type == null)
            return null;

        Write(" Rarity: ");
        ItemRarity? rarity = (ItemRarity?) EnterInt();
        if (rarity == null)
            return null;

        int point = _metadata.EquipRarityPoint[(int) rarity];

        WriteLine($" - Stat Point Left: {point}");
        Write($" Atk Point (1 Pt = {_metadata.EquipPointMultiplier[0]} ATK): ");
        int? atk = EnterInt();
        if (atk == null)
            return null;
        point -= atk.Value;

        WriteLine($" - Stat Point Left: {point}");
        Write($" DEF Point (1 Pt = {_metadata.EquipPointMultiplier[1]} DEF): ");
        int? def = EnterInt();
        if (def == null)
            return null;
        point -= def.Value;

        WriteLine($" - Stat Point Left: {point}");
        Write($" HP Point (1 Pt = {_metadata.EquipPointMultiplier[2]} HP): ");
        int? hpPt = EnterInt();
        if (hpPt == null)
            return null;
        point -= hpPt.Value;

        WriteLine($" - Stat Point Left: {point}");
        Write($" MP Point (1 Pt = {_metadata.EquipPointMultiplier[3]} MP): ");
        int? mpPt = EnterInt();
        if (mpPt == null)
            return null;
        point -= mpPt.Value;

        Write(" Price (0 for auto-calc): ");
        int? price = EnterInt();
        if (price == null)
            return null;

        return new Equipment()
        {
            Name = name,
            Type = type.Value,
            Rarity = rarity.Value,
            BonusATK = atk.Value * _metadata.EquipPointMultiplier[0],
            BonusDEF = def.Value * _metadata.EquipPointMultiplier[1],
            BonusHP = hpPt.Value * _metadata.EquipPointMultiplier[2],
            BonusMP = mpPt.Value * _metadata.EquipPointMultiplier[3],
            Price = price != 0 ? price.Value : Equipment.CalcPrice((ItemRarity) rarity)
        };
    }

    private async Task ManageSkill()
    {
        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" -- Skills");
            Write(" Rarity Stat Multiplier:\n  ");
            for (int i = 0; i < Enum.GetValues(typeof(ItemRarity)).Length; i++)
            {
                Write($" {(ItemRarity) i}: {_metadata.SkillRarityMultiplier[i]}% |");
            }
            WriteLine();
            Write(" Type Damage Multiplier:\n  ");
            for (int i = 0; i < Enum.GetValues(typeof(SkillType)).Length; i++)
            {
                Write($" {(SkillType) i}: {_metadata.SkillTypeDmgMultiplier[i]:F1} |");
            }
            WriteLine();
            WriteLine($" 1 MP = {_metadata.SkillMPMultiplier[0]:F1} Damage = {_metadata.SkillMPMultiplier[1]:F1} Heal");
            DrawLine('-');
            WriteLine(" 1. Create");
            WriteLine(" 2. Update");
            WriteLine(" 3. Delete");
            WriteLine(" 4. View List");
            WriteLine(" 5. Update Metadata");
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

                        Skill? skillToAdd = EnterSkillInfo(out int dmgPt, out int healPt);
                        if (skillToAdd == null) continue;

                        skillToAdd.ID = _metadata.SkillIDTracker[(int) skillToAdd.Rarity]++;
                        Skills[skillToAdd.ID] = skillToAdd;
                        await SkillDB.Add(skillToAdd, dmgPt, healPt);

                        DrawLine('-');
                        WriteLine(" Added Successfully");
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

                            if (!Skills.ContainsKey((int) id))
                            {
                                WriteLine(" Skill not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                skillToUpdate = Skills[(int) id];

                            DrawLine('-');
                            WriteLine(" Skill Info:");
                            WriteLine(" ID  | Name                      | Rarity     | Type   | MP | Dmg | Heal | Price");
                            WriteLine($" {skillToUpdate.ID, -3} | {skillToUpdate.Name, -25} | {skillToUpdate.Rarity, -10} | {skillToUpdate.Type, -6} | {skillToUpdate.MPCost, -2} | {skillToUpdate.Damage, -3} | {skillToUpdate.Heal, -4} | {skillToUpdate.Price} G");
                            DrawLine('-');

                            Skill? skillToReplace = EnterSkillInfo(out dmgPt, out healPt);
                            if (skillToReplace == null) continue;

                            skillToReplace.ID = skillToUpdate.ID;
                            Skills[skillToReplace.ID] = skillToReplace;
                            await SkillDB.Update(skillToReplace, dmgPt, healPt);

                            DrawLine('-');
                            WriteLine(" Updated Successfully");
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

                            if (!Skills.ContainsKey((int) id))
                            {
                                WriteLine(" Skill not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                skillToDelete = Skills[(int) id];

                            DrawLine('-');
                            WriteLine(" Skill Info:");
                            WriteLine(" ID  | Name                      | Rarity     | Type   | MP | Dmg | Heal | Price");
                            WriteLine($" {skillToDelete.ID, -3} | {skillToDelete.Name, -25} | {skillToDelete.Rarity, -10} | {skillToDelete.Type, -6} | {skillToDelete.MPCost, -2} | {skillToDelete.Damage, -3} | {skillToDelete.Heal, -4} | {skillToDelete.Price} G");
                            DrawLine('-');

                            Write(" Are you sure you want to delete this skill? (Y/N): ");

                            var keyPressed = ReadKey(true).Key;
                            WriteLine();
                            if (keyPressed == ConsoleKey.Y || keyPressed == ConsoleKey.Enter)
                            {
                                Skills.Remove(skillToDelete.ID);
                                await SkillDB.Delete(skillToDelete.ID);

                                int deletedID = skillToDelete.ID;
                                for (int i = deletedID; i < _metadata.SkillIDTracker[deletedID / 100]; i++)
                                {
                                    if (Skills.ContainsKey(i + 1))
                                    {
                                        Skills[i] = Skills[i + 1];
                                        Skills[i].ID = i;
                                        Skills.Remove(i + 1);
                                        await SkillDB.UpdateID(i + 1, i);
                                    }
                                }
                                _metadata.SkillIDTracker[deletedID / 100]--;

                                DrawLine('-');
                                WriteLine(" Deleted Successfully");
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
                        continue;

                    case "4":
                        List<Skill> skillList = [.. Skills.Values];
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
                                WriteLine($" {skill.ID, -3} | {skill.Name, -25} | {skill.Rarity, -10} | {skill.Type, -6} | {skill.MPCost, -2} | {skill.Damage, -3} | {skill.Heal, -4} | {skill.Price} G");
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
                        double? doubleInput = null;

                        Clear();
                        DrawHeader(Header);
                        WriteLine(" -- Update Metadata");
                        DrawLine('-');

                        int[] newSkillRarityMultiplier = new int[Enum.GetValues(typeof(ItemRarity)).Length];
                        WriteLine(" - Rarity multiplier (%):");

                        for (int i = 0; i < Enum.GetValues(typeof(ItemRarity)).Length; i++)
                        {
                            Write($" {(ItemRarity) i}: ");
                            intInput = EnterInt();
                            if (intInput == null) break;
                            newSkillRarityMultiplier[i] = intInput.Value;
                        }
                        if (intInput == null) continue;

                        double[] newSkillMPMultiplier = new double[2];
                        WriteLine(" - MP point multiplier:");

                        Write(" Damage: ");
                        doubleInput = EnterDouble();
                        if (doubleInput == null) continue;
                        newSkillMPMultiplier[0] = doubleInput.Value;

                        Write(" Heal: ");
                        doubleInput = EnterDouble();
                        if (doubleInput == null) continue;
                        newSkillMPMultiplier[1] = doubleInput.Value;

                        double[] newSkillTypeDmgMultiplier = new double[Enum.GetValues(typeof(SkillType)).Length];
                        WriteLine(" - Type damage multiplier:");

                        for (int i = 0; i < Enum.GetValues(typeof(SkillType)).Length; i++)
                        {
                            Write($" {(SkillType) i}: ");
                            doubleInput = EnterDouble();
                            if (doubleInput == null) break;
                            newSkillTypeDmgMultiplier[i] = doubleInput.Value;
                        }
                        if (doubleInput == null) continue;

                        _metadata.SkillRarityMultiplier = newSkillRarityMultiplier;
                        _metadata.SkillMPMultiplier = newSkillMPMultiplier;
                        _metadata.SkillTypeDmgMultiplier = newSkillTypeDmgMultiplier;

                        _metadata.Save();
                        await LoadSkills();
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

    private Skill? EnterSkillInfo(out int dmgPt, out int healPt)
    {
        dmgPt = healPt = 0;

        Write(" All Type:");
        foreach (SkillType skillType in Enum.GetValues(typeof(SkillType)))
        {
            Write($" {(int) skillType} = {skillType} |");
        }
        WriteLine();
        Write(" All Rarity:");
        foreach (ItemRarity itemRarity in Enum.GetValues(typeof(ItemRarity)))
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
        SkillType? type = (SkillType?) EnterInt();
        if (type == null)
            return null;

        Write(" Rarity: ");
        ItemRarity? rarity = (ItemRarity?) EnterInt();
        if (rarity == null)
            return null;

        Write(" MP Cost: ");
        int? mpCost = EnterInt();
        if (mpCost == null)
            return null;

        WriteLine($" MP Point Left: {mpCost}");
        Write($" Damage Point (1 Pt = {_metadata.SkillMPMultiplier[0]} Dmg): ");
        int? dmg = EnterInt();
        if (dmg == null)
            return null;

        WriteLine($" MP Point Left: {mpCost - dmg.Value}");
        Write($" Heal Point (1 Pt = {_metadata.SkillMPMultiplier[1]} Heal): ");
        int? heal = EnterInt();
        if (heal == null)
            return null;

        Write(" Price (0 for auto-calc): ");
        int? price = EnterInt();
        if (price == null)
            return null;

        dmgPt = dmg.Value;
        healPt = heal.Value;

        return new Skill()
        {
            Name = name,
            Type = type.Value,
            Rarity = rarity.Value,
            MPCost = mpCost.Value,
            Damage = (int) (dmg.Value * _metadata.SkillMPMultiplier[0] * _metadata.SkillTypeDmgMultiplier[(int) type.Value]) * _metadata.SkillRarityMultiplier[(int) rarity.Value] / 100,
            Heal = (int) (heal.Value * _metadata.SkillMPMultiplier[1]) * _metadata.SkillRarityMultiplier[(int) rarity.Value] / 100,
            Price = price != 0 ? price.Value : Skill.CalcPrice((ItemRarity) rarity)
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

                        monsterToAdd.ID = _metadata.MonsterIDTracker[monsterToAdd.Floor - 1][(int) monsterToAdd.Type]++;
                        Monsters[monsterToAdd.ID] = monsterToAdd;
                        await MonsterDB.Add(monsterToAdd);

                        DrawLine('-');
                        WriteLine(" Added Successfully");
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

                            if (!Monsters.ContainsKey((int) id))
                            {
                                WriteLine(" Monster not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                monsterToUpdate = Monsters[(int) id];

                            DrawLine('-');
                            WriteLine(" Monster Info:");
                            WriteLine(" ID   | Name                      | Floor | Type   | ATK | DEF | HP");
                            WriteLine($" {monsterToUpdate.ID, -4} | {monsterToUpdate.Name, -25} | {monsterToUpdate.Floor, -5} | {monsterToUpdate.Type, -6} | {monsterToUpdate.ATK, -3} | {monsterToUpdate.DEF, -3} | {monsterToUpdate.HP, -2}");
                            DrawLine('-');

                            Monster? monsterToReplace = EnterMonsterInfo();
                            if (monsterToReplace == null) continue;

                            monsterToReplace.ID = monsterToUpdate.ID;
                            Monsters[monsterToReplace.ID] = monsterToReplace;
                            await MonsterDB.Update(monsterToReplace);

                            DrawLine('-');
                            WriteLine(" Updated Successfully");
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

                            if (!Monsters.ContainsKey((int) id))
                            {
                                WriteLine(" Monster not found");
                                ReadKey(true);
                                break;
                            }
                            else
                                monsterToDelete = Monsters[(int) id];

                            DrawLine('-');
                            WriteLine(" Monster Info:");
                            WriteLine(" ID   | Name                      | Floor | Type   | ATK | DEF | HP");
                            WriteLine($" {monsterToDelete.ID, -4} | {monsterToDelete.Name, -25} | {monsterToDelete.Floor, -5} | {monsterToDelete.Type, -6} | {monsterToDelete.ATK, -3} | {monsterToDelete.DEF, -3} | {monsterToDelete.HP, -2}");
                            DrawLine('-');

                            Write(" Are you sure you want to delete this monster? (Y/N): ");

                            var keyPressed = ReadKey(true).Key;
                            WriteLine();
                            if (keyPressed == ConsoleKey.Y || keyPressed == ConsoleKey.Enter)
                            {
                                Monsters.Remove(monsterToDelete.ID);
                                await MonsterDB.Delete(monsterToDelete.ID);

                                int deletedID = monsterToDelete.ID;
                                for (int i = deletedID; i < _metadata.MonsterIDTracker[monsterToDelete.Floor - 1][(int) monsterToDelete.Type]; i++)
                                {
                                    if (Monsters.ContainsKey(i + 1))
                                    {
                                        Monsters[i] = Monsters[i + 1];
                                        Monsters[i].ID = i;
                                        Monsters.Remove(i + 1);
                                        await MonsterDB.UpdateID(i + 1, i);
                                    }
                                }
                                _metadata.MonsterIDTracker[monsterToDelete.Floor - 1][(int) monsterToDelete.Type]--;

                                DrawLine('-');
                                WriteLine(" Deleted Successfully");
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
                        List<Monster> monsterList = [.. Monsters.Values];
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
                                WriteLine($" {monster.ID, -4} | {monster.Name, -25} | {monster.Floor, -5} | {monster.Type, -6} | {monster.ATK, -3} | {monster.DEF, -3} | {monster.HP, -2}");
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
        foreach (MonsterType monsterType in Enum.GetValues(typeof(MonsterType)))
        {
            Write($" {(int) monsterType} = {monsterType} |");
        }
        WriteLine();
        WriteLine($" Max Floor: {GameProgress.MaxFloor}");
        DrawLine('-');

        Write(" Name (25 max): ");
        string? name = ReadInput(false, 25);
        if (string.IsNullOrWhiteSpace(name))
            return null;

        Write(" Type: ");
        MonsterType? type = (MonsterType?) EnterInt();
        if (type == null)
            return null;

        Write(" Floor: ");
        int? floor = EnterInt();
        if (floor == null)
            return null;
        if (floor < 1 || floor > GameProgress.MaxFloor)
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
            Type = type.Value,
            Floor = floor.Value,
            ATK = atk.Value,
            DEF = def.Value,
            MaxHP = hp.Value,
            HP = hp.Value,
        };
    }

    private static int? EnterInt()
    {
        string? numInput = ReadInput();
        if (numInput == null)
            return null;
        if (string.IsNullOrWhiteSpace(numInput))
            return 0;
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