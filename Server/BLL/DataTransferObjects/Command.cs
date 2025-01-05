using System.Text.Json;

enum CommandType
{
    Empty, Ping, Error, GetAES,
    UpdateAsset,
    CheckUsername, Register,
    GetUserPwd, Login, Logout,
    ChangeNickname, ChangeEmail, ChangePassword,
    UploadScore, GetUserScores, GetMonthlyScores, GetAllTimeScores,
    UploadSave, DownloadSave,
    Disconnect
}

[Serializable]
class Command(CommandType cmdType = CommandType.Empty, string? payload = null)
{
    public CommandType CommandType { get; set; } = cmdType;
    public string Payload { get; set; } = payload ?? "";

    public void Set(CommandType cmdType, string? payload = null)
    {
        CommandType = cmdType;
        Payload = payload ?? "";
    }

    public void SetError(string? payload = null)
        => Set(CommandType.Error, payload);

    public string Name()
        => CommandType.ToString();

    public string Serialize()
        => JsonSerializer.Serialize(this);

    public static Command? Deserialize(string data) =>
        JsonSerializer.Deserialize<Command>(data);
}