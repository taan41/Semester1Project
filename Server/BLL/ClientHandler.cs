using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using static Utilities;

class ClientHandler
{
    readonly TcpClient client;
    readonly NetworkStream stream;
    readonly EndPoint endPoint;

    User? user = null;

    public EndPoint EndPoint => endPoint;
    public User? User => user;

    public ClientHandler(TcpClient _client)
    {
        client = _client;
        stream = _client.GetStream();
        endPoint = _client.Client.RemoteEndPoint!;

        LogHandler.AddLog($"Connected to server", this);
    }

    public override string ToString()
        => user?.ToString() ?? endPoint?.ToString() ?? "Null client";

    public async Task HandlingClientAsync(CancellationToken token)
    {
        try
        {
            byte[] buffer = new byte[DataConstants.bufferSize];
            Memory<byte> memory = new(buffer, 0, buffer.Length);
            int bytesRead;

            Command? receivedCmd;
            Command cmdToSend = new();

            User? tempUser = null;

            while((bytesRead = await stream.ReadAsync(memory, token)) > 0)
            {
                receivedCmd = Command.FromJson(Encode.GetString(buffer, 0, bytesRead));

                switch (receivedCmd?.CommandType)
                {
                    case CommandType.Ping:
                        cmdToSend = new(CommandType.Ping, null);
                        break;

                    case CommandType.CheckUsername:
                        cmdToSend = await CheckUsername(receivedCmd);
                        break;

                    case CommandType.Register:
                        cmdToSend = await Register(receivedCmd);
                        break;
                    
                    case CommandType.GetUserPwd:
                        (cmdToSend, tempUser) = await GetUserPwd(receivedCmd);
                        break;

                    case CommandType.Login:
                        cmdToSend = Login(receivedCmd, tempUser);
                        break;

                    case CommandType.Logout:
                        cmdToSend = Logout(receivedCmd);
                        break;

                    case CommandType.ValidateEmail:
                        cmdToSend = await ValidateEmail(receivedCmd);
                        break;

                    case CommandType.ResetPwd:
                        cmdToSend = await ResetPwd(receivedCmd);
                        break;

                    case CommandType.ChangeNickname:
                        cmdToSend = await ChangeNickname(receivedCmd);
                        break;

                    case CommandType.ChangeEmail:
                        cmdToSend = await ChangeEmail(receivedCmd);
                        break;

                    case CommandType.ChangePassword:
                        cmdToSend = await ChangePassword(receivedCmd);
                        break;

                    case CommandType.UploadSave:    
                        cmdToSend = await UploadSave(receivedCmd);
                        break;

                    case CommandType.DownloadSave:
                        cmdToSend = await DownloadSave(receivedCmd);
                        break;

                    case CommandType.UploadScore:
                        cmdToSend = await UploadScore(receivedCmd);
                        break;

                    case CommandType.GetUserScores:
                        cmdToSend = await GetUserScores(receivedCmd);
                        break;

                    case CommandType.GetMonthlyScores:
                        cmdToSend = await GetMonthlyScores(receivedCmd);
                        break;

                    case CommandType.GetAllTimeScores:
                        cmdToSend = await GetAllTimeScores(receivedCmd);
                        break;

                    case CommandType.Disconnect:
                        cmdToSend = Disconnect(receivedCmd);
                        await stream.WriteAsync(Encode.GetBytes(cmdToSend.ToJson()), token);
                        return;

                    default:
                        cmdToSend = Helper.ErrorCmd(this, new(CommandType.Error, null), "Received invalid cmd");
                        break;
                }

                if (cmdToSend.CommandType != CommandType.Empty)
                {
                    await stream.WriteAsync(Encode.GetBytes(cmdToSend.ToJson()), token);
                    cmdToSend.Set(CommandType.Empty, null);
                }
            }
        }
        catch(OperationCanceledException) {}
        catch(Exception ex)
        {
            LogHandler.AddLog($"Error while handling client: {ex}", this);
        }
        finally
        {
            Server.RemoveClient(this);
            stream.Close();
            client.Close();
        }
    }

    private async Task<Command> CheckUsername(Command cmd)
    {
        var (success, errorMessage) = await UserDB.CheckUsername(cmd.Payload);

        if (!success)
            return Helper.ErrorCmd(this, cmd, errorMessage, false);

        return new(cmd.CommandType);
    }

    private async Task<Command> Register(Command cmd)
    {
        User? registeredUser = User.FromJson(cmd.Payload);

        if (registeredUser == null)
            return Helper.ErrorCmd(this, cmd, "Invalid registering user");

        var (success, errorMessage) = await UserDB.Add(registeredUser);

        if (!success)
            return Helper.ErrorCmd(this, cmd, errorMessage);

        LogHandler.AddLog($"Registered with Username '{registeredUser.Username}'", this);
        return new(cmd.CommandType);
    }

    private async Task<(Command cmdToSend, User? requestedUser)> GetUserPwd(Command cmd)
    {
        var (requestedUser, errorMessage) = await UserDB.Get(cmd.Payload);

        if (requestedUser == null)
            return (Helper.ErrorCmd(this, cmd, errorMessage, false), null);

        if (requestedUser.PwdSet == null)
            return (Helper.ErrorCmd(this, cmd, "No password found"), null);

        return (new(cmd.CommandType, requestedUser.PwdSet.ToJson()), requestedUser);
    }

    private Command Login(Command cmd, User? tempUser)
    {
        if (user != null || tempUser == null)
            return Helper.ErrorCmd(this, cmd, "Invalid user/tempUser");

        user = tempUser;
        LogHandler.AddLog($"Logged in as {user}", endPoint);
        return new(cmd.CommandType, user.ToJson());
    }

    private Command Logout(Command cmd)
    {
        if (user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        LogHandler.AddLog($"Logged out from {user}", endPoint);
        user = null;
        return new(cmd.CommandType);
    }

    private async Task<Command> ValidateEmail(Command cmd)
    {
        User? requestedUser = User.FromJson(cmd.Payload);

        if (requestedUser == null)
            return Helper.ErrorCmd(this, cmd, "Invalid user data", false);

        var (dbUser, errorMessage) = await UserDB.Get(requestedUser.Username);

        if (dbUser == null)
            return Helper.ErrorCmd(this, cmd, errorMessage, false);

        if (dbUser.Email != requestedUser.Email)
            return Helper.ErrorCmd(this, cmd, "Email doesn't match", false);

        return new(cmd.CommandType);
    }

    private async Task<Command> ResetPwd(Command cmd)
    {
        User? requestedUser = User.FromJson(cmd.Payload);

        if (requestedUser == null)
            return Helper.ErrorCmd(this, cmd, "Invalid user data", false);

        var (success, updateErrorMessage) = await UserDB.Update(requestedUser.Username, null, null, requestedUser.PwdSet);

        if (!success)
            return Helper.ErrorCmd(this, cmd, updateErrorMessage);

        LogHandler.AddLog($"Reset password of {requestedUser}", this);
        return new(cmd.CommandType);
    }

    private async Task<Command> ChangeNickname(Command cmd)
    {
        if (user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        string newNickname = cmd.Payload;

        var (success, errorMessage) = await UserDB.Update(user.UserID, newNickname, null, null);

        if (!success)
            return Helper.ErrorCmd(this, cmd, errorMessage);
        
        LogHandler.AddLog($"Changed nickname of {user} from '{user.Nickname}' to '{newNickname}'", this);
        user.Nickname = newNickname;
        return new(cmd.CommandType);
    }

    private async Task<Command> ChangeEmail(Command cmd)
    {
        if (user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        string newEmail = cmd.Payload;

        var (success, errorMessage) = await UserDB.Update(user.UserID, newEmail, null, null);

        if (!success)
            return Helper.ErrorCmd(this, cmd, errorMessage);
        
        LogHandler.AddLog($"Changed email of {user} from '{user.Email}' to '{newEmail}'", this);
        user.Nickname = newEmail;
        return new(cmd.CommandType);
    }

    private async Task<Command> ChangePassword(Command cmd)
    {
        if (user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        var newPwd = PasswordSet.FromJson(cmd.Payload);

        var (success, errorMessage) = await UserDB.Update(user.UserID, null, null, newPwd);

        if (!success)
            return Helper.ErrorCmd(this, cmd, errorMessage);
        
        LogHandler.AddLog($"Changed password of {user}", this);
        user.PwdSet = newPwd;
        return new(cmd.CommandType);
    }

    private async Task<Command> UploadSave(Command cmd)
    {
        if (user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        GameSave? gameSave = GameSave.FromJson(cmd.Payload);

        if (gameSave == null)
            return Helper.ErrorCmd(this, cmd, "Invalid game save");

        gameSave.Name = $"{user.Username}Save";

        var (success, errorMessage) = await GameSaveDB.Save(user.UserID, gameSave);

        if (!success)
            return Helper.ErrorCmd(this, cmd, errorMessage);

        LogHandler.AddLog($"Uploaded save", this);
        return new(cmd.CommandType);
    }

    private async Task<Command> DownloadSave(Command cmd)
    {
        if (user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        var (gameSave, errorMessage) = await GameSaveDB.Load(user.UserID);

        if (gameSave == null)
            return Helper.ErrorCmd(this, cmd, errorMessage);

        LogHandler.AddLog($"Downloaded save", this);
        return new(cmd.CommandType, gameSave.ToJson());
    }

    private async Task<Command> UploadScore(Command cmd)
    {
        if (user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        Score? score = Score.FromJson(cmd.Payload);

        if (score == null)
            return Helper.ErrorCmd(this, cmd, "Invalid score");

        var (success, errorMessage) = await ScoreDB.Add(score);

        if (!success)
            return Helper.ErrorCmd(this, cmd, errorMessage);

        LogHandler.AddLog($"Uploaded score", this);
        return new(cmd.CommandType);
    }

    private async Task<Command> GetUserScores(Command cmd)
    {
        if (user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        var (personal, errorMessage) = await ScoreDB.GetUser(user.UserID);

        if (personal == null)
            return Helper.ErrorCmd(this, cmd, errorMessage);

        LogHandler.AddLog($"Got scores", this);
        return new(cmd.CommandType, JsonSerializer.Serialize(personal));
    }

    private async Task<Command> GetMonthlyScores(Command cmd)
    {
        if (user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        var (monthly, errorMessage) = await ScoreDB.GetMonthly();

        if (monthly == null)
            return Helper.ErrorCmd(this, cmd, errorMessage);

        LogHandler.AddLog($"Got monthly scores", this);
        return new(cmd.CommandType, JsonSerializer.Serialize(monthly));
    }

    private async Task<Command> GetAllTimeScores(Command cmd)
    {
        if (user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        var (alltime, errorMessage) = await ScoreDB.GetAllTime();

        if (alltime == null)
            return Helper.ErrorCmd(this, cmd, errorMessage);

        LogHandler.AddLog($"Got all time scores", this);
        return new(cmd.CommandType, JsonSerializer.Serialize(alltime));
    }

    private Command Disconnect(Command cmd)
    {
        LogHandler.AddLog($"Disconnected", this);
        user = null;
        return new(cmd.CommandType);
    }

    private class Helper
    {
        /// <summary>
        /// Set error command & log error based on 'errorMessage'
        /// </summary>
        public static Command ErrorCmd(ClientHandler client, Command sourceCmd, string errorDetail, bool addLog = true)
        {
            string logContent = $"Error: (Cmd: {sourceCmd.Name()}) (Detail: {errorDetail})";

            if (addLog) LogHandler.AddLog(logContent, client);
            return new(CommandType.Error, errorDetail);
        }
    }
}