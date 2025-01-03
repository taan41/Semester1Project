class EventManager
{
    private static readonly List<(ItemRarity rarity, int weight)> rarityWeights =
    [
        (ItemRarity.Common, 70),
        (ItemRarity.Rare, 30),
        (ItemRarity.Epic, 15),
        (ItemRarity.Legendary, 5)
    ];
    private const int BasePower = 15;

    private readonly Random _rng;
    private readonly GameData _gameData;
    private readonly List<List<Event>> _allEvents;

    private readonly int _normalScale = 1, _eliteScale = 2, _bossScale = 5;

    public EventManager(GameData gameData)
    {
        _gameData = gameData;
        _rng = gameData.Seed != null ? new((int) gameData.Seed) : new();
        _allEvents = GenerateAllEvents();

        _normalScale = 1;
        _eliteScale = 2;
        _bossScale = 5;
    }

    // Get events of current room
    public List<Event> GetEvents()
        => _allEvents.ElementAt(_gameData.Progress.Room + (_gameData.Progress.Floor - 1) * GameProgress.MaxRoom);

    private List<List<Event>> GenerateAllEvents()
    {
        List<List<Event>> allEvents = [];

        for (int roomIndex = 0; roomIndex < GameProgress.MaxRoom * GameProgress.MaxFloor; roomIndex++)
        {
            int roomNumber = roomIndex % GameProgress.MaxRoom + 1;
            int floorNumber = roomIndex / GameProgress.MaxRoom + 1;
            int monsterPower = BasePower + (roomNumber - 1) * 1 + (floorNumber - 1) * GameProgress.MaxRoom;

            if (roomNumber % GameProgress.MaxRoom == 0)
            {
                allEvents.Add([GenerateBossFight(roomIndex, floorNumber, monsterPower)]);
                continue;
            }

            if (roomNumber % 10 == 0)
            {
                allEvents.Add([new(EventType.Camp), GenerateShop(roomIndex)]);
                continue;
            }
            
            int eventQuantity = _rng.Next(1, 101) > 50 ? 3 : 2;

            List<Event> possibleEvents = [];
            while(eventQuantity-- > 0)
            {
                if (roomNumber % 10 == 5)
                    possibleEvents.Add(GenerateTreasure(floorNumber));
                else
                    possibleEvents.Add(GenerateNormalEvent(roomIndex, floorNumber, monsterPower));
            }

            allEvents.Add(possibleEvents);
        }

        return allEvents;
    }

    private Event GenerateNormalEvent(int roomIndex, int floorNumber, int monsterPower)
    {
        int randomValue = _rng.Next(1, 101);

        if (randomValue > 80 && roomIndex > 4)
            return GenerateEliteFight(roomIndex, floorNumber, monsterPower);

        return GenerateNormalFight(roomIndex, floorNumber, monsterPower);
    }

    private FightEvent GenerateNormalFight(int roomIndex, int floorNumber, int monsterPower)
    {
        int quantityRNG = _rng.Next(1, 101);
        int normalQuantity = quantityRNG > 35 ? (quantityRNG > 90 ? 4 : 3) : 2;
        
        List<Monster> monsters = GenerateMonsters(floorNumber, monsterPower, normalQuantity);

        return new FightEvent(monsters, GenerateFightRewards(roomIndex, floorNumber, monsterPower, normalQuantity));
    }

    private FightEvent GenerateEliteFight(int roomIndex, int floorNumber, int monsterPower)
    {
        int quantityRNG = _rng.Next(1, 101);
        int eliteQuantity = quantityRNG > 90 ? 2 : 1;
        int normalQuantity = quantityRNG > 50 ? (quantityRNG > 75 ? 0 : 1) : 2;
        
        List<Monster> monsters = GenerateMonsters(floorNumber, monsterPower, normalQuantity, eliteQuantity);

        return new FightEvent(monsters, GenerateFightRewards(roomIndex, floorNumber, monsterPower, normalQuantity, eliteQuantity));
    }

    private FightEvent GenerateBossFight(int roomIndex, int floorNumber, int monsterPower)
    {
        int quantityRNG = _rng.Next(1, 101);
        int bossQuantity = quantityRNG > 99 ? 2 : 1;
        int eliteQuantity = quantityRNG > 15 ? (quantityRNG > 99 ? 0 : 1) : 2;
        int normalQuantity = quantityRNG > 15 ? (quantityRNG > 99 ? 0 : 3) : 2;
        
        List<Monster> monsters = GenerateMonsters(floorNumber, monsterPower, normalQuantity, eliteQuantity, bossQuantity);

        return new FightEvent(monsters, GenerateFightRewards(roomIndex, floorNumber, monsterPower, normalQuantity, eliteQuantity, bossQuantity));
    }

    private List<Monster> GenerateMonsters(int floorNumber, int monsterPower, int normalQuantity, int eliteQuantity = 0, int bossQuantity = 0)
    {
        List<Monster> monsters = [];

        while(bossQuantity-- > 0)
        {
            Monster pickedMonster = AssetManager.Monsters[_rng.Next((floorNumber - 1) * 1000 + 201, Monster.IDTracker[floorNumber - 1][2])];
            monsters.Add(new(pickedMonster, monsterPower * _bossScale));
        }

        while(eliteQuantity-- > 0)
        {
            Monster pickedMonster = AssetManager.Monsters[_rng.Next((floorNumber - 1) * 1000 + 101, Monster.IDTracker[floorNumber - 1][1])];
            monsters.Add(new(pickedMonster, monsterPower * _eliteScale));
        }

        while(normalQuantity-- > 0)
        {
            Monster pickedMonster = AssetManager.Monsters[_rng.Next((floorNumber - 1) * 1000 + 1, Monster.IDTracker[floorNumber - 1][0])];
            monsters.Add(new(pickedMonster, monsterPower * _normalScale));
        }

        return monsters;
    }

    private ShopEvent GenerateShop(int roomIndex)
    {
        List<Equipment> equipments = [];
        for (int i = 0; i < 6; i++)
        {
            ItemRarity rarity = GenerateRarity(roomIndex);
            equipments.Add(new(AssetManager.Equipments[_rng.Next((int) rarity * 100 + 1, Equipment.IDTracker[(int) rarity])]));
        }
        equipments.Sort(new EquipmentComparer());

        List<Skill> skills = [];
        for(int i = 0; i < 4; i++)
        {
            ItemRarity rarity = GenerateRarity(roomIndex);
            skills.Add(new(AssetManager.Skills[_rng.Next((int) rarity * 100 + 1, Equipment.IDTracker[(int) rarity])]));
        }
        skills.Sort(new SkillComparer());

        List<Item> items = [];
        items.AddRange(equipments);
        items.AddRange(skills);

        return new(items);
    }

    // Weight-based RNG for item's rarity
    private ItemRarity GenerateRarity(int roomIndex)
    {
        int[] cumulativeWeight = new int[4];
        int maxRoom = GameProgress.MaxRoom;

        cumulativeWeight[0] = rarityWeights[0].weight / (roomIndex > maxRoom ? 2 * roomIndex / maxRoom : 1);
        cumulativeWeight[1] = cumulativeWeight[0] + rarityWeights[1].weight;
        cumulativeWeight[2] = cumulativeWeight[1] + Math.Max(0, rarityWeights[2].weight * (roomIndex - maxRoom) / maxRoom);
        cumulativeWeight[3] = cumulativeWeight[2] + Math.Max(0, rarityWeights[3].weight * 3 * (roomIndex - maxRoom * 15 / 10) / maxRoom);

        int rarityRNG = _rng.Next(cumulativeWeight[3]);
        for (int i = 0; i < 4; i++)
        {
            if (rarityRNG < cumulativeWeight[i])
                return rarityWeights[i].rarity;
        }

        return ItemRarity.Legendary;
    }

    private TreasureEvent GenerateTreasure(int floorNumber)
    {
        List<Item> rewards = [];
        rewards.Add(new Gold((200 + floorNumber * 100) * _rng.Next(85, 140) / 100));

        ItemRarity equipRarity = GenerateRarity(floorNumber  * GameProgress.MaxRoom);
        rewards.Add(new Equipment(AssetManager.Equipments[_rng.Next((int) equipRarity * 100 + 1, Equipment.IDTracker[(int) equipRarity])]));

        ItemRarity skillRarity = GenerateRarity(floorNumber  * GameProgress.MaxRoom);
        rewards.Add(new Skill(AssetManager.Skills[_rng.Next((int) skillRarity * 100 + 1, Skill.IDTracker[(int) skillRarity])]));

        return new(rewards);
    }

    private List<Item> GenerateFightRewards(int roomIndex, int floorNumber, int monsterPower, int normalQuantity, int eliteQuantity = 0, int bossQuantity = 0)
    {
        List<Item> rewards = [];
        rewards.Add(CalculateGold(monsterPower, normalQuantity, eliteQuantity, bossQuantity, _rng.Next(100)));

        // Guarantee drops after boss fight, and only epic & above rarity
        if (bossQuantity > 0)
        {
            ItemRarity equipRarity = _rng.Next(100 + (floorNumber - 1) * 200) < 75 ?
                ItemRarity.Epic : ItemRarity.Legendary;
            rewards.Add(new Equipment(AssetManager.Equipments[_rng.Next((int) equipRarity * 100 + 1, Equipment.IDTracker[(int) equipRarity])]));

            ItemRarity skillRarity = _rng.Next(100 + (floorNumber - 1) * 200) < 75 ?
                ItemRarity.Epic : ItemRarity.Legendary;
            rewards.Add(new Skill(AssetManager.Skills[_rng.Next((int) skillRarity * 100 + 1, Skill.IDTracker[(int) skillRarity])]));
        }
        else
        {
            int itemChance = _rng.Next(1, 101);
            if (itemChance < 91 && itemChance < 40 + roomIndex + eliteQuantity * 20)
            {
                ItemRarity equipRarity = GenerateRarity(roomIndex);
                rewards.Add(new Equipment(AssetManager.Equipments[_rng.Next((int) equipRarity * 100 + 1, Equipment.IDTracker[(int) equipRarity])]));
            }

            int skillChance = _rng.Next(1, 101);
            if (skillChance < 71 && skillChance < 15 + roomIndex + eliteQuantity * 10)
            {
                ItemRarity skillRarity = GenerateRarity(roomIndex);
                rewards.Add(new Skill(AssetManager.Skills[_rng.Next((int) skillRarity * 100 + 1, Skill.IDTracker[(int) skillRarity])]));
            }
        }

        return rewards;
    }

    // 125% of monsterPower as base value, +25% per normal monster, +60% per elite, +200% per boss, influctate between 85% - 115%
    private static Gold CalculateGold(int monsterPower, int normalQuantity, int eliteQuantity, int bossQuantity, int randomValue)
        => new(monsterPower * (125 + 25 * normalQuantity + 60 * eliteQuantity + 200 * bossQuantity) * (85 + 3 * randomValue % 10) / 10000);
}