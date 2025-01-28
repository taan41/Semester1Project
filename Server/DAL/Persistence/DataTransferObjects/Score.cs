namespace DAL.Persistence.DataTransferObjects
{
    [Serializable]
    public class Score
    {
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
    }
}