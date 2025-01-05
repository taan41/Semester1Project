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

    public virtual int ID { get; set; } = -1;
    public virtual ItemRarity Rarity { get; set; } = ItemRarity.Common;
    public virtual int Price { get; set; } = 0;

    public Item() {}

    public Item(string name, ItemRarity rarity = ItemRarity.Common, int price = 0) : base(name)
    {
        Rarity = rarity;
        Price = price != 0 ? price : BasePrice * (100 + (int) Rarity * RarityMultiplier) / 100;
    }

    public override void Print()
    {
        Console.ForegroundColor = Rarity switch
        {
            ItemRarity.Rare => ConsoleColor.Cyan,
            ItemRarity.Epic => ConsoleColor.Magenta,
            ItemRarity.Legendary => ConsoleColor.DarkYellow,
            _ => ConsoleColor.White
        };
        base.Print();
        Console.ResetColor();
    }

    public virtual void PrintPrice(bool buying)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($" ({Price * (buying ? 100 : SellPricePercentage) / 100} G)");
        Console.ResetColor();

        Print();
    }
}