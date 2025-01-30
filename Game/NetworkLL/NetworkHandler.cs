using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using DAL;
using DAL.ConfigClasses;
using NetworkLL.DataTransferObjects;

using static NetworkLL.NetworkUtilities;

namespace NetworkLL
{
    public class NetworkHandler
    {
        private static ServerConfig ServerConfig => ConfigManager.Instance.ServerConfig;

        private TcpClient? client;
        private NetworkStream? stream;
        private Aes? aes;

        private byte[] buffer = new byte[2048];

        public static NetworkHandler Instance { get; private set; } = new();

        public bool IsConnected => client != null && client.Connected && stream != null;

        private NetworkHandler() {}

        public bool Connect()
        {
            if (IsConnected)
                return true;

            try
            {
                string ip = CheckIPv4(ServerConfig.ServerIP) ? ServerConfig.ServerIP : "127.0.0.1";
                client = new TcpClient(ip, ServerConfig.Port);
                stream = client.GetStream();

                Security.InitAESClient(stream, out aes);

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
                Communicate(new(Command.Type.Disconnect), out _, false);
            }
            stream?.Close();
            client?.Close();
            stream = null;
            client = null;
        }

        public bool Communicate(Command cmdToSend, out string result, bool getResponse = true)
        {
            if (!IsConnected)
            {
                result = "Can't connect to server";
                return false;
            }

            if (aes == null)
            {
                result = "AES key is not set";
                return false;
            }

            int bytesRead, totalRead = 0;
            Array.Clear(buffer);

            try
            {
                stream!.Write(Security.EncryptString(cmdToSend.ToJson(), aes));

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

            if (!getResponse)
            {
                result = string.Empty;
                return true;
            }
            
            Command? tempCmd = Command.FromJson(Security.DecryptString(buffer[..totalRead], aes));

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