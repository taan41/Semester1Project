using System.Net.Sockets;
using DAL;
using DAL.ConfigClasses;
using NetworkLL.DataTransferObjects;

using static NetworkLL.Utilities;

namespace NetworkLL
{
    public class NetworkHandler
    {
        private static ServerConfig ServerConfig => ConfigManager.Instance.ServerConfig;

        private TcpClient? client;
        private NetworkStream? stream;
        private byte[] buffer = new byte[1024];

        public static NetworkHandler Instance { get; private set; } = new();

        public bool IsConnected => client != null && client.Connected && stream != null;

        private NetworkHandler() {}

        public bool Connect()
        {
            if (IsConnected)
                return true;

            try
            {
                client = new TcpClient(ServerConfig.ServerIP, ServerConfig.Port);
                stream = client.GetStream();
                return true;
            }
            catch (Exception ex) when (ex is SocketException or IOException)
            {
                Close();
                return false;
            }
        }

        public void Close()
        {
            if (IsConnected)
            {
                Communicate(new(Command.Type.Disconnect), out _);
            }
            stream?.Close();
            client?.Close();
            stream = null;
            client = null;
        }

        public bool Communicate(Command cmdToSend, out string result)
        {
            if (!IsConnected)
            {
                result = "Can't connect to server";
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
                result = $"Can't connect to server";
                return false;
            }
            
            Command? tempCmd = Command.FromJson(Encode.GetString(buffer, 0, totalRead));
            Array.Clear(buffer);

            switch(tempCmd?.CommandType)
            {
                case var value when value == cmdToSend.CommandType:
                    result = tempCmd!.Payload;
                    return true;

                case Command.Type.Error:
                    result = tempCmd.Payload;
                    return false;

                default:
                    result = $"Received invalid data";
                    return false;
            }
        }
    }
}