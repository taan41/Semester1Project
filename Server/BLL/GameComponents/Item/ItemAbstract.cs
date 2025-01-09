enum ItemRarity
{
    Common, Rare, Epic, Legendary
}

[Serializable]
abstract class Item : Component
{
    public const int
        BasePrice = 100, RarityMultiplier = 75,
        EquipMultiplier = 30, SkillMultiplier = 10,
        SellPricePercentage = 60;

    public virtual int ID { get; set; }
    public virtual ItemRarity Rarity { get; set; } = ItemRarity.Common;
    public virtual int Price { get; set; }

    public Item() {}

    public Item(string name, ItemRarity rarity = ItemRarity.Common, int price = -1) : base(name)
    {
        Rarity = rarity;
        Price = price != -1 ? price : BasePrice * (100 + (int) Rarity * RarityMultiplier) / 100;
    }
}