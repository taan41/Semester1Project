using System.Text.Json.Serialization;

namespace BLL.GameComponents.EntityComponents
{
    [Serializable]
    public class Monster : Entity
    {
        public enum Type
        {
            Normal, Elite, Boss
        }

        public int ID { get; set; }
        public int Floor { get; set; } = 1;
        public Type MonsterType { get; set; }
        
        [JsonIgnore]
        public int Power { get => (ATK * Config.MonsterPowerATKPercentage + MaxHP * Config.MonsterPowerHPPercentage) / 100; }

        public Monster() {}

        public Monster(Monster other, int targetPower = 0) : base(other.Name, other.ATK, other.DEF, other.HP, 0)
        {
            ID = other.ID;
            MonsterType = other.MonsterType;
            Floor = other.Floor;
            
            if (targetPower != 0 && targetPower != Power)
                ScaleStat(targetPower);
        }

        public void ScaleStat(int targetPower)
        {
            int ogPower = Power;
            ATK = ATK * targetPower / ogPower;
            MaxHP = MaxHP * targetPower / ogPower;
            HP = HP * targetPower / ogPower;
        }
    }

    class MonsterComparer : IComparer<Monster>
    {
        public int Compare(Monster? x, Monster? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int idComparison = x.ID.CompareTo(y.ID);
            if (idComparison != 0) return idComparison;

            int floorComparison = x.Floor.CompareTo(y.Floor);
            if (floorComparison != 0) return floorComparison;

            int typeComparison = x.MonsterType.CompareTo(y.MonsterType);
            if (typeComparison != 0) return typeComparison;

            return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
    }
}