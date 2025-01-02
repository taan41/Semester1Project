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

        Name = Monsters[0].Type switch
        {
            MonsterType.Boss => "(!!!) Boss Fight",
            MonsterType.Elite => "(!) Elite Fight",
            _ => "Normal Fight"
        };
    }
}