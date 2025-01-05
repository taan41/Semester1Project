[Serializable]
class Player : Entity
{
    public const int MaxSkillCount = 3;

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

    public void ChangeEquip(Equipment equipment)
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
            if (oldEquipment.BonusMaxHP < HP)
                HP -= oldEquipment.BonusMaxHP;
            else
                HP = 1;
            MaxMP -= oldEquipment.BonusMaxMP;
            MP -= oldEquipment.BonusMaxMP;
            
            EquipInventory.Add(oldEquipment);
        }

        ATK += equipment.BonusATK;
        MaxHP += equipment.BonusMaxHP;
        HP += equipment.BonusMaxHP;
        MaxMP += equipment.BonusMaxMP;
        MP += equipment.BonusMaxMP;

        EquipInventory.Remove(equipment);
    }

    public void ChangeSkill(int index, Skill skillToChange)
    {
        Skill changingSkill = Skills.ElementAt(index);
        Skills.RemoveAt(index);
        Skills.Insert(index, skillToChange);
        
        SkillInventory.Remove(skillToChange);
        AddItem(changingSkill);
    }

    public void AddItem<T>(T item) where T : Item
    {
        switch (item)
        {
            case Equipment equipment:
                AddItem(equipment);
                break;

            case Skill skill:
                AddItem(skill);
                break;

            case Gold gold:
                PlayerGold.Quantity += gold.Quantity;
                break;
        }
    }

    private void AddItem(Equipment equipment)
    {
        if (equipment.Type == EquipType.Weapon && EquippedWeapon == null)
            ChangeEquip(equipment);
        else if (equipment.Type == EquipType.Armor && EquippedArmor == null)
            ChangeEquip(equipment);
        else if (equipment.Type == EquipType.Ring && EquippedRing == null)
            ChangeEquip(equipment);
        else
        {
            EquipInventory.Add(equipment);
            EquipInventory.Sort(new EquipmentComparer());
        }
    }

    private void AddItem(Skill skill)
    {
        if (Skills.Count < MaxSkillCount)
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

    public void TradeItem(Item item, bool buying)
    {
        if (buying)
        {
            AddItem(item);
            PlayerGold.Quantity -= item.Price;
        }
        else
        {
            if (item is Equipment equip)
            {
                if (EquipInventory.Remove(equip))
                    PlayerGold.Quantity += equip.Price * Item.SellPricePercentage / 100;
            }
            else if (item is Skill skill)
            {
                if (SkillInventory.Remove(skill))
                    PlayerGold.Quantity += skill.Price * Item.SellPricePercentage / 100;

            }
        }
    }

    public override void Print()
    {
        base.Print();
        Console.WriteLine($"| ATK: {ATK,-3}");
        Console.Write(" HP ");
        GameUIHelper.UIMisc.DrawBar(HP, MaxHP, true, GameUIHelper.UIConstants.PlayerBarLen, ConsoleColor.Red);
        Console.Write(" MP ");
        GameUIHelper.UIMisc.DrawBar(MP, MaxMP, true, GameUIHelper.UIConstants.PlayerBarLen, ConsoleColor.Blue);
        Console.WriteLine($" Gold: {PlayerGold.Quantity}");
    }
}