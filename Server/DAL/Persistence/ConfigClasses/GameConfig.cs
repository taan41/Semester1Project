using DAL.DBHandlers;

namespace DAL.Persistence.ConfigClasses
{
    [Serializable]
    public class GameConfig
    {
        // Entity config
        public int EntityMPPercentagePerAttack { get; set; } = 20;

        // Player config
        public int PlayerDefaultATK { get; set; } = 3;
        public int PlayerDefaultDEF { get; set; } = 0;
        public int PlayerDefaultHP { get; set; } = 25;
        public int PlayerDefaultMP { get; set; } = 10;
        public int PlayerDefaultGold { get; set; } = 100;
        public int PlayerMaxSkillCount { get; set; } = 3;

        // Monster config
        public int MonsterDefaultATK { get; set; } = 1;
        public int MonsterDefaultHP { get; set; } = 10;
        public int MonsterPowerATKPercentage { get; set; } = 500;
        public int MonsterPowerHPPercentage { get; set; } = 100;

        // Item config
        public int ItemBasePrice { get; set; } = 100;
        public int ItemPriceRarityBonusPercentage { get; set; } = 75;
        public int ItemPriceEquipBonusPercentage { get; set; } = 30;
        public int ItemPriceSkillBonusPercentage { get; set; } = 10;
        public int ItemSellPricePercentage { get; set; } = 60;

        // Equip config
        public int EquipATKPtPercentage { get; set; } = 200;
        public int EquipDEFPtPercentage { get; set; } = 100;
        public int EquipHPPtPercentage { get; set; } = 800;
        public int EquipMPPtPercentage { get; set; } = 400;

        // Skill config
        public int SkillDamagePtPercentage { get; set; } = 200;
        public int SkillHealPtPercentage { get; set; } = 100;
        public int SkillSinglePercentage { get; set; } = 100;
        public int SkillRandomPercentage { get; set; } = 120;
        public int SkillAllPercentage { get; set; } = 70;

        // Game progress config
        public int ProgressMaxFloor { get; set; } = 3;
        public int ProgressMaxRoom { get; set; } = 16;

        // Event manager config
        public int EventPowerPerRoomPercentage { get; set; } = 50;
        public int EventPowerPerFloorPercentage { get; set; } = 50;
        public int EventElitePowerPercentage { get; set; } = 200;
        public int EventBossPowerPercentage { get; set; } = 500;
        public int EventTreasureRoomCount { get; set; } = 10;
        public int EventShopRoomCount { get; set; } = 5;
        public int EventCampRoomCount { get; set; } = 5;
        public int EventBaseGold { get; set; } = 100;
        public int EventGoldPerNormal { get; set; } = 25;
        public int EventGoldPerElite { get; set; } = 50;
        public int EventGoldPerBoss { get; set; } = 100;
        public int EventGoldFloorPercentage { get; set; } = 50;
        public int EventGoldPerTreasure { get; set; } = 100;
        public int EventTreasureGoldPerFloorPercentage { get; set; } = 80;

        public GameConfig() {}

        public async Task<bool> Save()
            => (await ConfigDB.Add(DBManager.Configs.GameConfig, Utitlities.ToJson(this))).success;
    }
}