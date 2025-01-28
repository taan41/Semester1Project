namespace DAL.ConfigClasses
{
    [Serializable]
    public class GameConfig
    {
        // Entity config
        public int EntityMPRegenPercentage { get; set; } = 15;

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
        public int MonsterPowerATKPercentage { get; set; } = 400;
        public int MonsterPowerHPPercentage { get; set; } = 100;

        // Item config
        public int ItemPriceBase { get; set; } = 200;
        public int ItemPriceRarityBonusPercentage { get; set; } = 100;
        public int ItemPriceEquipBonusPercentage { get; set; } = 30;
        public int ItemPriceSkillBonusPercentage { get; set; } = 15;
        public int ItemPriceSellingPercentage { get; set; } = 15;

        // Equip config
        public int EquipPtATKPercentage { get; set; } = 200;
        public int EquipPtDEFPercentage { get; set; } = 100;
        public int EquipPtHPPercentage { get; set; } = 800;
        public int EquipPtMPPercentage { get; set; } = 400;

        // Skill config
        public int SkillPtDmgPercentage { get; set; } = 150;
        public int SkillPtHealPercentage { get; set; } = 100;
        public int SkillRarityCommonPercentage { get; set; } = 100;
        public int SkillRarityRarePercentage { get; set; } = 150;
        public int SkillRarityEpicPercentage { get; set; } = 250;
        public int SkillRarityLegendaryPercentage { get; set; } = 400;
        public int SkillTypeSinglePercentage { get; set; } = 100;
        public int SkillTypeRandomPercentage { get; set; } = 120;
        public int SkillTypeAllPercentage { get; set; } = 50;

        // Game progress config
        public int ProgressMaxFloor { get; set; } = 3;
        public int ProgressMaxRoom { get; set; } = 16;

        // Event manager config
        public int EventPowerPerRoom { get; set; } = 100;
        public int EventPowerPerFloorRatio { get; set; } = 50;
        public int EventPowerElitePercentage { get; set; } = 200;
        public int EventPowerBossPercentage { get; set; } = 400;
        public int EventRoomCountTreasure { get; set; } = 10;
        public int EventRoomCountCamp { get; set; } = 5;
        public int EventRoomCountShop { get; set; } = 5;
        public int EventGoldBase { get; set; } = 75;
        public int EventGoldPerNormal { get; set; } = 5;
        public int EventGoldPerElite { get; set; } = 25;
        public int EventGoldPerBoss { get; set; } = 100;
        public int EventGoldFloorPercentage { get; set; } = 15;
        public int EventGoldTreasure { get; set; } = 150;
        public int EventGoldTreasurePerFloorPercentage { get; set; } = 50;

        public GameConfig() {}

        public GameConfig(bool rewrite)
        {
            if (rewrite)
                FileManager.WriteJson(FileManager.FolderNames.Configs, FileManager.FileNames.GameConfig, this);
        }
    }
}