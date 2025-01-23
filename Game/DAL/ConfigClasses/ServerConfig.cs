namespace DAL.ConfigClasses
{
    [Serializable]
    public class ServerConfig
    {
        public string ServerIP { get; set; }
            // = "127.0.0.1";
            = "26.244.97.115";
        public int Port { get; set; } = 6969;

        public ServerConfig() {}

        public ServerConfig(bool rewrite)
        {
            if (rewrite)
                FileManager.WriteJson(FileManager.FolderNames.Configs, FileManager.FileNames.ServerConfig, this);
        }
    }
}