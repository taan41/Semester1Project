namespace BLL.GameComponents.ItemComponents
{

    [Serializable]
    public class Equipment : Item
    {
        public enum Type
        {
            Weapon, Armor, Relic
        }

        public Type EquipType { get; set; } = Type.Weapon;
        public int BonusATKPoint { get; set; } = 0;
        public int BonusDEFPoint { get; set; } = 0;
        public int BonusHPPoint { get; set; } = 0;
        public int BonusMPPoint { get; set; } = 0;

        public Equipment() {}

        public Equipment(Equipment other) : base(other.Name, other.ItemRarity)
        {
            ID = other.ID;
            EquipType = other.EquipType;

            BonusATKPoint = other.BonusATKPoint;
            BonusDEFPoint = other.BonusDEFPoint;
            BonusHPPoint = other.BonusHPPoint;
            BonusMPPoint = other.BonusMPPoint;
            
            Price = other.Price != -1 ? other.Price : Config.ItemPriceBase * (100 + (int) ItemRarity * Config.ItemPriceRarityBonusPercentage) * (100 + Config.ItemPriceEquipBonusPercentage) / 10000;
        }

        public static Equipment DefaultEquipment()
            => new() { Name = "Default Equipment", ItemRarity = Rarity.Common, EquipType = Type.Weapon, BonusATKPoint = 0, BonusDEFPoint = 0, BonusHPPoint = 0, BonusMPPoint = 0 };
    }

    class EquipmentComparer : IComparer<Equipment>
    {
        public int Compare(Equipment? x, Equipment? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;
            
            int rarityComparison = y.ItemRarity.CompareTo(x.ItemRarity);
            if (rarityComparison != 0) return rarityComparison;

            int typeComparison = x.EquipType.CompareTo(y.EquipType);
            if (typeComparison != 0) return typeComparison;

            int idComparison = x.ID.CompareTo(y.ID);
            if (idComparison != 0) return idComparison;

            return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
    }
}