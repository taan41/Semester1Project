using System.Text.Json;

enum CommandType
{
    Empty, Ping, Error, GetAES,
    UpdateAsset,
    CheckUsername, Register,
    GetUserPwd, Login, Logout, RequestResetPwd, ResetPwd,
    ChangeNickname, ChangeEmail, ChangePassword,
    UploadScore, GetUserScores, GetMonthlyScores, GetAllTimeScores,
    UploadSave, DownloadSave,
    Disconnect
}

[Serializable]
class Command
{
    public CommandType CommandType { get; set; } = CommandType.Empty;
    public string Payload { get; set; } = "";

    public Command() {}

    public Command(CommandType cmdType, string? payload = null)
    {
        CommandType = cmdType;
        Payload = payload ?? "";
    }

    public void Set(CommandType cmdType, string? payload = null)
    {
        CommandType = cmdType;
        Payload = payload ?? "";
    }

    public void SetError(string? payload = null)
        => Set(CommandType.Error, payload);

    public string Name()
        => CommandType.ToString();

    public string ToJson()
        => JsonSerializer.Serialize(this);

    public static Command? FromJson(string data) =>
        JsonSerializer.Deserialize<Command>(data);
}