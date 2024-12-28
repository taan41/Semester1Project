using System.Net;
using System.Net.Sockets;

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

    }
}