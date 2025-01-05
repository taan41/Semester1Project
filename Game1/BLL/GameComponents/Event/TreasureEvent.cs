class TreasureEvent : Event
{
    public List<Item> Treasures { get; set; } = [];

    public TreasureEvent() {}

    public TreasureEvent(List<Item> treasures) : base(EventType.Treasure)
        => Treasures = treasures;
}