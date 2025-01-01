class EventManager(int seed)
{
    private readonly Random random = new(seed);

    public List<Event> GenerateEvents(GameData gameData)
    {
        int monsterPower = 15 + (gameData.Progress.Room - 1) * 1 + (gameData.Progress.Floor - 1) * GameProgress.MaxRoom;

        if (gameData.Progress.Room == GameProgress.MaxRoom)
            return [GenerateBossFight(gameData, monsterPower)];

        if (gameData.Progress.Room % 10 == 0)
            return [new(EventType.Camp)];
        
        int eventQuantity = random.Next(1, 101) > 50 ? 3 : 2;

        List<Event> events = [];
        while(eventQuantity-- > 0)
            events.Add(GenerateNormalEvent(gameData, monsterPower));

        return events;
    }

    private Event GenerateNormalEvent(GameData gameData, int monsterPower)
    {
        int randomValue = random.Next(1, 101);

        if (randomValue > 15 && gameData.Progress.Room > 3)
            return GenerateEliteFight(gameData, monsterPower);

        return GenerateNormalFight(gameData, monsterPower);
    }

    private FightEvent GenerateNormalFight(GameData gameData, int monsterPower)
    {
        int randomValue = random.Next(1, 101);
        int normalQuantity = randomValue > 60 ? (randomValue > 90 ? 4 : 2) : 3;
        
        Gold gold = CalculateGold(monsterPower, normalQuantity, 0, 0, randomValue);

        List<Monster> availableMonsters = GameAssets.NormalMonsterList.FindAll(monster => monster.Floor == gameData.Progress.Floor);
        List<Monster> monsters = [];

        while(normalQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[random.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower));
        }

        return new FightEvent(monsters, [gold]);
    }

    private FightEvent GenerateEliteFight(GameData gameData, int monsterPower)
    {
        int randomValue = random.Next(1, 101);
        int eliteQuantity = randomValue > 90 ? 2 : 1;
        int normalQuantity = randomValue > 50 ? (randomValue > 75 ? 0 : 1) : 2;
        
        Gold gold = CalculateGold(monsterPower, normalQuantity, eliteQuantity, 0, randomValue);

        List<Monster> availableMonsters = GameAssets.EliteMonsterList.FindAll(monster => monster.Floor == gameData.Progress.Floor);
        List<Monster> monsters = [];

        while(eliteQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[random.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower * 2));
        }

        availableMonsters = GameAssets.NormalMonsterList.FindAll(monster => monster.Floor == gameData.Progress.Floor);
        while(normalQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[random.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower));
        }

        return new FightEvent(monsters, [gold]);
    }

    private FightEvent GenerateBossFight(GameData gameData, int monsterPower)
    {
        int randomValue = random.Next(1, 101);
        int bossQuantity = randomValue > 99 ? 2 : 1;
        int eliteQuantity = randomValue > 15 ? 1 : 2;
        int normalQuantity = randomValue > 15 ? (randomValue > 99 ? 0 : 3) : 2;
        
        Gold gold = CalculateGold(monsterPower, normalQuantity, eliteQuantity, bossQuantity, randomValue);

        List<Monster> availableMonsters = GameAssets.BossMonsterList.FindAll(monster => monster.Floor == gameData.Progress.Floor);
        List<Monster> monsters = [];

        while(bossQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[random.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower * 5));
        }

        availableMonsters = GameAssets.EliteMonsterList.FindAll(monster => monster.Floor == gameData.Progress.Floor);
        while(eliteQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[random.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower * 2));
        }

        availableMonsters = GameAssets.NormalMonsterList.FindAll(monster => monster.Floor == gameData.Progress.Floor);
        while(normalQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[random.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower));
        }

        return new FightEvent(monsters, [gold]);
    }

    private static Gold CalculateGold(int monsterPower, int normalQuantity, int eliteQuantity, int bossQuantity, int randomValue)
        => new((int) (monsterPower * (1.25 + 0.25 * normalQuantity + 1 * eliteQuantity + 2 * bossQuantity) * (0.8 + 0.5 * randomValue / 100)));
}