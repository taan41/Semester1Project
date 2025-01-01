class EventManager(int seed)
{
    private readonly Random random = new(seed);

    public List<Event> GenerateEvents(GameData gameData)
    {
        if (gameData.Progress.Room % 5 == 0)
            return [new(EventType.Camp)];
        
        int weight = 15 + (gameData.Progress.Room - 1) * 1 + (gameData.Progress.Floor - 1) * 20;
        int eventQuantity = random.Next(1, 101) > 50 ? 3 : 2;

        List<Event> events = [];
        while(eventQuantity-- > 0)
            events.Add(GenerateNormalEvent(gameData, weight));

        return events;
    }

    private Event GenerateNormalEvent(GameData gameData, int weight)
    {
        int randomValue = random.Next(1, 101);
        int monsterQuantity = randomValue > 60 ? (randomValue > 90 ? 4 : 2) : 3;
        
        Gold gold = new((int) (weight * (1.2 + 0.2 * monsterQuantity) * random.Next(85, 120) / 100));

        List<Monster> availableMonsters = GameAssets.MonsterList.FindAll(monster => monster.Power <= weight);
        List<Monster> monsters = [];

        while(monsterQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[random.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, weight));
        }

        return new FightEvent(monsters, [gold]);
    }
}