using DAL.DataModels;
using DAL.DBHandlers;

namespace DAL.Config
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

        public async Task<bool> Save()
            => (await ConfigDB.Add(DBManager.ConfigNames.ServerConfig, Utitlities.ToJson(this))).success;
    }
}