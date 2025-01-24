using BLL.GameComponents.ItemComponents;

namespace BLL.GameComponents.EntityComponents
{
    [Serializable]
    public class Player : Entity
    {
        private static int MaxSkillCount => Config.PlayerMaxSkillCount;

        public List<Equipment> Equipped { get; set; } = [];
        public List<Equipment> EquipInventory { get; set; } = [];
        public List<Skill> Skills { get; set; } = [];
        public List<Skill> SkillInventory { get; set; } = [];
        public Gold PlayerGold { get; set; } = new(0);

        public Player() {}

        public Player(string name, int atk, int def, int hp, int mp, int goldQuantity) : base(name, atk, def, hp, mp)
        {
            PlayerGold = new(goldQuantity);
        }

        public static Player DefaultPlayer() 
            => new("Player", Config.PlayerDefaultATK, Config.PlayerDefaultDEF, Config.PlayerDefaultHP, Config.PlayerDefaultMP, Config.PlayerDefaultGold);

        public void ChangeEquip(Equipment equipToChange)
        {
            Equipment? oldEquipment = null;

            Equipped.ForEach(equip =>
            {
                if (equip.EquipType == equipToChange.EquipType)
                    oldEquipment = equip;
            });
            
            ATK += equipToChange.BonusATKPoint * Config.EquipPtATKPercentage / 100;
            DEF += equipToChange.BonusDEFPoint * Config.EquipPtDEFPercentage / 100;
            MaxHP += equipToChange.BonusHPPoint * Config.EquipPtHPPercentage / 100;
            HP += equipToChange.BonusHPPoint * Config.EquipPtHPPercentage / 100;
            MaxMP += equipToChange.BonusMPPoint * Config.EquipPtMPPercentage / 100;
            MP += equipToChange.BonusMPPoint * Config.EquipPtMPPercentage / 100;

            Equipped.Add(equipToChange);
            EquipInventory.Remove(equipToChange);
            
            if (oldEquipment != null)
            {
                ATK -= oldEquipment.BonusATKPoint * Config.EquipPtATKPercentage / 100;
                DEF -= oldEquipment.BonusDEFPoint * Config.EquipPtDEFPercentage / 100;
                MaxHP -= oldEquipment.BonusHPPoint * Config.EquipPtHPPercentage / 100;
                int newHP = HP - oldEquipment.BonusHPPoint * Config.EquipPtHPPercentage / 100;
                HP = newHP < 1 ? 1 : newHP;
                MaxMP -= oldEquipment.BonusMPPoint * Config.EquipPtMPPercentage / 100;
                MP -= oldEquipment.BonusMPPoint * Config.EquipPtMPPercentage / 100;
                
                Equipped.Remove(oldEquipment);
                EquipInventory.Add(oldEquipment);
            }
        }

        public void ChangeSkill(int index, Skill skillToChange)
        {
            Skill removedSkill = Skills.ElementAt(index);
            Skills.RemoveAt(index);
            Skills.Insert(index, skillToChange);
            
            SkillInventory.Remove(skillToChange);
            SkillInventory.Add(removedSkill);
        }

        public void AddItem<T>(T item) where T : Item
        {
            switch (item)
            {
                case Equipment equipment:
                    AddEquip(equipment);
                    break;

                case Skill skill:
                    AddSkill(skill);
                    break;

                case Gold gold:
                    PlayerGold.Quantity += gold.Quantity;
                    break;
            }
        }

        private void AddEquip(Equipment equipToAdd)
        {
            bool alreadyEquipped = false;

            Equipped.ForEach(equip =>
            {
                if (equip.EquipType == equipToAdd.EquipType)
                {
                    alreadyEquipped = true;
                }
            });

            if (!alreadyEquipped)
            {
                ChangeEquip(equipToAdd);
            }
            else
            {
                EquipInventory.Add(equipToAdd);
                EquipInventory.Sort(new EquipmentComparer());
            }
        }

        private void AddSkill(Skill skill)
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
                        PlayerGold.Quantity += equip.Price * Config.ItemPriceSellingPercentage / 100;
                }
                else if (item is Skill skill)
                {
                    if (SkillInventory.Remove(skill))
                        PlayerGold.Quantity += skill.Price * Config.ItemPriceSellingPercentage / 100;

                }
            }
        }
    }
}