enum ItemRarity
{
    Common, Rare, Epic, Legendary
}

[Serializable]
abstract class Item : Component
{
    public ItemRarity Rarity { get; set; }

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