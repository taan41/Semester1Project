using System.Net;
using System.Net.Sockets;

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
                receivedCmd = Json.Deserialize<Command>(Encode.GetString(buffer, 0, bytesRead));

                switch (receivedCmd?.CommandType)
                {
                    case CommandType.Ping:
                        cmdToSend.Set(CommandType.Ping, null);
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

                    case CommandType.ChangeNickname:
                        cmdToSend = await ChangeNickname(receivedCmd);
                        break;

                    case CommandType.ChangePassword:
                        cmdToSend = await ChangePassword(receivedCmd);
                        break;

                    case CommandType.Disconnect:
                        LogHandler.AddLog($"Client disconnected", this);
                        return;

                    default:
                        cmdToSend = Helper.ErrorCmd(this, new(CommandType.Error, null), "Received invalid cmd");
                        break;
                }

                if (cmdToSend.CommandType != CommandType.Empty)
                {
                    await stream.WriteAsync(Encode.GetBytes(Json.Serialize(cmdToSend)), token);
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
        var (success, errorMessage) = await DBHandler.UserDB.CheckUsername(cmd.Payload);

        if(!success)
            return Helper.ErrorCmd(this, cmd, errorMessage, false);

        return new(cmd.CommandType);
    }

    private async Task<Command> Register(Command cmd)
    {
        User? registeredUser = Json.Deserialize<User>(cmd.Payload);

        if(registeredUser == null)
            return Helper.ErrorCmd(this, cmd, "Invalid registering user");

        var (success, errorMessage) = await DBHandler.UserDB.Add(registeredUser);

        if(!success)
            return Helper.ErrorCmd(this, cmd, errorMessage);

        LogHandler.AddLog($"Registered with Username '{registeredUser.Username}'", this);
        return new(cmd.CommandType);
    }

    private async Task<(Command cmdToSend, User? requestedUser)> GetUserPwd(Command cmd)
    {
        var (requestedUser, errorMessage) = await DBHandler.UserDB.Get(cmd.Payload);

        if(requestedUser == null)
            return (Helper.ErrorCmd(this, cmd, errorMessage, false), null);

        if(requestedUser.PwdSet == null)
            return (Helper.ErrorCmd(this, cmd, "No password found"), null);

        return (new(cmd.CommandType, Json.Serialize(requestedUser.PwdSet)), requestedUser);
    }

    private Command Login(Command cmd, User? tempUser)
    {
        if (user != null || tempUser == null)
            return Helper.ErrorCmd(this, cmd, "Invalid user/tempUser");

        user = tempUser;
        LogHandler.AddLog($"Logged in as {user}", endPoint);
        return new(cmd.CommandType, Json.Serialize(user));
    }

    private Command Logout(Command cmd)
    {
        if (user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        LogHandler.AddLog($"Logged out from {user}", endPoint);
        user = null;
        return new(cmd.CommandType);
    }

    private async Task<Command> ChangeNickname(Command cmd)
    {
        if(user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        string newNickname = cmd.Payload;

        var (success, errorMessage) = await DBHandler.UserDB.Update(user.UserID, newNickname, null, null);

        if(!success)
            return Helper.ErrorCmd(this, cmd, errorMessage);
        
        LogHandler.AddLog($"Changed nickname of {user} from '{user.Nickname}' to '{newNickname}'", this);
        user.Nickname = newNickname;
        return new(cmd.CommandType);
    }

    private async Task<Command> ChangeEmail(Command cmd)
    {
        if(user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        string newEmail = cmd.Payload;

        var (success, errorMessage) = await DBHandler.UserDB.Update(user.UserID, newEmail, null, null);

        if(!success)
            return Helper.ErrorCmd(this, cmd, errorMessage);
        
        LogHandler.AddLog($"Changed email of {user} from '{user.Email}' to '{newEmail}'", this);
        user.Nickname = newEmail;
        return new(cmd.CommandType);
    }

    private async Task<Command> ChangePassword(Command cmd)
    {
        if(user == null || user.UserID < 1)
            return Helper.ErrorCmd(this, cmd, "Invalid user");

        var newPwd = Json.Deserialize<PasswordSet>(cmd.Payload);

        var (success, errorMessage) = await DBHandler.UserDB.Update(user.UserID, null, null, newPwd);

        if(!success)
            return Helper.ErrorCmd(this, cmd, errorMessage);
        
        LogHandler.AddLog($"Changed password of {user}", this);
        user.PwdSet = newPwd;
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