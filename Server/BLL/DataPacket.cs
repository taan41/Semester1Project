namespace BLL;

[Serializable]
public class DataPacket
{
    public enum PacketType
    {
        Generic, Database, User, Game
    }

    public enum GenericRequest
    {
        Empty, Ping, Error, Disconnect,
    }

    public enum DatabaseRequest
    {
        ConfigGame, ConfigServer, ConfigDatabase,
        UpdateEquip, UpdateSkill, UpdateMonster,
    }

    public enum UserRequest
    {
        UsernameAvailable, UserRegister,
        UserGet, UserUpdate, UserDelete,
        Login, Logout,
    }

    public enum GameRequest
    {
        UploadSave, DownloadSave,
        UploadScore, PersonalScores, MonthlyScores, AllTimeScores
    }

    public int Type { get; set; } = (int) PacketType.Generic;
    public int Request { get; set; } = (int) GenericRequest.Empty;
    public string Payload { get; set; } = "";

    public DataPacket() {}

    public DataPacket(int type, int request, string payload)
    {
        Type = type;
        Request = request;
        Payload = payload;
    }

    public DataPacket(DataPacket other, string payload = "")
    {
        Type = other.Type;
        Request = other.Request;
        Payload = payload;
    }
}