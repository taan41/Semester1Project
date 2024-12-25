[Serializable]
class Player : Entity
{
    public List<Equipment> Equipments { get; set; } = [];
    public List<Skill> Skills { get; set; } = [];
    public Gold Gold { get; set; } = new(0);

    public override void Print()
    {
        Console.WriteLine($" {Name,-UINumbers.NameLen} | ATK: {Attack,-3}");
        Console.Write(" HP ");
        UIHandler.DrawBar(Health, MaxHealth, true, UINumbers.PlayerBarLen, ConsoleColor.Red);
        Console.Write(" MP ");
        UIHandler.DrawBar(Mana, MaxMana, true, UINumbers.PlayerBarLen, ConsoleColor.Blue);
    }
}