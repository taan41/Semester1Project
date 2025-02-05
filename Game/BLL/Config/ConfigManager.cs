using static DAL.FileManager;

namespace BLL.Config
{
    public class ConfigManager
    {
        public static ConfigManager Instance { get; } = new(true);

        public GameConfig GameConfig { get; set; } = new();
        public ServerConfig ServerConfig { get; set; } = new();
        public DatabaseConfig DatabaseConfig { get; set; } = new();

        private ConfigManager(bool loadConfig)
        {
            if (loadConfig)
                LoadConfig();
        }

        public void LoadConfig()
        {
            GameConfig = ReadJson<GameConfig>(FolderNames.Configs, FileNames.GameConfig) ?? new GameConfig(true);
            ServerConfig = ReadJson<ServerConfig>(FolderNames.Configs, FileNames.ServerConfig) ?? new ServerConfig(true);
            DatabaseConfig = ReadJson<DatabaseConfig>(FolderNames.Configs, FileNames.DatabaseConfig) ?? new DatabaseConfig(true);
        }
    }
}