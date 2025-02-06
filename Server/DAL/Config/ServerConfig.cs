using DAL.DBHandlers;

namespace DAL.Config
{
    [Serializable]
    public class ServerConfig
    {
        public string ServerIP { get; set; }
            // = "127.0.0.1";
            = "26.244.97.115";
        public int Port { get; set; } = 6969;

        public ServerConfig() {}

        public async Task<bool> Save()
            => (await ConfigDB.Add(DBManager.ConfigNames.ServerConfig, Utitlities.ToJson(this))).success;
    }
}