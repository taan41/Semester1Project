using System.Net;
using System.Net.Sockets;
using static System.Console;

class Server
{
    const int defaultPort = 5000;

    static readonly List<ClientHandler> clientHandlers = [];
    static TcpListener? listener;

    public static void Start()
    {
        string? serverIP = null;
        int port = defaultPort;

        ServerUI.FillDBInfo();

        while (true)
        {
            if (!StartServer(ref serverIP, ref port))
                return;

            if (serverIP == null)
                listener = new(IPAddress.Any, port);
            else
                listener = new(IPAddress.Parse(serverIP), port);

            listener.Start();
            LogHandler.AddLog($"Server starts on address: {serverIP ?? "Any"}, port: {port}");

            CancellationTokenSource serverStopToken = new();
            _ = Task.Run(() => AcceptClientsAsync(listener, serverStopToken.Token));

            ServerOverview(serverIP, port);

            serverStopToken.Cancel();
            listener.Stop();
            LogHandler.AddLog("Server stops");
        }
    }

    static bool StartServer(ref string? serverIP, ref int port)
    {
        while(true)
        {
            ServerUI.MainMenu(serverIP, port, false);

            switch(ReadLine())
            {
                case "1":
                    return true;

                case "2":
                    Write(" Enter IP: ");
                    serverIP = ReadLine();

                    if(serverIP == null || !CheckIPv4(serverIP))
                    {
                        serverIP = null;
                        WriteLine(" Invalid IP.");
                        ReadKey(true);
                    }
                    continue;

                case "3":
                    Write(" Enter port: ");
                    try
                    {
                        port = Convert.ToInt32(ReadLine());
                        
                        if(port < 0 || port > 65535)
                            throw new FormatException();
                    }
                    catch(FormatException)
                    {
                        port = defaultPort;
                        WriteLine(" Invalid port");
                        ReadKey(true);
                    }
                    continue;

                case "4":
                    ServerUI.ViewLog();
                    continue;

                case "0": case null:
                    WriteLine(" Exiting program...");
                    return false;

                default: continue;
            }
        }
    }

    static void ServerOverview(string? serverIP, int port)
    {
        while (true)
        {
            ServerUI.MainMenu(serverIP, port, true);

            switch (ReadLine())
            {
                case "1":
                    ServerUI.ViewConnectedClients(clientHandlers);
                    continue;

                case "2":

                case "3":
                    break;

                case "4":
                    ServerUI.ViewLog();
                    continue;

                case "0":
                case null:
                    WriteLine(" Shutting down server...");
                    ReadKey(true);
                    return;

                default: continue;
            }
        }
    }
    
    static async Task AcceptClientsAsync(TcpListener listener, CancellationToken token)
    {
        try
        {
            while(true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(token);

                if(token.IsCancellationRequested)
                    return;

                ClientHandler clientHandler = new(client);

                lock(clientHandlers)
                    clientHandlers.Add(clientHandler);

                _ = Task.Run(() => clientHandler.HandlingClientAsync(token), token);
            }
        }
        catch(OperationCanceledException) {}
        catch(Exception ex)
        {
            LogHandler.AddLog($"Error: {ex.Message}");
        }
        finally
        {
            listener.Stop();
        }
    }

    public static void RemoveClient(ClientHandler client)
    {
        lock(clientHandlers)
        {
            clientHandlers.Remove(client);
        }
    }

    private static bool CheckIPv4(string? ipAddress)
    {
        if(!IPAddress.TryParse(ipAddress, out _))
            return false;

        string[] parts = ipAddress.Split('.');
        if(parts.Length != 4) return false;

        foreach(string part in parts)
        {
            if (!int.TryParse(part, out int number))
                return false;

            if (number < 0 || number > 255)
                return false;

            if (part.Length > 1 && part[0] == '0')
                return false;
        }

        return true;
    }
}