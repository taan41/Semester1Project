namespace BLL.Game.Components.Item
{
    [Serializable]
    public class Skill : GameItem
    {
        public enum Type
        {
            Single, Random, All
        }
        
        public Type SkillType { get; set; }
        public int DamagePoint { get; set; } = 0;
        public int HealPoint { get; set; } = 0;
        public int MPCost { get; set; } = 0;

        public Skill() {}

        public Skill(int id, string name, int rarity, int skillType, int damagePoint, int healPoint, int mpCost, int price = -1) : base(name, (Rarity) rarity)
        {
            ID = id;
            SkillType = (Type) skillType;
            DamagePoint = damagePoint;
            HealPoint = healPoint;
            MPCost = mpCost;

            Price = price != -1 ? price : GameConfig.ItemPriceBase * (100 + (int) ItemRarity * GameConfig.ItemPriceRarityBonusPercentage) * (100 + GameConfig.ItemPriceSkillBonusPercentage) / 10000;
        }

        public Skill(Skill other) : base(other.Name, other.ItemRarity)
        {
            ID = other.ID;
            SkillType = other.SkillType;

            DamagePoint = other.DamagePoint;
            HealPoint = other.HealPoint;
            MPCost = other.MPCost;

            Price = other.Price != -1 ? other.Price : GameConfig.ItemPriceBase * (100 + (int) ItemRarity * GameConfig.ItemPriceRarityBonusPercentage) * (100 + GameConfig.ItemPriceSkillBonusPercentage) / 10000;
        }

        public static Skill DefaultSkill()
            => new() { Name = "Default Skill", ItemRarity = Rarity.Common, SkillType = Type.Single, DamagePoint = 0, HealPoint = 0, MPCost = 0 };
    }

    class SkillComparer : IComparer<Skill>
    {
        public int Compare(Skill? x, Skill? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;
            
            int rarityComparison = y.ItemRarity.CompareTo(x.ItemRarity);
            if (rarityComparison != 0) return rarityComparison;

            int typeComparison = x.SkillType.CompareTo(y.SkillType);
            if (typeComparison != 0) return typeComparison;

            int idComparison = x.ID.CompareTo(y.ID);
            if (idComparison != 0) return idComparison;

            return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
    }
}