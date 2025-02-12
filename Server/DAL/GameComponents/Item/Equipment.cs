namespace DAL.GameComponents.Item
{

    [Serializable]
    public class Equipment : GameItem
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

        public Equipment(Equipment other) : base(other)
        {
            EquipType = other.EquipType;
            BonusATKPoint = other.BonusATKPoint;
            BonusDEFPoint = other.BonusDEFPoint;
            BonusHPPoint = other.BonusHPPoint;
            BonusMPPoint = other.BonusMPPoint;
        }
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