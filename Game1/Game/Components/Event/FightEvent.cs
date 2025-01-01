class FightEvent : Event
{
    public List<Monster> Monsters { get; set; } = [];
    public List<Item> Rewards { get; set; } = [];

    public FightEvent() {}

    public FightEvent(List<Monster> monsters, List<Item> rewards)
    {
        Type = EventType.Fight;
        Monsters.AddRange(monsters);
        Rewards.AddRange(rewards);
        Name = $"{Monsters[0].Type} Fight";
        // {(Monsters.Count > 1 ? $" + {Monsters.Count - 1}" : "")}
    }
}