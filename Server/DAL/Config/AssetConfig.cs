using DAL.DBHandlers;

namespace DAL.Config
{
    [Serializable]
    public class AssetConfig
    {
        public int[][] EquipPtPerRarityPerType { get; set; } = 
            [
                [3, 6, 10, 15], // Weapon
                [2, 4, 7, 10], // Armor
                [1, 2, 3, 5], // Relic
            ];

        public AssetConfig() {}

        public async Task<bool> Save()
            => (await ConfigDB.Add(DBManager.ConfigNames.AssetConfig, Utitlities.ToJson(this))).success;
    }
}
