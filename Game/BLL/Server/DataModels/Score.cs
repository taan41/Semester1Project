using BLL.Config;

namespace BLL.Server.DataModels
{
    [Serializable]
    public class Score
    {
        private static int NicknameMax => ConfigManager.Instance.DatabaseConfig.NicknameMax;

        public int RunID { get; set; } = -1;
        public int UserID { get; set; } = -1;
        public string Nickname { get; set; } = "Temp Name";
        public TimeSpan ClearTime { get; set; } = TimeSpan.Zero;
        public DateTime UploadedTime { get; set; } = DateTime.Now;

        public Score() {}

        public override string ToString()
            => $"{Nickname.PadRight(NicknameMax)} - {ClearTime:hh\\:mm\\:ss\\.fff} - {UploadedTime:yyyy-MM-dd HH\\:mm\\:ss}";
    }
}