using System.Net;
using System.Net.Sockets;
using DAL;
using DAL.DBHandlers;

namespace BLL
{
    public static class Server
    {
        private static ConfigManager Config => ConfigManager.Instance;

        private static TcpListener? listener;
        private static CancellationTokenSource? serverStopToken;

        public static readonly List<ClientHandler> ClientList = [];

        public static async Task<(bool success, string error)> InitializeDB(string server, string db, string uid, string password)
        {
            if (!DBManager.Initialize(server, db, uid, password, out string error))
            {
                LogHandler.AddLog($"Error while connecting to MySql DB: {error}");
                return (false, error);
            }

            LogHandler.Initialize();
            await ConfigManager.Instance.LoadConfig(true);
            return (true, "");
        }

        public static async Task Start(string? serverIP, int port)
        {
            listener = serverIP == null ? new(IPAddress.Any, port) : new(IPAddress.Parse(serverIP), port);
            listener.Start();

            Config.ServerConfig.Port = port;
            await Config.ServerConfig.Save();

            LogHandler.AddLog($"Server started on address: {serverIP ?? "Any"}, port: {port}");

            serverStopToken = new();
            _ = Task.Run(() => AcceptClientsAsync(listener, serverStopToken.Token));
        }

        public static void Stop()
        {
            serverStopToken?.Cancel();
            listener?.Stop();

            LogHandler.AddLog("Server stopped");
        }
        
        static async Task AcceptClientsAsync(TcpListener listener, CancellationToken serverStopToken)
        {
            try
            {
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(serverStopToken);

                    if (serverStopToken.IsCancellationRequested)
                        return;

                    ClientHandler clientHandler = new(client);

                    lock(ClientList)
                        ClientList.Add(clientHandler);

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

        public static void RemoveClient(ClientHandler client)
        {
            lock(ClientList)
            {
                ClientList.Remove(client);
            }
        }
    }
}