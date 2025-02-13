using DAL;

namespace BLL.Config
{
    [Serializable]
    public class ServerConfig
    {
        public const string
            LocalhostIP = "127.0.0.1",
            RadminIP = "26.244.97.115";
        public const int DefaultPort = 60470;

        public List<string> ServerIPs { get; set; } = [LocalhostIP, RadminIP];
        public List<int> ServerPorts { get; set; } = [DefaultPort];

        public ServerConfig() {}

        public ServerConfig(bool rewrite)
        {
            if (rewrite)
                FileManager.WriteJson(FileManager.FolderNames.Configs, FileManager.FileNames.ServerConfig, this);
        }
    }
}