using DAL.DBHandlers;

namespace DAL.Persistence.ConfigClasses
{
    [Serializable]
    public class AssetConfig
    {
        public int[] EquipPtPerRarity { get; set; } = [3, 6, 10, 15];
        public int[] SkillPtPerMPPerRarity { get; set; } = [1, 2, 3, 5];

        public AssetConfig() {}

        public async Task<bool> Save()
            => (await ConfigDB.Add(DBManager.Configs.AssetConfig, Utitlities.ToJson(this))).success;
    }
}
