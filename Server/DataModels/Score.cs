using System.Text.Json;

[Serializable]
class Score(int userID, string nickname, TimeSpan clearTime, DateTime? uploadedTime = null)
{
    public int UserID { get; set; } = userID;
    public string Nickname { get; set; } = nickname;
    public TimeSpan ClearTime { get; set; } = clearTime;
    public DateTime UploadedTime { get; set; } = uploadedTime ?? DateTime.Now;

    public override string ToString()
        => $"{Nickname, -MagicNum.nicknameMax} - {ClearTime:hh:mm:ss.fff}";

    public string Serialize()
        => JsonSerializer.Serialize(this);

    public static Score? Deserialize(string data)
        => JsonSerializer.Deserialize<Score>(data);
}