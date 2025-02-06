using DAL.DBHandlers;

namespace DAL.Config
{
    [Serializable]
    public class ConfigManager
    {
        public static ConfigManager Instance { get; } = new();

        public GameConfig GameConfig { get; private set; } = new();
        public ServerConfig ServerConfig { get; private set; } = new();
        public DatabaseConfig DatabaseConfig { get; private set; } = new();
        public AssetConfig AssetConfig { get; private set; } = new();

        private ConfigManager() {}

        public async Task LoadConfig(bool rewrite = false)
        {
            GameConfig = (await ConfigDB.Get<GameConfig>(DBManager.ConfigNames.GameConfig)).config ?? new();
            ServerConfig = (await ConfigDB.Get<ServerConfig>(DBManager.ConfigNames.ServerConfig)).config ?? new();
            DatabaseConfig = (await ConfigDB.Get<DatabaseConfig>(DBManager.ConfigNames.DatabaseConfig)).config ?? new();
            AssetConfig = (await ConfigDB.Get<AssetConfig>(DBManager.ConfigNames.AssetConfig)).config ?? new();

            if (rewrite)
                await SaveConfig();
        }

        public async Task SaveConfig()
        {
            await GameConfig.Save();
            await ServerConfig.Save();
            await DatabaseConfig.Save();
            await AssetConfig.Save();
        }
    }
}