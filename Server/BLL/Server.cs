using System.Net;
using System.Net.Sockets;
using DAL.Config;
using DAL.DBHandlers;

namespace BLL;

public class Server
{
    private static ConfigManager Config => ConfigManager.Instance;
    private static DBManager DBManager => DBManager.Instance;

    private TcpListener? listener;
    private CancellationTokenSource? serverStopToken;

    public static Server Instance { get; } = new();

    public List<ClientHandler> Clients { get; private set; } = [];

    public async Task<(bool success, string error)> InitializeDB(string sqlIP, string uid, string password)
    {
        if (!DBManager.Initialize(sqlIP, uid, password, out string error))
        {
            LogHandler.AddLog($"Error while connecting to MySql DB: {error}");
            return (false, error);
        }

        LogHandler.Initialize();
        await ConfigManager.Instance.LoadConfig(true);
        return (true, "");
    }

    public async Task Start(string? serverIP, int port)
    {
        listener = serverIP == null ? new(IPAddress.Any, port) : new(IPAddress.Parse(serverIP), port);
        listener.Start();

        Config.ServerConfig.Port = port;
        await Config.ServerConfig.Save();

        LogHandler.AddLog($"Server started on address: {serverIP ?? "Any"}, port: {port}");

        serverStopToken = new();
        _ = Task.Run(() => AcceptClientsAsync(listener, serverStopToken.Token));
    }

    public void Stop()
    {
        serverStopToken?.Cancel();
        listener?.Stop();

        LogHandler.AddLog("Server stopped");
    }
    
    public async Task AcceptClientsAsync(TcpListener listener, CancellationToken serverStopToken)
    {
        try
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(serverStopToken);

                if (serverStopToken.IsCancellationRequested)
                    return;

                ClientHandler clientHandler = new(client);

                lock(Clients)
                    Clients.Add(clientHandler);

                _ = Task.Run(() => clientHandler.HandlingClientAsync(serverStopToken), CancellationToken.None);
            }
        }
        catch (OperationCanceledException) {}
        catch (Exception ex)
        {
            LogHandler.AddLog($"Error: {ex.Message}");
        }
        finally
        {
            listener.Stop();
        }
    }

    public void RemoveClient(ClientHandler client)
    {
        lock(Clients)
        {
            Clients.Remove(client);
        }
    }
}