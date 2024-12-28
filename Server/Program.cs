using System.Net;
using System.Net.Sockets;
using static System.Console;

class Server
{
    const int defaultPort = 5000;

    static readonly List<ClientHandler> clientHandlers = [];
    static TcpListener? listener;

    public static void Main()
    {
        string? serverIP = null;
        int port = defaultPort;

        try
        {
            InitMySqlConn();

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
        catch (Exception ex)
        {
            WriteLine($" Server error: {ex.Message}");
            ReadKey(true);
        }
    }

    static void InitMySqlConn()
    {
        string? server, db, uid, password;

        while (true)
        {
            UIHelper.DrawLine('-');
            UIHelper.WriteCenter("Server Control Center");
            UIHelper.DrawLine('-');
            WriteLine(" MySql database info:");

            Write(" Server (default 'localhost'): ");
            if (string.IsNullOrWhiteSpace(server = ReadLine()))
                server = "localhost";

            Write(" Database (default 'consoleconquer'): ");
            if (string.IsNullOrWhiteSpace(db = ReadLine()))
                db = "consoleconquer";

            Write(" UID (default 'root'): ");
            if (string.IsNullOrWhiteSpace(uid = ReadLine()))
                uid = "root";

            Write(" Password: ");
            if ((password = ReadLine()) == null)
                throw new("Cancelled initializing MySql connection");


            if (!DBHandler.Initialize(server, db, uid, password, out string errorMessage))
                throw new($" Error while connecting to MySql DB: {errorMessage}");

            WriteLine(" Connect to MySql database successfully");
            
            LogHandler.Initialize();

            ReadKey(true);
            return;
        }
    }

    static bool StartServer(ref string? serverIP, ref int port)
    {
        while(true)
        {
            ServerHelper.StartServerMenu(serverIP, port);

            switch(ReadLine())
            {
                case "1":
                    return true;

                case "2":
                    Write(" Enter IP: ");
                    serverIP = ReadLine();

                    if(serverIP == null || !ServerHelper.CheckIPv4(serverIP))
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
                    while(ServerHelper.ViewLog());
                    continue;

                case "0": case null:
                    WriteLine(" Shutting down program...");
                    return false;

                default: continue;
            }
        }
    }

    static void ServerOverview(string? serverIP, int port)
    {
        while (true)
        {
            ServerHelper.OverviewMenu(serverIP, port);

            switch (ReadLine())
            {
                case "1":
                    ViewConnectedClients();
                    continue;

                case "2":

                case "3":
                    break;

                case "4":
                    while (ServerHelper.ViewLog()) ;
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

    static void ViewConnectedClients()
    {
        int curPage = 0, maxPage;

        while(true)
        {
            maxPage = (clientHandlers.Count - 1) / 10;

            ServerHelper.ConnectedClients(clientHandlers, curPage);

            switch (ReadLine())
            {
                case "8":
                    if (curPage > 0)
                        curPage--;
                    continue;

                case "9":
                    if (curPage < maxPage)
                        curPage++;
                    continue;

                case "0": case null:
                    return;

                default:
                    continue;
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
    }

    public static void RemoveClient(ClientHandler client)
    {
        lock(clientHandlers)
        {
            clientHandlers.Remove(client);
        }
    }
}