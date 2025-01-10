using System.Text.Json;
using static System.Console;
using static ServerUIHelper;

[Serializable]
class AssetMetadata
{
    private const string FileName = "AssetMetadata.json";
    private const string DirPath = "Persistence";

    private static JsonSerializerOptions _toJsonOption = new()
    {
        WriteIndented = true
    };
    
    public int[] EquipIDTracker { get; set; } = new int[Enum.GetValues(typeof(ItemRarity)).Length];
    public int[] SkillIDTracker { get; set; } = new int[Enum.GetValues(typeof(ItemRarity)).Length];
    public int[][] MonsterIDTracker { get; set; } = new int[GameProgress.MaxFloor][];

    public int[] EquipRarityPoint { get; set; } = new int[Enum.GetValues(typeof(ItemRarity)).Length];
    public int[] EquipPointMultiplier { get; set; } = new int[3];

    public int[] SkillRarityMultiplier { get; set; } = new int[Enum.GetValues(typeof(ItemRarity)).Length];
    public double[] SkillMPMultiplier { get; set; } = new double[2];
    public double[] SkillTypeDmgMultiplier { get; set; } = new double[Enum.GetValues(typeof(SkillType)).Length];

    public AssetMetadata()
    {
        for (int i = 0; i < MonsterIDTracker.Length; i++)
        {
            MonsterIDTracker[i] = new int[Enum.GetValues(typeof(MonsterType)).Length];
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(DirPath);
        File.WriteAllText(Path.Combine(DirPath, FileName), JsonSerializer.Serialize(this, _toJsonOption));
    }

    public static AssetMetadata Load()
    {
        AssetMetadata metadata = new();

        string path = Path.Combine(DirPath, FileName);
        if (!File.Exists(path)) return new AssetMetadata();

        try
        {
            metadata = JsonSerializer.Deserialize<AssetMetadata>(File.ReadAllText(path)) ?? new AssetMetadata();
        }
        catch (JsonException) { }

        return metadata;
    }
}

class AssetManagerPL
{
    public static string Header { get; } = "Asset Manager";

    public static AssetManagerPL Intance { get; } = new();

    private readonly AssetMetadata _metadata = AssetMetadata.Load();

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
                    ManageSkill();
                    break;

                case "3":
                    ManageMonster();
                    break;

                case "0": case null:
                    return;

                default: continue;
            }
        }
    }

    private async Task Initialize()
    {
        for (int i = 0; i < _metadata.EquipIDTracker.Length; i++)
        {
            _metadata.EquipIDTracker[i] = i * 100 + 1;
            _metadata.SkillIDTracker[i] = i * 100 + 1;
        }
        for (int i = 0; i < _metadata.MonsterIDTracker.Length; i++)
        {
            for (int j = 0; j < _metadata.MonsterIDTracker[i].Length; j++)
            {
                _metadata.MonsterIDTracker[i][j] = i * 1000 + j * 100 + 1;
            }
        }

        var (equipDB, _) = await EquipmentDB.GetAll();
        var (skillDB, _) = await SkillDB.GetAll();
        var (monsterDB, _) = await MonsterDB.GetAll();

        if (equipDB != null)
        {
            Equipments = equipDB;

            foreach (int id in Equipments.Keys)
                if (id >= _metadata.EquipIDTracker[id / 100])
                    _metadata.EquipIDTracker[id / 100] = id + 1;
        }

        if (skillDB != null)
        {
            Skills = skillDB;

            foreach (int id in Skills.Keys)
                if (id >= _metadata.SkillIDTracker[id / 100])
                    _metadata.SkillIDTracker[id / 100] = id + 1;
        }

        if (monsterDB != null)
        {
            Monsters = monsterDB;

            foreach (int id in Monsters.Keys)
                if (id >= _metadata.MonsterIDTracker[id / 1000][id % 1000 / 100])
                    _metadata.MonsterIDTracker[id / 1000][id % 1000 / 100] = id + 1;
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
            WriteLine($" 1 Stat Point = {_metadata.EquipPointMultiplier[0]} ATK = {_metadata.EquipPointMultiplier[1]} HP = {_metadata.EquipPointMultiplier[2]} MP");
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

                            equipToUpdate = Equipments[(int) id];

                            DrawLine('-');
                            WriteLine(" Equipment Info:");
                            WriteLine($" {equipToUpdate.ID, -3} | {equipToUpdate.Name, -25} | {equipToUpdate.Rarity, -10} | {equipToUpdate.Type, -6} | {equipToUpdate.BonusATK, -3} | {equipToUpdate.BonusHP, -2} | {equipToUpdate.BonusMP, -2} | {equipToUpdate.Price} G");
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

                    case "3":
                        Equipment? equipToDelete = null;
                        while (true)
                        {
                            Clear();
                            DrawHeader(Header);
                            WriteLine(" -- Delete Equipment");
                            Write(" Enter Equipment ID: ");

                            int? id = EnterInt();
                            if (id == null) break;

                            equipToDelete = Equipments[(int) id];

                            DrawLine('-');
                            WriteLine(" Equipment Info:");
                            WriteLine($" {equipToDelete.ID, -3} | {equipToDelete.Name, -25} | {equipToDelete.Rarity, -10} | {equipToDelete.Type, -6} | {equipToDelete.BonusATK, -3} | {equipToDelete.BonusHP, -2} | {equipToDelete.BonusMP, -2} | {equipToDelete.Price} G");
                            DrawLine('-');

                            Write(" Are you sure you want to delete this equipment? (Y/N): ");

                            var keyPressed = ReadKey(true).Key;
                            WriteLine();
                            if (keyPressed == ConsoleKey.Y || keyPressed == ConsoleKey.Enter)
                            {
                                Equipments.Remove(equipToDelete.ID);
                                await EquipmentDB.Delete(equipToDelete.ID);

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
                            WriteLine(" ID  | Name                      | Rarity     | Type   | ATK | HP | MP | Price");
                            DrawLine('-');
                            foreach (Equipment equip in equipmentList.Skip(page * 15).Take(15))
                            {
                                WriteLine($" {equip.ID, -3} | {equip.Name, -25} | {equip.Rarity, -10} | {equip.Type, -6} | {equip.BonusATK, -3} | {equip.BonusHP, -2} | {equip.BonusMP, -2} | {equip.Price} G");
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

                    case "5":
                        DrawLine('-');
                        WriteLine(" Stat points per rarity:");
                        for (int i = 0; i < Enum.GetValues(typeof(ItemRarity)).Length; i++)
                        {
                            Write($"   {(ItemRarity) i}: ");
                            try
                            {
                                _metadata.EquipRarityPoint[i] = Convert.ToInt32(ReadLine());
                            }
                            catch (FormatException)
                            {
                                WriteLine("Invalid input");
                                ReadKey(true);
                                continue;
                            }
                        }
                        WriteLine(" Stat point multiplier:");
                        Write("   ATK: ");
                        try
                        {
                            _metadata.EquipPointMultiplier[0] = Convert.ToInt32(ReadLine());
                        }
                        catch (FormatException)
                        {
                            WriteLine("Invalid input");
                            ReadKey(true);
                            continue;
                        }
                        Write("   HP: ");
                        try
                        {
                            _metadata.EquipPointMultiplier[1] = Convert.ToInt32(ReadLine());
                        }
                        catch (FormatException)
                        {
                            WriteLine("Invalid input");
                            ReadKey(true);
                            continue;
                        }
                        Write("   MP: ");
                        try
                        {
                            _metadata.EquipPointMultiplier[2] = Convert.ToInt32(ReadLine());
                        }
                        catch (FormatException)
                        {
                            WriteLine("Invalid input");
                            ReadKey(true);
                            continue;
                        }
                        _metadata.Save();
                        break;

                //     case "1":
                //         Write("name (25 max): ");
                //         string? name = ReadLine();
                //         if (string.IsNullOrWhiteSpace(name) || name.Length > GameUIHelper.UIConstants.NameLen) continue;

                //         Write("type (0 = weapon, 1 = armor, 2 = ring): ");
                //         EquipType type = (EquipType) Convert.ToInt32(ReadLine());

                //         Write("rarity (0, 3: Common, Rare, Epic, Legendary): ");
                //         ItemRarity rarity = (ItemRarity) Convert.ToInt32(ReadLine());
                //         int point = rarity switch
                //         {
                //             ItemRarity.Rare => 6,
                //             ItemRarity.Epic => 10,
                //             ItemRarity.Legendary => 15,
                //             _ => 3
                //         };
                //         WriteLine($"Recommend stat points : {point} (1 point = 2 atk = 10 hp = 5 mp)");

                //         WriteLine($"Point left: {point}");
                //         Write("atk point (1 pt = 2 atk): ");
                //         int atk = Convert.ToInt32(ReadLine());
                //         point -= atk;

                //         WriteLine($"Point left: {point}");
                //         Write("hp point (1 pt = 10 hp): ");
                //         int hpPt = Convert.ToInt32(ReadLine());
                //         point -= hpPt;

                //         WriteLine($"Point left: {point}");
                //         Write("mp point (1 pt = 5 mp): ");
                //         int mpPt = Convert.ToInt32(ReadLine());
                //         point -= mpPt;

                //         Equipment equipment = new(name, atk * 2, hpPt * 10, mpPt * 5, rarity, type);
                //         AssetManagerPL.Equipments[equipment.ID] = equipment;
                //         WriteLine("Added");
                //         ReadKey(true);
                //         continue;

                //     case "2":
                //         AssetManagerPL.Equipments.Values.ToList().ForEach(equip => WriteLine($"{equip.ID:d3} {equip.Name, -25} {equip.Rarity, -10} {equip.Type, -6} {equip.BonusATK} atk, {equip.BonusHP} hp, {equip.BonusMP} mp, {equip.Price} g"));
                //         ReadKey(true);
                //         continue;

                //     case "3":
                //         AssetManagerPL.Equipments.Clear();
                //         continue;

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
        Write(" Type:");
        foreach (EquipType equipType in Enum.GetValues(typeof(EquipType)))
        {
            Write($" {(int) equipType} = {equipType} |");
        }
        WriteLine();
        Write(" Rarity:");
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
        Write($" HP Point (1 Pt = {_metadata.EquipPointMultiplier[1]} HP): ");
        int? hpPt = EnterInt();
        if (hpPt == null)
            return null;
        point -= hpPt.Value;

        WriteLine($" - Stat Point Left: {point}");
        Write($" MP Point (1 Pt = {_metadata.EquipPointMultiplier[2]} MP): ");
        int? mpPt = EnterInt();
        if (mpPt == null)
            return null;
        point -= mpPt.Value;

        Write(" Price (-1 for auto-calc): ");
        int price = EnterInt() ?? -1;

        return new Equipment()
        {
            Name = name,
            Type = type.Value,
            Rarity = rarity.Value,
            BonusATK = atk.Value * _metadata.EquipPointMultiplier[0],
            BonusHP = hpPt.Value * _metadata.EquipPointMultiplier[1],
            BonusMP = mpPt.Value * _metadata.EquipPointMultiplier[2],
            Price = price == -1 ? (atk.Value + hpPt.Value + mpPt.Value) * 10 : price
        };
    }

    private void ManageSkill()
    {
        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" -- Skills");
            Write(" Rarity Multiplier:\n  ");
            for (int i = 0; i < Enum.GetValues(typeof(ItemRarity)).Length; i++)
            {
                Write($" {(ItemRarity) i}: {_metadata.SkillRarityMultiplier[i]}% |");
            }
            WriteLine();
            WriteLine($" 1 MP = {_metadata.SkillMPMultiplier[0]:F1} Damage = {_metadata.SkillMPMultiplier[1]:F1} Heal");
            Write(" Type Damage Multiplier:\n  ");
            for (int i = 0; i < Enum.GetValues(typeof(SkillType)).Length; i++)
            {
                Write($" {(SkillType) i}: {_metadata.SkillTypeDmgMultiplier[i]:F1} |");
            }
            WriteLine();
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
                    case "5":
                        DrawLine('-');
                        WriteLine(" Rarity multiplier (%):");
                        for (int i = 0; i < Enum.GetValues(typeof(ItemRarity)).Length; i++)
                        {
                            Write($"   {(ItemRarity) i}: ");
                            try
                            {
                                _metadata.SkillRarityMultiplier[i] = Convert.ToInt32(ReadLine());
                            }
                            catch (FormatException)
                            {
                                WriteLine("Invalid input");
                                ReadKey(true);
                                continue;
                            }
                        }
                        WriteLine(" MP multiplier:");
                        Write("   Damage: ");
                        try
                        {
                            _metadata.SkillMPMultiplier[0] = Convert.ToDouble(ReadLine());
                        }
                        catch (FormatException)
                        {
                            WriteLine("Invalid input");
                            ReadKey(true);
                            continue;
                        }
                        Write("   Heal: ");
                        try
                        {
                            _metadata.SkillMPMultiplier[1] = Convert.ToDouble(ReadLine());
                        }
                        catch (FormatException)
                        {
                            WriteLine("Invalid input");
                            ReadKey(true);
                            continue;
                        }
                        WriteLine(" Type damage multiplier:");
                        for (int i = 0; i < Enum.GetValues(typeof(SkillType)).Length; i++)
                        {
                            Write($"   {(SkillType) i}: ");
                            try
                            {
                                _metadata.SkillTypeDmgMultiplier[i] = Convert.ToDouble(ReadLine());
                            }
                            catch (FormatException)
                            {
                                WriteLine("Invalid input");
                                ReadKey(true);
                                continue;
                            }
                        }
                        _metadata.Save();
                        continue;
                    // case "1":
                    //     Write("name (25 max): ");
                    //     string? name = ReadLine();
                    //     if (string.IsNullOrWhiteSpace(name) || name.Length > GameUIHelper.UIConstants.NameLen) continue;

                    //     Write("type (0, 2: Single, Random, All): ");
                    //     SkillType type = (SkillType) Convert.ToInt32(ReadLine());

                    //     Write("rarity (0, 3: Common, Rare, Epic, Legendary): ");
                    //     ItemRarity rarity = (ItemRarity) Convert.ToInt32(ReadLine());
                    //     int multiplier = rarity switch
                    //     {
                    //         ItemRarity.Rare => 200,
                    //         ItemRarity.Epic => 300,
                    //         ItemRarity.Legendary => 450,
                    //         _ => 100
                    //     };
                    //     WriteLine($"Multiplier based on rairty : {multiplier}%");
                    //     WriteLine("1 mp = 1 heal = 2 damage");

                    //     Write("mp cost: ");
                    //     int mp = Convert.ToInt32(ReadLine());

                    //     WriteLine($"MP left: {mp}");
                    //     Write("dmg point (1 pt = 1 mp = 2 dmg): ");
                    //     int dmgPt = Convert.ToInt32(ReadLine());

                    //     WriteLine($"MP left: {mp - dmgPt}");
                    //     Write("heal point (1 pt = 1 mp = 1 heal): ");
                    //     int healPt = Convert.ToInt32(ReadLine());

                    //     Skill skill = new(name, dmgPt * 2 * multiplier / 100, healPt * multiplier / 100, mp, rarity, type);
                    //     AssetManagerPL.Skills[skill.ID] = skill;
                    //     WriteLine("Added");
                    //     ReadKey(true);
                    //     continue;

                    // case "2":
                    //     AssetManagerPL.Skills.Values.ToList().ForEach(skill => WriteLine($"{skill.ID:d3} {skill.Name, -25} {skill.Rarity, -10} {skill.Type, -6} {skill.Damage} dmg, {skill.Heal} heal, {skill.MPCost} mp, {skill.Price} g"));
                    //     ReadKey(true);
                    //     continue;

                    // case "3":
                    //     AssetManagerPL.Skills.Clear();
                    //     continue;

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

    private static void ManageMonster()
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
                    // case "1":
                    //     Write("name (25 max): ");
                    //     string? name = ReadLine();
                    //     if (string.IsNullOrWhiteSpace(name) || name.Length > GameUIHelper.UIConstants.NameLen) continue;

                    //     Write("type (0 = normal, 1 = elite, 2 = boss): ");
                    //     MonsterType type = (MonsterType) Convert.ToInt32(ReadLine());

                    //     Write("floor: ");
                    //     int floor = Convert.ToInt32(ReadLine());

                    //     Write("atk: ");
                    //     int atk = Convert.ToInt32(ReadLine());

                    //     Write("hp: ");
                    //     int hp = Convert.ToInt32(ReadLine());

                    //     Monster monster = new(name, atk, hp, floor, type);
                    //     AssetManagerPL.Monsters[monster.ID] = monster;
                    //     WriteLine("Added");
                    //     ReadKey(true);
                    //     continue;

                    // case "2":
                    //     AssetManagerPL.Monsters.Values.ToList().ForEach(monster => WriteLine($"{monster.ID:d4} {monster.Name, -25} {monster.Type, -6} floor: {monster.Floor}, atk: {monster.ATK}, hp: {monster.HP}"));
                    //     ReadKey(true);
                    //     continue;

                    // case "3":
                    //     AssetManagerPL.Monsters.Clear();
                    //     continue;

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

    private static int? EnterInt()
    {
        string? numInput = ReadInput();
        if (numInput == null)
            return null;
        if (string.IsNullOrWhiteSpace(numInput))
            return 0;
        if (int.TryParse(numInput, out int result))
            return result;
        return null;
    }

    private static double? EnterDouble()
    {
        try
        {
            string? numInput = ReadInput();
            if (numInput == null) return null;
            if (!string.IsNullOrWhiteSpace(numInput) && double.TryParse(numInput, out double result))
                return result;
        }
        catch (FormatException)
        {
            WriteLine("Invalid input");
            ReadKey(true);
        }
        return null;
    }
}