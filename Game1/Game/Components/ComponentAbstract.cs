abstract class Component
{
    public string Name { get; set; } = "Temp name";

    public Component() {}

    public Component(string name)
    {
        Name = name;
    }

    public virtual void Print()
        => Console.Write($" {Name,-UIHelper.UIConstants.NameLen} ");
}