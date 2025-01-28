using BLL.GameComponents.ItemComponents;
using BLL.GameComponents.EntityComponents;
using BLL.GameComponents.EventComponents;
using BLL.GameComponents.Others;
using DAL;
using DAL.ConfigClasses;

namespace BLL.GameHelpers
{
    public class EventGenerator
    {
        private static GameConfig Config => ConfigManager.Instance.GameConfig;

        private static readonly List<(Item.Rarity rarity, int weight)> rarityWeights =
        [
            (Item.Rarity.Common, 70),
            (Item.Rarity.Rare, 30),
            (Item.Rarity.Epic, 15),
            (Item.Rarity.Legendary, 5)
        ];

        private static int MaxFloor => Config.ProgressMaxFloor;
        private static int MaxRoom => Config.ProgressMaxRoom;

        private static int BasePower =>
            (Config.MonsterDefaultATK * Config.MonsterPowerATKPercentage +
            Config.MonsterDefaultHP * Config.MonsterPowerHPPercentage) / 100;

        private static int PowerPerRoom => Config.EventPowerPerRoom;
        private static int PowerPerFloorRatio => Config.EventPowerPerFloorRatio;

        private static int ElitePowerPercentage => Config.EventPowerElitePercentage;
        private static int BossPowerPercentage =>  Config.EventPowerBossPercentage;

        private static int TreasureRoomCount => Config.EventRoomCountTreasure;
        private static int ShopRoomCount => Config.EventRoomCountShop;
        private static int CampRoomCount => Config.EventRoomCountCamp;

        private static int BaseGold => Config.EventGoldBase;
        private static int NormalGold => Config.EventGoldPerNormal;
        private static int EliteGold => Config.EventGoldPerElite;  
        private static int BossGold => Config.EventGoldPerBoss;
        private static int GoldFloorPercentage => Config.EventGoldFloorPercentage;

        private static int TreasureGold => Config.EventGoldTreasure;
        private static int TreasureGoldPerFloorPercentage => Config.EventGoldTreasurePerFloorPercentage;

        private readonly Random _rng;
        private readonly RunData _gameData;
        private readonly List<List<Event>> _allEvents;

        public EventGenerator(RunData gameData)
        {
            _gameData = gameData;
            _rng = new(gameData.Seed);
            _allEvents = GenerateAllEvents();
        }

        // Get events of current room
        public List<Event> GetEvents()
            => _allEvents.ElementAt(Math.Max(0, _gameData.Progress.Room - 1 + (_gameData.Progress.Floor - 1) * MaxRoom));

        public void RerollShop(RunProgress progress, ShopEvent shop)
        {
            shop.SellingItems.Clear();
            shop.SellingItems.AddRange(GenerateShopItems(progress.Room + (progress.Floor - 1) * MaxRoom));
        }

        private List<List<Event>> GenerateAllEvents()
        {
            List<List<Event>> allEvents = [];

            for (int roomIndex = 0; roomIndex < MaxRoom * MaxFloor; roomIndex++)
            {
                int roomNumber = roomIndex % MaxRoom + 1;
                int floorNumber = roomIndex / MaxRoom + 1;
                int monsterPower =
                    (BasePower + (roomNumber - 1) * PowerPerRoom / 100) *
                    (100 + (floorNumber - 1) * PowerPerFloorRatio) / 100;

                if (roomNumber % MaxRoom == 0)
                {
                    allEvents.Add([GenerateBossFight(roomIndex, floorNumber, monsterPower)]);
                    continue;
                }
                
                List<Event> roomEvents = [];

                int eventQuantity = _rng.Next(1, 101) > 50 ? 3 : 2;
                bool addNormalEvent = true;

                if (roomNumber % TreasureRoomCount == 0)
                {
                    while (eventQuantity-- > 0)
                        roomEvents.Add(GenerateTreasure(floorNumber));
                    allEvents.Add(roomEvents);
                    continue;
                }

                if (roomNumber % ShopRoomCount == 0)
                {
                    roomEvents.Add(GenerateShop(roomIndex));
                    addNormalEvent = false;
                }

                if (roomNumber % CampRoomCount == 0)
                {
                    roomEvents.Add(new(Event.Type.Camp));
                    addNormalEvent = false;
                }

                if (addNormalEvent)
                {
                    int maxRandomQuantity = eventQuantity - 1;
                    while (eventQuantity-- > 0)
                    {
                        int eventTypeRNG = _rng.Next(1, 101);

                        if (eventTypeRNG > 70 && roomIndex > 1 && maxRandomQuantity > 0)
                        {
                            roomEvents.Add(GenerateRandomEvent(roomIndex, floorNumber, monsterPower));
                            maxRandomQuantity--;
                        }
                        else if (eventTypeRNG > 40 && roomIndex > 2)
                            roomEvents.Add(GenerateEliteFight(roomIndex, floorNumber, monsterPower));
                        else
                            roomEvents.Add(GenerateNormalFight(roomIndex, floorNumber, monsterPower));
                    }
                }

                allEvents.Add(roomEvents);
            }

            return allEvents;
        }

        private RandomEvent GenerateRandomEvent(int roomIndex, int floorNumber, int monsterPower)
        {
            int randomValue = _rng.Next(1, 101);

            RandomEvent randomEvent = new()
            {
                ChildEvent = randomValue switch
                {
                    > 90 => new Event(Event.Type.Camp),
                    > 80 => GenerateShop(roomIndex),
                    > 50 => GenerateTreasure(floorNumber),
                    > 35 => GenerateEliteFight(roomIndex, floorNumber, monsterPower),
                    _ => GenerateNormalFight(roomIndex, floorNumber, monsterPower)
                }
            };
            return randomEvent;
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

            while (bossQuantity-- > 0)
            {
                Monster pickedMonster = AssetLoader.GetMonster(_rng.Next((floorNumber - 1) * 1000 + 201, IDTracker.MonsterIDs[floorNumber - 1][2]));
                monsters.Add(new(pickedMonster, monsterPower * BossPowerPercentage / 100));
            }

            while (eliteQuantity-- > 0)
            {
                Monster pickedMonster = AssetLoader.GetMonster(_rng.Next((floorNumber - 1) * 1000 + 101, IDTracker.MonsterIDs[floorNumber - 1][1]));
                monsters.Add(new(pickedMonster, monsterPower * ElitePowerPercentage / 100));
            }

            while (normalQuantity-- > 0)
            {
                Monster pickedMonster = AssetLoader.GetMonster(_rng.Next((floorNumber - 1) * 1000 + 1, IDTracker.MonsterIDs[floorNumber - 1][0]));
                monsters.Add(new(pickedMonster, monsterPower));
            }

            return monsters;
        }

        private List<Item> GenerateShopItems(int roomIndex)
        {
            List<Equipment> equipments = [];
            for (int i = 0; i < 6; i++)
            {
                Item.Rarity rarity = GenerateRarity(roomIndex);
                equipments.Add(new(AssetLoader.GetEquip(_rng.Next((int) rarity * 100 + 1, IDTracker.EquipIDs[(int) rarity]))));
            }
            equipments.Sort(new EquipmentComparer());

            List<Skill> skills = [];
            for(int i = 0; i < 4; i++)
            {
                Item.Rarity rarity = GenerateRarity(roomIndex);
                skills.Add(new(AssetLoader.GetSkill(_rng.Next((int) rarity * 100 + 1, IDTracker.SkillIDs[(int) rarity]))));
            }
            skills.Sort(new SkillComparer());

            List<Item> items = [];
            items.AddRange(equipments);
            items.AddRange(skills);

            return items;
        }

        private ShopEvent GenerateShop(int roomIndex)
            => new(GenerateShopItems(roomIndex));

        // Weight-based RNG for item's rarity
        private Item.Rarity GenerateRarity(int roomIndex)
        {
            int[] cumulativeWeight = new int[4];
            int maxRoom = MaxRoom;

            cumulativeWeight[0] = roomIndex > maxRoom ?
                roomIndex > 2 * maxRoom ? 0 : rarityWeights[0].weight / 5 : rarityWeights[0].weight;
            cumulativeWeight[1] = cumulativeWeight[0] + rarityWeights[1].weight;
            cumulativeWeight[2] = cumulativeWeight[1] + Math.Max(0, rarityWeights[2].weight * (roomIndex - maxRoom) / maxRoom);
            cumulativeWeight[3] = cumulativeWeight[2] + Math.Max(0, rarityWeights[3].weight * 3 * (roomIndex - maxRoom * 15 / 10) / maxRoom);

            int rarityRNG = _rng.Next(cumulativeWeight[3]);
            for (int i = 0; i < 4; i++)
            {
                if (rarityRNG < cumulativeWeight[i])
                    return rarityWeights[i].rarity;
            }

            return Item.Rarity.Legendary;
        }

        private TreasureEvent GenerateTreasure(int floorNumber)
        {
            List<Item> rewards = [];
            rewards.Add(new Gold(
                TreasureGold *
                (100 + (floorNumber - 1) * TreasureGoldPerFloorPercentage) *
                _rng.Next(85, 140) / 10000
            ));

            Item.Rarity equipRarity = GenerateRarity(floorNumber  * MaxRoom);
            rewards.Add(new Equipment(AssetLoader.GetEquip(_rng.Next((int) equipRarity * 100 + 1, IDTracker.EquipIDs[(int) equipRarity]))));

            Item.Rarity skillRarity = GenerateRarity(floorNumber  * MaxRoom);
            rewards.Add(new Skill(AssetLoader.GetSkill(_rng.Next((int) skillRarity * 100 + 1, IDTracker.SkillIDs[(int) skillRarity]))));

            return new(rewards);
        }

        private List<Item> GenerateFightRewards(int roomIndex, int floorNumber, int monsterPower, int normalQuantity, int eliteQuantity = 0, int bossQuantity = 0)
        {
            List<Item> rewards = [];
            rewards.Add(CalculateGold(floorNumber, normalQuantity, eliteQuantity, bossQuantity));

            // Guarantee drops after boss fight, and only epic & above rarity
            if (bossQuantity > 0)
            {
                Item.Rarity equipRarity = _rng.Next(100 + (floorNumber - 1) * 200) < 75 ?
                    Item.Rarity.Epic : Item.Rarity.Legendary;
                rewards.Add(new Equipment(AssetLoader.GetEquip(_rng.Next((int) equipRarity * 100 + 1, IDTracker.EquipIDs[(int) equipRarity]))));

                Item.Rarity skillRarity = _rng.Next(100 + (floorNumber - 1) * 200) < 75 ?
                    Item.Rarity.Epic : Item.Rarity.Legendary;
                rewards.Add(new Skill(AssetLoader.GetSkill(_rng.Next((int) skillRarity * 100 + 1, IDTracker.SkillIDs[(int) skillRarity]))));
            }
            else
            {
                int itemChance = _rng.Next(1, 101);
                if (itemChance < 91 && itemChance < 40 + roomIndex + eliteQuantity * 20)
                {
                    Item.Rarity equipRarity = GenerateRarity(roomIndex);
                    rewards.Add(new Equipment(AssetLoader.GetEquip(_rng.Next((int) equipRarity * 100 + 1, IDTracker.EquipIDs[(int) equipRarity]))));
                }

                int skillChance = _rng.Next(1, 101);
                if (skillChance < Math.Min(71, 15 + roomIndex + eliteQuantity * 10))
                {
                    Item.Rarity skillRarity = GenerateRarity(roomIndex);
                    rewards.Add(new Skill(AssetLoader.GetSkill(_rng.Next((int) skillRarity * 100 + 1, IDTracker.SkillIDs[(int) skillRarity]))));
                }
            }

            return rewards;
        }

        private Gold CalculateGold(int floorNumber, int normalQuantity, int eliteQuantity, int bossQuantity)
            => new(
                (BaseGold + NormalGold * normalQuantity + EliteGold * eliteQuantity + BossGold * bossQuantity) * 
                (100 + (floorNumber - 1) * GoldFloorPercentage) * _rng.Next(85, 115) / 10000
            );
    }
}