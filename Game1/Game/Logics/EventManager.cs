class EventManager
{
    private readonly Random _random;
    private readonly GameData _gameData;
    private readonly List<List<Event>> _events;

    public EventManager(GameData gameData)
    {
        _gameData = gameData;
        _random = new(_gameData.Seed);
        _events = GenerateEvents(_random);
    }

    public List<Event> GetEvents()
        => _events.ElementAt(_gameData.Progress.Room + (_gameData.Progress.Floor - 1) * GameProgress.MaxRoom);

    private List<List<Event>> GenerateEvents(Random rng)
    {
        List<List<Event>> events = [];

        for (int room = 1; room <= GameProgress.MaxRoom * GameProgress.MaxFloor; room++)
        {
            int curRoom = room % GameProgress.MaxRoom;
            int curFloor = (room - 1) / GameProgress.MaxRoom + 1;
            int monsterPower = 15 + (curRoom - 1) * 1 + (curFloor - 1) * GameProgress.MaxRoom;

            if (curRoom % GameProgress.MaxRoom == 0)
            {
                events.Add([GenerateBossFight(rng, curFloor, monsterPower)]);
                continue;
            }

            if (_gameData.Progress.Room % 10 == 0)
            {
                events.Add([new(EventType.Camp)]);
                continue;
            }
            
            int eventQuantity = rng.Next(1, 101) > 50 ? 3 : 2;

            List<Event> possibleEvents = [];
            while(eventQuantity-- > 0)
                possibleEvents.Add(GenerateNormalEvent(rng, curRoom, curFloor, monsterPower));

            events.Add(possibleEvents);
        }

        return events;
    }

    private static Event GenerateNormalEvent(Random rng, int room, int floor, int monsterPower)
    {
        int randomValue = rng.Next(1, 101);

        if (randomValue > 75 && room > 3)
            return GenerateEliteFight(rng, floor, monsterPower);

        return GenerateNormalFight(rng, floor, monsterPower);
    }

    private static FightEvent GenerateNormalFight(Random rng, int floor, int monsterPower)
    {
        int randomValue = rng.Next(1, 101);
        int normalQuantity = randomValue > 35 ? (randomValue > 90 ? 4 : 3) : 2;
        
        Gold gold = CalculateGold(monsterPower, normalQuantity, 0, 0, randomValue);
        List<Monster> monsters = GenerateMonsters(rng, floor, monsterPower, normalQuantity);

        return new FightEvent(monsters, [gold]);
    }

    private static FightEvent GenerateEliteFight(Random rng, int floor, int monsterPower)
    {
        int randomValue = rng.Next(1, 101);
        int eliteQuantity = randomValue > 90 ? 2 : 1;
        int normalQuantity = randomValue > 50 ? (randomValue > 75 ? 0 : 1) : 2;
        
        Gold gold = CalculateGold(monsterPower, normalQuantity, eliteQuantity, 0, randomValue);
        List<Monster> monsters = GenerateMonsters(rng, floor, monsterPower, normalQuantity, eliteQuantity);

        return new FightEvent(monsters, [gold]);
    }

    private static FightEvent GenerateBossFight(Random rng, int floor, int monsterPower)
    {
        int randomValue = rng.Next(1, 101);
        int bossQuantity = randomValue > 99 ? 2 : 1;
        int eliteQuantity = randomValue > 15 ? (randomValue > 99 ? 0 : 1) : 2;
        int normalQuantity = randomValue > 15 ? (randomValue > 99 ? 0 : 3) : 2;
        
        Gold gold = CalculateGold(monsterPower, normalQuantity, eliteQuantity, bossQuantity, randomValue);
        List<Monster> monsters = GenerateMonsters(rng, floor, monsterPower, normalQuantity, eliteQuantity, bossQuantity);

        return new FightEvent(monsters, [gold]);
    }

    private static List<Monster> GenerateMonsters(Random rng, int floor, int monsterPower, int normalQuantity, int eliteQuantity = 0, int bossQuantity = 0)
    {
        List<Monster> monsters = [];
        List<Monster> availableMonsters = GameAssets.BossMonsterList.FindAll(monster => monster.Floor == floor);

        while(bossQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[rng.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower * 5));
        }

        availableMonsters = GameAssets.EliteMonsterList.FindAll(monster => monster.Floor == floor);
        while(eliteQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[rng.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower * 2));
        }

        availableMonsters = GameAssets.NormalMonsterList.FindAll(monster => monster.Floor == floor);
        while(normalQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[rng.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower));
        }

        return monsters;
    }

    private static Gold CalculateGold(int monsterPower, int normalQuantity, int eliteQuantity, int bossQuantity, int randomValue)
        => new((int) (monsterPower * (1.25 + 0.25 * normalQuantity + 0.6 * eliteQuantity + 2 * bossQuantity) * (0.85 + 0.3 * randomValue / 100)));
}