namespace BLL.GameComponents.ItemComponents
{
    [Serializable]
    public class Skill : Item
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

        public Skill(Skill other) : base(other.Name, other.ItemRarity)
        {
            ID = other.ID;
            SkillType = other.SkillType;

            DamagePoint = other.DamagePoint;
            HealPoint = other.HealPoint;
            MPCost = other.MPCost;

            Price = other.Price != -1 ? other.Price : Config.ItemPriceBase * (100 + (int) ItemRarity * Config.ItemPriceRarityBonusPercentage) * (100 + Config.ItemPriceSkillBonusPercentage) / 10000;
        }
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