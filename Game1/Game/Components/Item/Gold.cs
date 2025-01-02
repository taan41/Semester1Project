[Serializable]
class Gold : Item
{
    public int Quantity { get; set; }

    public Gold(int quantity)
    {
        Quantity = quantity;
        Name = "Gold";
    }

    public override void Print()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($" Gold ({Quantity})  ");
        Console.ResetColor();
    }
}