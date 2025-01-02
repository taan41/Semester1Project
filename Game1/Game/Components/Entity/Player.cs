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

    public List<Equipment> GetEquipped()
    {
        List<Equipment> equipped = [];

        if (EquippedWeapon != null)
            equipped.Add(EquippedWeapon);
        if (EquippedArmor != null)
            equipped.Add(EquippedArmor);
        if (EquippedRing != null)
            equipped.Add(EquippedRing);
        
        return equipped;
    }

    public void Equip(Equipment equipment)
    {
        Equipment? oldEquipment = null;

        if (equipment.Type == EquipType.Weapon)
        {
            oldEquipment = EquippedWeapon;
            EquippedWeapon = equipment;
        }
        else if (equipment.Type == EquipType.Armor)
        {
            oldEquipment = EquippedArmor;
            EquippedArmor = equipment;
        }
        else if (equipment.Type ==  EquipType.Ring)
        {
            oldEquipment = EquippedRing;
            EquippedRing = equipment;
        }
        
        if (oldEquipment != null)
        {
            ATK -= oldEquipment.BonusATK;
            MaxHP -= oldEquipment.BonusMaxHP;
            HP -= oldEquipment.BonusMaxHP;
            MaxMP -= oldEquipment.BonusMaxMP;
            MP -= oldEquipment.BonusMaxMP;
            EquipInventory.Add(oldEquipment);
        }

        ATK += equipment.BonusATK;
        MaxHP += equipment.BonusMaxHP;
        HP += equipment.BonusMaxHP;
        MaxMP += equipment.BonusMaxMP;
        MP += equipment.BonusMaxMP;
    }

    public void ChangeSkill(int index, Skill skillToChange)
    {
        Skill changingSkill = Skills.ElementAt(index);
        Skills.RemoveAt(index);
        Skills.Insert(index, skillToChange);
        
        SkillInventory.Remove(skillToChange);
        AddSkill(changingSkill);
    }

    public void AddItem(Item item)
    {
        if (item is Equipment equipment)
            AddEquip(equipment);
        else if (item is Skill skill)
            AddSkill(skill);
        else if (item is Gold gold)
            PlayerGold.Quantity += gold.Quantity;
    }

    public void AddEquip(Equipment equipment)
    {
        if (equipment.Type == EquipType.Weapon && EquippedWeapon == null)
            Equip(equipment);
        else if (equipment.Type == EquipType.Armor && EquippedArmor == null)
            Equip(equipment);
        else if (equipment.Type == EquipType.Ring && EquippedRing == null)
            Equip(equipment);
        else
        {
            EquipInventory.Add(equipment);
            EquipInventory.Sort(new EquipmentComparer());
        }
    }

    public void AddSkill(Skill skill)
    {
        if (Skills.Count < 3)
            Skills.Add(skill);
        else
        {
            SkillInventory.Add(skill);
            SkillInventory.Sort(new SkillComparer());
        }
    }

    public void Regenerate(int hp, int mp)
    {
        HP += hp;
        MP += mp;
    }

    public void Regenerate()
        => Regenerate(MaxHP, MaxMP);

    public override void Print()
    {
        base.Print();
        Console.WriteLine($"| ATK: {ATK,-3}");
        Console.Write(" HP ");
        UIHelper.UIMisc.DrawBar(HP, MaxHP, true, UIHelper.UIConstants.PlayerBarLen, ConsoleColor.Red);
        Console.Write(" MP ");
        UIHelper.UIMisc.DrawBar(MP, MaxMP, true, UIHelper.UIConstants.PlayerBarLen, ConsoleColor.Blue);
        Console.WriteLine($" Gold: {PlayerGold.Quantity}");
    }
}