abstract class Component(string name = "Temp name")
{
    public virtual string Name { get; set; } = name;

    public virtual void Print()
        => Console.Write($" {Name,-GameUIHelper.UIConstants.NameLen} ");
}