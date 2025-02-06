namespace DAL.DataModels
{
    [Serializable]
    public class Score
    {
        public int RunID { get; set; } = -1;
        public int UserID { get; set; } = -1;
        public string Nickname { get; set; } = "Temp Name";
        public TimeSpan ClearTime { get; set; } = TimeSpan.Zero;
        public DateTime UploadedTime { get; set; } = DateTime.Now;

        public Score() {}
    }
}