enum TargetType
{
    Single, All, Random
}

[Serializable]
class Skill : Item
{
    public TargetType Target { get; set; }
    public int MPCost { get; set; } = 0;
    public int Damage { get; set; } = 0;
    public int Heal { get; set; } = 0;

    public override void Print()
    {
        base.Print();
        Console.Write($" | {Target} |");
        if (Damage > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($" [ ▲ {Damage} ]");
        }
        if (Heal > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($" [ + {Heal} ]");
        }
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($" ({MPCost} MP)");
        Console.ResetColor();
        Console.WriteLine("  ");

    }
}