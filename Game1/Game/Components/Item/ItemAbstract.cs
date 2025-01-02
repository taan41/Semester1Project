enum ItemRarity
{
    Common, Rare, Epic, Legendary
}

[Serializable]
abstract class Item : Component
{
    public const int
        BasePrice = 100, RarityMultiplier = 30,
        EquipMultiplier = 50, SkillMultiplier = 0,
        SellPricePercentage = 60;


    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    public int Price { get; set; }

    public Item() {}

    public Item(string name, ItemRarity rarity = ItemRarity.Common, int price = -1) : base(name)
    {
        Rarity = rarity;
        Price = price != -1 ? price : BasePrice * (100 + (int) Rarity * RarityMultiplier) / 100;
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