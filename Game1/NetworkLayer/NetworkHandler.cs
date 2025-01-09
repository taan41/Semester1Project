using System.Net.Sockets;

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

    public bool CheckUsername(string username, out string errorMessage)
    {
        Command cmdToSend = new(CommandType.CheckUsername, username);
        return HandleCommand(cmdToSend, out _, out errorMessage);
    }

    public bool Register(User user, out string errorMessage)
    {
        Command cmdToSend = new(CommandType.Register, user.ToJson());
        return HandleCommand(cmdToSend, out _, out errorMessage);
    }

    public PasswordSet? GetPassword(string username, out string errorMessage)
    {
        Command cmdToSend = new(CommandType.GetUserPwd, username);
        return HandleCommand(cmdToSend, out Command receivedCmd, out errorMessage)
            ? PasswordSet.FromJson(receivedCmd.Payload)
            : null;
    }

    public bool Login(out string errorMessage)
    {
        Command cmdToSend = new(CommandType.Login);
        return HandleCommand(cmdToSend, out _, out errorMessage);
    }

    public bool RequestResetPwd(User user, out string errorMessage)
    {
        Command cmdToSend = new(CommandType.RequestResetPwd, user.ToJson());
        return HandleCommand(cmdToSend, out _, out errorMessage);
    }

    public bool ResetPwd(User user, out string errorMessage)
    {
        Command cmdToSend = new(CommandType.ResetPwd, user.ToJson());
        return HandleCommand(cmdToSend, out _, out errorMessage);
    }

    private bool HandleCommand(Command cmdToSend, out Command receivedCmd, out string errorMessage)
    {
        receivedCmd = new();
        errorMessage = "";

        if (!IsConnected)
        {
            errorMessage = "Can't connect to server";
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
            errorMessage = $"Can't connect to server";
            return false;
        }
        
        Command? tempCmd = Command.FromJson(Encode.GetString(buffer, 0, totalRead));
        Array.Clear(buffer);

        if(tempCmd == null)
        {
            errorMessage = "Null command";
            return false;
        }

        switch(tempCmd.CommandType)
        {
            case var value when value == cmdToSend.CommandType:
                receivedCmd = tempCmd;
                return true;

            case CommandType.Error:
                errorMessage = tempCmd.Payload;
                return false;

            default:
                errorMessage = $"Received invalid command: {tempCmd.CommandType}";
                return false;
        }
    }
}