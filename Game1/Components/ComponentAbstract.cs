abstract class Component
{
    public string Name { get; set; } = "Temp name";

    public virtual void Print()
        => Console.Write($" {Name,-UIHandler.Numbers.NameLen} ");
}