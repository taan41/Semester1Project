using System.Net.Sockets;
using System.Text.Json;
using static Utilities;

class NetworkHandler
{
    const string serverIP = "127.0.0.1";
    const int port = 5000;

    public bool IsConnected => client != null && client.Connected && stream != null;

    private readonly TcpClient? client;
    private readonly NetworkStream? stream;
    private byte[] buffer = new byte[1024];

    public NetworkHandler(out string? error)
    {
        error = null;

        try
        {
            client = new TcpClient(serverIP, port);
            stream = client.GetStream();
            stream.ReadTimeout = 15000;
            stream.WriteTimeout = 15000;
        }
        catch (Exception ex) when (ex is SocketException or IOException)
        {
            error = $"Can't connect to server";
            Close();
        }
    }

    public void Close()
    {
        if (IsConnected)
            HandleCommand(new(CommandType.Disconnect), out _, out _);
        stream?.Close();
        client?.Close();
    }

    public bool CheckUsername(string username, out string errorMsg)
    {
        Command cmdToSend = new(CommandType.CheckUsername, username);
        return HandleCommand(cmdToSend, out _, out errorMsg);
    }

    public bool Register(User user, out string errorMsg)
    {
        Command cmdToSend = new(CommandType.Register, user.ToJson());
        return HandleCommand(cmdToSend, out _, out errorMsg);
    }

    public PasswordSet? GetPassword(string username, out string errorMsg)
    {
        Command cmdToSend = new(CommandType.GetUserPwd, username);
        return HandleCommand(cmdToSend, out Command receivedCmd, out errorMsg)
            ? PasswordSet.FromJson(receivedCmd.Payload)
            : null;
    }

    public User? Login(out string errorMsg)
    {
        Command cmdToSend = new(CommandType.Login);
        return HandleCommand(cmdToSend, out Command receivedCmd, out errorMsg)
            ? User.FromJson(receivedCmd.Payload)
            : null;
    }

    public bool Logout(out string errorMsg)
    {
        Command cmdToSend = new(CommandType.Logout);
        return HandleCommand(cmdToSend, out _, out errorMsg);
    }

    public bool ValidateEmail(User user, out string errorMsg)
    {
        Command cmdToSend = new(CommandType.ValidateEmail, user.ToJson());
        return HandleCommand(cmdToSend, out _, out errorMsg);
    }

    public bool ResetPwd(User user, out string errorMsg)
    {
        Command cmdToSend = new(CommandType.ResetPwd, user.ToJson());
        return HandleCommand(cmdToSend, out _, out errorMsg);
    }

    public bool UpdateEquip(out Dictionary<int, Equipment>? equipments, out string errorMsg)
    {
        Command cmdToSend = new(CommandType.UpdateEquip);
        if (!HandleCommand(cmdToSend, out Command receivedCmd, out errorMsg))
        {
            equipments = null;
            return false;
        }
        else
        {
            equipments = JsonSerializer.Deserialize<Dictionary<int, Equipment>>(receivedCmd.Payload);

            if (equipments == null)
            {
                errorMsg = "Null equipments";
                return false;
            }

            return true;
        }
    }

    public bool UpdateSkill(out Dictionary<int, Skill>? skills, out string errorMsg)
    {
        Command cmdToSend = new(CommandType.UpdateSkill);
        if (!HandleCommand(cmdToSend, out Command receivedCmd, out errorMsg))
        {
            skills = null;
            return false;
        }
        else
        {
            skills = JsonSerializer.Deserialize<Dictionary<int, Skill>>(receivedCmd.Payload);

            if (skills == null)
            {
                errorMsg = "Null skills";
                return false;
            }

            return true;
        }
    }

    public bool UpdateMonster(out Dictionary<int, Monster>? monsters, out string errorMsg)
    {
        Command cmdToSend = new(CommandType.UpdateMonster);
        if (!HandleCommand(cmdToSend, out Command receivedCmd, out errorMsg))
        {
            monsters = null;
            return false;
        }
        else
        {
            monsters = JsonSerializer.Deserialize<Dictionary<int, Monster>>(receivedCmd.Payload);

            if (monsters == null)
            {
                errorMsg = "Null monsters";
                return false;
            }
            
            return true;
        }
    }

    public GameSave? DownloadSave(out string errorMsg)
    {
        Command cmdToSend = new(CommandType.DownloadSave);
        return HandleCommand(cmdToSend, out Command receivedCmd, out errorMsg)
            ? JsonSerializer.Deserialize<GameSave>(receivedCmd.Payload)
            : null;
    }

    public bool UploadSave(GameSave gameSave, out string errorMsg)
    {
        gameSave.Name = "CloudSave";
        Command cmdToSend = new(CommandType.UploadSave, JsonSerializer.Serialize(gameSave));
        return HandleCommand(cmdToSend, out _, out errorMsg);
    }

    public bool UploadScore(Score score, out string errorMsg)
    {
        Command cmdToSend = new(CommandType.UploadScore, score.ToJson());
        return HandleCommand(cmdToSend, out _, out errorMsg);
    }

    public bool GetScores(out List<Score>? personal, out List<Score>? monthly, out List<Score>? alltime, out string errorMsg)
    {
        personal = monthly = alltime = null;
        
        Command cmdToSend = new(CommandType.GetUserScores);
        if(!HandleCommand(cmdToSend, out Command receivedCmd, out errorMsg))
            return false;
        else
            personal = JsonSerializer.Deserialize<List<Score>>(receivedCmd.Payload);

        cmdToSend.Set(CommandType.GetMonthlyScores);
        if(!HandleCommand(cmdToSend, out receivedCmd, out errorMsg))
            return false;
        else
            monthly = JsonSerializer.Deserialize<List<Score>>(receivedCmd.Payload);

        cmdToSend.Set(CommandType.GetAllTimeScores);
        if(!HandleCommand(cmdToSend, out receivedCmd, out errorMsg))
            return false;
        else
            alltime = JsonSerializer.Deserialize<List<Score>>(receivedCmd.Payload);

        if (personal == null || monthly == null || alltime == null)
        {
            errorMsg = "Null scores";
            return false;
        }

        return true;
    }

    private bool HandleCommand(Command cmdToSend, out Command receivedCmd, out string errorMsg)
    {
        receivedCmd = new();
        errorMsg = "";

        if (!IsConnected)
        {
            errorMsg = "Can't connect to server";
            return false;
        }

        int bytesRead, totalRead = 0;
        try
        {
            stream!.Write(Encode.GetBytes(cmdToSend.ToJson()));

            while((bytesRead = stream.Read(buffer, totalRead, 1024)) > 0)
            {
                totalRead += bytesRead;
                
                if(bytesRead < 1024)
                    break;

                if(totalRead + 1024 >= buffer.Length)
                    Array.Resize(ref buffer, buffer.Length * 2);
            }
        }
        catch (Exception ex) when (ex is IOException or SocketException)
        {
            errorMsg = $"Can't connect to server";
            return false;
        }
        
        Command? tempCmd = Command.FromJson(Encode.GetString(buffer, 0, totalRead));
        Array.Clear(buffer);

        if(tempCmd == null)
        {
            errorMsg = "Null command";
            return false;
        }

        switch(tempCmd.CommandType)
        {
            case var value when value == cmdToSend.CommandType:
                receivedCmd = tempCmd;
                return true;

            case CommandType.Error:
                errorMsg = tempCmd.Payload;
                return false;

            default:
                errorMsg = $"Received invalid command: {tempCmd.CommandType}";
                return false;
        }
    }
}