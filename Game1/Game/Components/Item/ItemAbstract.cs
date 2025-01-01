enum ItemRarity
{
    Common, Rare, Epic, Legendary
}

[Serializable]
abstract class Item : Component
{    
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;

    public Item() {}

    public Item(string name, ItemRarity rarity = ItemRarity.Common) : base(name)
    {
        Rarity = rarity;
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
}