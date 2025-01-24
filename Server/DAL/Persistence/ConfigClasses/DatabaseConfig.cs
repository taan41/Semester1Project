using DAL.DBHandlers;

namespace DAL.Persistence.ConfigClasses
{
    [Serializable]
    public class DatabaseConfig
    {
        public int UsernameMin { get; set; } = 4;
        public int UsernameMax { get; set; } = 50;
        public int PasswordMin { get; set; } = 6;
        public int PasswordMax { get; set; } = 50;
        public int NicknameMin { get; set; } = 1;
        public int NicknameMax { get; set; } = 25;
        public int EmailMin { get; set; } = 1;
        public int EmailMax { get; set; } = 254;

        public DatabaseConfig() {}

        public async Task<bool> Save()
            => (await ConfigDB.Add(DBManager.ConfigNames.DatabaseConfig, Utitlities.ToJson(this))).success;
    }
}