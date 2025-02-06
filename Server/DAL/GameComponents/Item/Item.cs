namespace DAL.GameComponents.Item
{
    [Serializable]
    public abstract class GameItem : GameComponent
    {
        public enum Rarity
        {
            Common, Rare, Epic, Legendary
        }

        public virtual int ID { get; set; } = -1;
        public virtual Rarity ItemRarity { get; set; } = Rarity.Common;
        public virtual int Price { get; set; } = 0;

        public GameItem() {}

        public GameItem(GameItem other)
        {
            Name = other.Name;
            ID = other.ID;
            ItemRarity = other.ItemRarity;
            Price = other.Price;
        }
    }
}