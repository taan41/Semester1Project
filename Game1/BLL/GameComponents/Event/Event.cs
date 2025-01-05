enum EventType
{
    Fight, Shop, Camp, Treasure
}

class Event : Component
{
    public EventType Type { get; set; }

    public Event() {}

    public Event(EventType type) : base(type.ToString())
    {
        Type = type;
    }

    public override void Print()
    {
        base.Print();
        Console.WriteLine();
    }
}