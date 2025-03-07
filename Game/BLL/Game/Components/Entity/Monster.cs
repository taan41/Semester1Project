using System.Text.Json.Serialization;

namespace BLL.Game.Components.Entity
{
    [Serializable]
    public class Monster : GameEntity
    {
        public enum Type
        {
            Normal, Elite, Boss
        }

        public int ID { get; set; }
        public int Floor { get; set; } = 1;
        public Type MonsterType { get; set; }
        
        [JsonIgnore]
        public int Power { get => (ATK * GameConfig.MonsterPowerATKPercentage + MaxHP * GameConfig.MonsterPowerHPPercentage) / 100; }

        public Monster() {}

        public Monster(int id, string name, int floor, int type, int atk, int def, int hp) : base(name, atk, def, hp, 0)
        {
            ID = id;
            MonsterType = (Type) type;
            Floor = floor;
        }

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

        public static Monster DefaultMonster()
            => new() { Name = "Default Monster", ATK = 0, DEF = 0, HP = 0, MonsterType = Type.Normal };
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