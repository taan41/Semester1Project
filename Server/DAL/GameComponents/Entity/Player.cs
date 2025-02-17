using DAL.GameComponents.Item;

namespace DAL.GameComponents.Entity
{
    [Serializable]
    public class Player : GameEntity
    {
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
    }
}