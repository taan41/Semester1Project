using System.Text.Json;

[Serializable]
class Score(int userID, string nickname, TimeSpan clearTime)
{
    public int UserID { get; set; } = userID;
    public string Nickname { get; set; } = nickname;
    public TimeSpan ClearTime { get; set; } = clearTime;

    public override string ToString()
        => $"{Nickname, -Utilities.DataConstants.nicknameMax} - {ClearTime:hh:mm:ss.fff}";

    public string ToJson()
        => JsonSerializer.Serialize(this);

    public static Score? FromJson(string data)
        => JsonSerializer.Deserialize<Score>(data);
}