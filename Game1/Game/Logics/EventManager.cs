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

    private readonly Random _rng;
    private readonly GameData _gameData;
    private readonly AssetManager _assets;
    private readonly List<List<Event>> _events;

    private readonly int _normalScale, _eliteScale, _bossScale;

    public EventManager(GameData gameData, AssetManager assetManager)
    {
        _gameData = gameData;
        _assets = assetManager;
        _rng = new(_gameData.Seed);
        _events = GenerateEvents(_rng);

        _normalScale = 1;
        _eliteScale = 2;
        _bossScale = 5;
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
        int randomValue = _rng.Next(1, 101);

        if (randomValue > 80 && room > 5)
            return GenerateEliteFight(floor, monsterPower);

        return GenerateNormalFight(floor, monsterPower);
    }

    private FightEvent GenerateNormalFight(int floor, int monsterPower)
    {
        int randomValue = _rng.Next(1, 101);
        int normalQuantity = randomValue > 35 ? (randomValue > 90 ? 4 : 3) : 2;
        
        Gold gold = CalculateGold(monsterPower, normalQuantity, 0, 0, randomValue);
        List<Monster> monsters = GenerateMonsters(floor, monsterPower, normalQuantity);

        return new FightEvent(monsters, [gold]);
    }

    private FightEvent GenerateEliteFight(int floor, int monsterPower)
    {
        int randomValue = _rng.Next(1, 101);
        int eliteQuantity = randomValue > 90 ? 2 : 1;
        int normalQuantity = randomValue > 50 ? (randomValue > 75 ? 0 : 1) : 2;
        
        Gold gold = CalculateGold(monsterPower, normalQuantity, eliteQuantity, 0, randomValue);
        List<Monster> monsters = GenerateMonsters(floor, monsterPower, normalQuantity, eliteQuantity);

        return new FightEvent(monsters, [gold]);
    }

    private FightEvent GenerateBossFight(int floor, int monsterPower)
    {
        int randomValue = _rng.Next(1, 101);
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

        while(bossQuantity-- > 0)
        {
            Monster pickedMonster = _assets.Monsters[_rng.Next((floor - 1) * 1000 + 201, Monster.IDTracker[floor - 1][2])];
            monsters.Add(new(pickedMonster, monsterPower * _bossScale));
        }

        while(eliteQuantity-- > 0)
        {
            Monster pickedMonster = _assets.Monsters[_rng.Next((floor - 1) * 1000 + 101, Monster.IDTracker[floor - 1][1])];
            monsters.Add(new(pickedMonster, monsterPower * _eliteScale));
        }

        while(normalQuantity-- > 0)
        {
            Monster pickedMonster = _assets.Monsters[_rng.Next((floor - 1) * 1000 + 1, Monster.IDTracker[floor - 1][0])];
            monsters.Add(new(pickedMonster, monsterPower * _normalScale));
        }

        return monsters;
    }

    private ShopEvent GenerateShop(int monsterPower)
    {
        List<Equipment> equipments = [];

        for (int i = 0; i < 5; i++)
            equipments.Add(GenerateEquip(monsterPower));

        equipments.Sort(new EquipmentComparer());

        List<Item> items = [];
        items.AddRange(equipments);

        return new(items);
    }

    private Equipment GenerateEquip(int monsterPower)
    {
        int totalWeight = 0;
        foreach(var (weight, powerMultiplier) in ItemWeights.Values)
        {
            if (weight + monsterPower * powerMultiplier / 100 > 0)
                totalWeight += weight + monsterPower * powerMultiplier / 100;
        }

        int randomValue = _rng.Next(totalWeight);
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

        return new(_assets.Equipments[_rng.Next((int) rarity * 100 + 1, Equipment.IDTracker[(int) rarity])]);
    }

    private static Gold CalculateGold(int monsterPower, int normalQuantity, int eliteQuantity, int bossQuantity, int randomValue)
        => new(monsterPower * (125 + 25 * normalQuantity + 60 * eliteQuantity + 200 * bossQuantity) * (85 + 30 * randomValue) / 100 / 100);
}