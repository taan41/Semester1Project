using System.Text.Json;
using DAL;

namespace NetworkLL.DataTransferObjects
{
    [Serializable]
    public class Score
    {
        private static int NicknameMax => ConfigManager.Instance.DatabaseConfig.NicknameMax;

        public int UserID { get; set; } = -1;
        public string Nickname { get; set; } = "Temp Name";
        public TimeSpan ClearTime { get; set; } = TimeSpan.Zero;
        public DateTime UploadedTime { get; set; } = DateTime.Now;

        public Score() {}

        public Score(int? userID, string nickname, TimeSpan clearTime)
        {
            UserID = userID ?? -1;
            Nickname = nickname;
            ClearTime = clearTime;
        }

        public override string ToString()
            => $"{Nickname.PadRight(NicknameMax)} - {ClearTime:hh\\:mm\\:ss\\.fff} - {UploadedTime:yyyy-MM-dd HH\\:mm\\:ss}";

        public string ToJson()
            => JsonSerializer.Serialize(this);

        public static Score? FromJson(string data)
            => JsonSerializer.Deserialize<Score>(data);
    }
}