[Serializable]
class Player : Entity
{
    public List<Equipment> Equipments { get; set; } = [];
    public List<Skill> Skills { get; set; } = [];
    public List<Item> Inventory { get; set; } = [];
    public Gold Gold { get; set; } = new(0);

    public override void Print()
    {
        base.Print();
        Console.WriteLine($"| ATK: {Attack,-3}");
        Console.Write(" HP ");
        UIHandler.Misc.DrawBar(Health, MaxHealth, true, UIHandler.Numbers.PlayerBarLen, ConsoleColor.Red);
        Console.Write(" MP ");
        UIHandler.Misc.DrawBar(Mana, MaxMana, true, UIHandler.Numbers.PlayerBarLen, ConsoleColor.Blue);
    }
}