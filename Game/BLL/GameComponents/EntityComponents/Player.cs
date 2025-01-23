using BLL.GameComponents.ItemComponents;

namespace BLL.GameComponents.EntityComponents
{
    [Serializable]
    public class Player : Entity
    {
        private static int MaxSkillCount => Config.PlayerMaxSkillCount;

        public List<Equipment> Equipped { get; protected set; } = [];
        public List<Equipment> EquipInventory { get; protected set; } = [];
        public List<Skill> Skills { get; protected set; } = [];
        public List<Skill> SkillInventory { get; protected set; } = [];
        public Gold PlayerGold { get; protected set; } = new(0);

        public Player() {}

        public Player(string name, int atk, int def, int hp, int mp, int goldQuantity) : base(name, atk, def, hp, mp)
        {
            PlayerGold = new(goldQuantity);
        }

        public static Player DefaultPlayer() 
            => new("Player", Config.PlayerDefaultATK, Config.PlayerDefaultDEF, Config.PlayerDefaultHP, Config.PlayerDefaultMP, Config.PlayerDefaultGold);

        public void ChangeEquip(Equipment equipment)
        {
            Equipment? oldEquipment = null;

            Equipped.ForEach(equip =>
            {
                if (equip.EquipType == equipment.EquipType)
                {
                    oldEquipment = equip;
                    return;
                }
            });

            base.ATK += equipment.BonusATKPoint * ComponentAbstract.Config.EquipATKPtPercentage / 100;
            base.DEF += equipment.BonusDEFPoint * ComponentAbstract.Config.EquipDEFPtPercentage / 100;
            base.MaxHP += equipment.BonusHPPoint * ComponentAbstract.Config.EquipHPPtPercentage / 100;
            base.HP += equipment.BonusHPPoint * ComponentAbstract.Config.EquipHPPtPercentage / 100;
            base.MaxMP += equipment.BonusMPPoint * ComponentAbstract.Config.EquipMPPtPercentage / 100;
            base.MP += equipment.BonusMPPoint * ComponentAbstract.Config.EquipMPPtPercentage / 100;

            EquipInventory.Remove(equipment);
            
            if (oldEquipment != null)
            {
                base.ATK -= oldEquipment.BonusATKPoint * ComponentAbstract.Config.EquipATKPtPercentage / 100;
                base.DEF -= oldEquipment.BonusDEFPoint * ComponentAbstract.Config.EquipDEFPtPercentage / 100;
                base.MaxHP -= oldEquipment.BonusHPPoint * ComponentAbstract.Config.EquipHPPtPercentage / 100;
                int newHP = base.HP - oldEquipment.BonusHPPoint * ComponentAbstract.Config.EquipHPPtPercentage / 100;
                HP = newHP < 1 ? 1 : newHP;
                base.MaxMP -= oldEquipment.BonusMPPoint * ComponentAbstract.Config.EquipMPPtPercentage / 100;
                base.MP -= oldEquipment.BonusMPPoint * ComponentAbstract.Config.EquipMPPtPercentage / 100;
                
                EquipInventory.Add(oldEquipment);
            }
        }

        public void ChangeSkill(int index, Skill skillToChange)
        {
            Skill removedSkill = Skills.ElementAt(index);
            Skills.RemoveAt(index);
            Skills.Insert(index, skillToChange);
            
            SkillInventory.Remove(skillToChange);
            AddItem(removedSkill);
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
            bool alreadyEquipped = false;

            Equipped.ForEach(equip =>
            {
                if (equip.EquipType == equipment.EquipType)
                {
                    alreadyEquipped = true;
                    return;
                }
            });

            if (!alreadyEquipped)
                Equipped.Add(equipment);
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

        public void SetStats(int? atk, int? def, int? mHp, int? hp, int? mMp, int? mp, int? gold)
        {
            ATK = atk ?? ATK;
            DEF = def ?? DEF;
            MaxHP = mHp ?? MaxHP;
            HP = hp ?? HP;
            MaxMP = mMp ?? MaxMP;
            MP = mp ?? MP;
            PlayerGold.Quantity = gold ?? PlayerGold.Quantity;
        }

        public void Regenerate(int mHpPercentage, int mMpPercentage)
        {
            HP += MaxHP * mHpPercentage / 100;
            MP += MaxMP * mMpPercentage / 100;
        }

        public void Regenerate()
            => Regenerate(100, 100);

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
                        PlayerGold.Quantity += equip.Price * ComponentAbstract.Config.ItemSellPricePercentage / 100;
                }
                else if (item is Skill skill)
                {
                    if (SkillInventory.Remove(skill))
                        PlayerGold.Quantity += skill.Price * ComponentAbstract.Config.ItemSellPricePercentage / 100;

                }
            }
        }
    }
}