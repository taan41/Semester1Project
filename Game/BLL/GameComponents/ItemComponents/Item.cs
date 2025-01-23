namespace BLL.GameComponents.ItemComponents
{
    [Serializable]
    public abstract class Item : ComponentAbstract
    {
        public enum Rarity
        {
            Common, Rare, Epic, Legendary
        }

        public virtual int ID { get; set; } = -1;
        public virtual Rarity ItemRarity { get; set; } = Rarity.Common;
        public virtual int Price { get; set; } = 0;

        public Item() {}

        public Item(string name, Rarity itemRarity = Rarity.Common) : base(name)
        {
            ItemRarity = itemRarity;
        }
    }
}