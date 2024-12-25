abstract class Component
{
    public string Name { get; set; } = "Temp component name";

    public virtual void Print()
        => Console.WriteLine(" Temp component print  ");
}