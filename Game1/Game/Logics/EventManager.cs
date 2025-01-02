class EventManager
{
    private static readonly Dictionary<ItemRarity, (int weight, int powerMultiplier)> ItemWeights = new()
    {
        [ItemRarity.Common] = (BasePower + GameProgress.MaxRoom, -100),
        [ItemRarity.Rare] = ((BasePower + GameProgress.MaxFloor) * 30 / 100, 0),
        [ItemRarity.Epic] = (- (BasePower + GameProgress.MaxRoom) * 70 / 100, 100),
        [ItemRarity.Legendary] = (- (BasePower + GameProgress.MaxRoom) / 2, 60),
    };
    private const int BasePower = 15;

    private readonly Random _random;
    private readonly GameData _gameData;
    private readonly AssetManager _assetManager;
    private readonly List<List<Event>> _events;

    public EventManager(GameData gameData, AssetManager assetManager)
    {
        _gameData = gameData;
        _assetManager = assetManager;
        _random = new(_gameData.Seed);
        _events = GenerateEvents(_random);
    }

    public List<Event> GetEvents()
        => _events.ElementAt(_gameData.Progress.Room + (_gameData.Progress.Floor - 1) * GameProgress.MaxRoom);

    private List<List<Event>> GenerateEvents(Random rng)
    {
        List<List<Event>> events = [];

        for (int room = 0; room < GameProgress.MaxRoom * GameProgress.MaxFloor; room++)
        {
            int curRoom = room % GameProgress.MaxRoom + 1;
            int curFloor = room / GameProgress.MaxRoom + 1;
            int monsterPower = BasePower + (curRoom - 1) * 1 + (curFloor - 1) * GameProgress.MaxRoom;

            if (curRoom % GameProgress.MaxRoom == 0)
            {
                events.Add([GenerateBossFight(curFloor, monsterPower)]);
                continue;
            }

            if (curRoom % 10 == 5)
            {
                events.Add([new(EventType.Treasure)]);
                continue;
            }

            if (curRoom % 10 == 0)
            {
                events.Add([new(EventType.Camp), GenerateShop(monsterPower)]);
                continue;
            }
            
            int eventQuantity = rng.Next(1, 101) > 50 ? 3 : 2;

            List<Event> possibleEvents = [];
            while(eventQuantity-- > 0)
                possibleEvents.Add(GenerateNormalEvent(curRoom, curFloor, monsterPower));

            events.Add(possibleEvents);
        }

        return events;
    }

    private Event GenerateNormalEvent(int room, int floor, int monsterPower)
    {
        int randomValue = _random.Next(1, 101);

        if (randomValue > 80 && room > 5)
            return GenerateEliteFight(floor, monsterPower);

        return GenerateNormalFight(floor, monsterPower);
    }

    private FightEvent GenerateNormalFight(int floor, int monsterPower)
    {
        int randomValue = _random.Next(1, 101);
        int normalQuantity = randomValue > 35 ? (randomValue > 90 ? 4 : 3) : 2;
        
        Gold gold = CalculateGold(monsterPower, normalQuantity, 0, 0, randomValue);
        List<Monster> monsters = GenerateMonsters(floor, monsterPower, normalQuantity);

        return new FightEvent(monsters, [gold]);
    }

    private FightEvent GenerateEliteFight(int floor, int monsterPower)
    {
        int randomValue = _random.Next(1, 101);
        int eliteQuantity = randomValue > 90 ? 2 : 1;
        int normalQuantity = randomValue > 50 ? (randomValue > 75 ? 0 : 1) : 2;
        
        Gold gold = CalculateGold(monsterPower, normalQuantity, eliteQuantity, 0, randomValue);
        List<Monster> monsters = GenerateMonsters(floor, monsterPower, normalQuantity, eliteQuantity);

        return new FightEvent(monsters, [gold]);
    }

    private FightEvent GenerateBossFight(int floor, int monsterPower)
    {
        int randomValue = _random.Next(1, 101);
        int bossQuantity = randomValue > 99 ? 2 : 1;
        int eliteQuantity = randomValue > 15 ? (randomValue > 99 ? 0 : 1) : 2;
        int normalQuantity = randomValue > 15 ? (randomValue > 99 ? 0 : 3) : 2;
        
        Gold gold = CalculateGold(monsterPower, normalQuantity, eliteQuantity, bossQuantity, randomValue);
        List<Monster> monsters = GenerateMonsters(floor, monsterPower, normalQuantity, eliteQuantity, bossQuantity);

        return new FightEvent(monsters, [gold]);
    }

    private List<Monster> GenerateMonsters(int floor, int monsterPower, int normalQuantity, int eliteQuantity = 0, int bossQuantity = 0)
    {
        List<Monster> monsters = [];
        List<Monster> availableMonsters = _assetManager.BossMonsters.FindAll(monster => monster.Floor == floor);

        while(bossQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[_random.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower * 5));
        }

        availableMonsters = _assetManager.EliteMonsters.FindAll(monster => monster.Floor == floor);
        while(eliteQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[_random.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower * 2));
        }

        availableMonsters = _assetManager.NormalMonsters.FindAll(monster => monster.Floor == floor);
        while(normalQuantity-- > 0)
        {
            Monster pickedMonster = availableMonsters[_random.Next(0, availableMonsters.Count)];
            monsters.Add(new(pickedMonster, monsterPower));
        }

        return monsters;
    }

    private ShopEvent GenerateShop(int monsterPower)
    {
        List<Item> sellingItems = [];

        for (int i = 0; i < 5; i++)
            sellingItems.Add(GenerateEquip(monsterPower));

        return new(sellingItems);
    }

    private Equipment GenerateEquip(int monsterPower)
    {
        int totalWeight = 0;
        foreach(var (weight, powerMultiplier) in ItemWeights.Values)
        {
            if (weight + monsterPower * powerMultiplier / 100 > 0)
                totalWeight += weight + monsterPower * powerMultiplier / 100;
        }

        int randomValue = _random.Next(totalWeight);
        int cumulativeWeight = 0;
        ItemRarity rarity = ItemRarity.Common;

        foreach(var itemWeight in ItemWeights)
        {
            int itemCurWeight = itemWeight.Value.weight + monsterPower * itemWeight.Value.powerMultiplier / 100;
            if (itemCurWeight <= 0)
                continue;

            cumulativeWeight += itemCurWeight;
            if (randomValue < cumulativeWeight)
            {
                rarity = itemWeight.Key;
                break;
            }
        }

        return rarity switch
        {
            ItemRarity.Rare => _assetManager.RareEquipments.ElementAt(_random.Next(_assetManager.RareEquipments.Count)),
            ItemRarity.Epic => _assetManager.EpicEquipments.ElementAt(_random.Next(_assetManager.EpicEquipments.Count)),
            ItemRarity.Legendary => _assetManager.LegendaryEquipments.ElementAt(_random.Next(_assetManager.LegendaryEquipments.Count)),
            _ => _assetManager.CommonEquipments.ElementAt(_random.Next(_assetManager.CommonEquipments.Count)),
        };
    }

    private static Gold CalculateGold(int monsterPower, int normalQuantity, int eliteQuantity, int bossQuantity, int randomValue)
        => new((int) (monsterPower * (1.25 + 0.25 * normalQuantity + 0.6 * eliteQuantity + 2 * bossQuantity) * (0.85 + 0.3 * randomValue / 100)));
}