using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using DAL.Config;
using DAL.DBHandlers;
using DAL.DataModels;

using static BLL.DataPacket;
using static BLL.GenericUtilities;
using static BLL.ServerUtilities;

namespace BLL
{
    public class ClientHandler
    {
        readonly TcpClient _client;
        readonly NetworkStream _stream;
        readonly EndPoint _endPoint;
        readonly Aes aes;

        User? _mainUser = null;

        public EndPoint EndPoint => _endPoint;
        public User? User => _mainUser;

        public ClientHandler(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
            _endPoint = client.Client.RemoteEndPoint!;

            Security.InitAESServer(_stream, out aes);

            LogHandler.AddLog($"Connected to server", this);
        }

        public override string ToString()
            => _mainUser?.ToString() ?? _endPoint?.ToString() ?? "Null client";

        public void Close()
        {
            aes.Dispose();
            _stream.Close();
            _client.Close();
        }

        public async Task HandlingClientAsync(CancellationToken token)
        {
            try
            {
                byte[] buffer = new byte[2048];
                int bytesRead, totalRead;
                bool disconnect = false;

                DataPacket? clientPacket;

                while (!token.IsCancellationRequested)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    totalRead = 0;

                    while((bytesRead = await _stream.ReadAsync(buffer, totalRead, 1024, token)) > 0)
                    {
                        totalRead += bytesRead;

                        if (bytesRead < 1024)
                            break;

                        if (totalRead + 1024 > buffer.Length)
                            Array.Resize(ref buffer, buffer.Length * 2);
                    }
                    
                    clientPacket = FromJson<DataPacket>(Security.DecryptString(buffer[..totalRead], aes));

                    if (clientPacket == null)
                    {
                        LogHandler.AddLog("Invalid packet received", this);
                        continue;
                    }

                    DataPacket responsePacket = (PacketType) clientPacket.Type switch
                    {
                        PacketType.Generic => HandleGenericPacket(clientPacket, ref disconnect),
                        PacketType.Database => await HandleDatabasePacket(clientPacket),
                        PacketType.User => await HandleUserPacket(clientPacket),
                        PacketType.Game => await HandleGamePacket(clientPacket),
                        _ => LogErrorPacket(this, clientPacket, "Invalid packet type"),
                    };

                    if (disconnect)
                        break;

                    byte[] responseBytes = Security.EncryptString(ToJson(responsePacket), aes);
                    await _stream.WriteAsync(responseBytes, 0, responseBytes.Length, token);
                }

            }
            catch (OperationCanceledException) {}
            catch (IOException)
            {
                LogHandler.AddLog("Disconnected unexpectedly", this);
            }
            catch (Exception ex)
            {
                LogHandler.AddLog($"Error while handling client: {ex}", this);
            }
            finally
            {
                Server.RemoveClient(this);
                Close();
            }
        }

        /// <summary>
        /// Return error packet & log errorDetail
        /// </summary>
        private static DataPacket LogErrorPacket(ClientHandler client, DataPacket sourcePacket, string errorDetail, bool addLog = true)
        {
            string logContent = $"Error: (Cmd:{sourcePacket.Type},{sourcePacket.Request}) (Detail:{errorDetail})";

            if (addLog) LogHandler.AddLog(logContent, client);
            return new((int) PacketType.Generic, (int) GenericRequest.Error, errorDetail);
        }

        private DataPacket HandleGenericPacket(DataPacket clientPacket, ref bool disconnect)
        {
            if (typeof(GenericRequest).IsEnumDefined(clientPacket.Request))
            {
                disconnect = clientPacket.Request == (int) GenericRequest.Disconnect;
                return new(clientPacket);
            }
            else
            {
                return LogErrorPacket(this, clientPacket, "Invalid generic request");
            }
        }

        private async Task<DataPacket> HandleDatabasePacket(DataPacket clientPacket)
            => (DatabaseRequest) clientPacket.Request switch
                {
                    DatabaseRequest.ConfigGame => new(clientPacket, ToJson(ConfigManager.Instance.GameConfig)),
                    DatabaseRequest.ConfigServer => new(clientPacket, ToJson(ConfigManager.Instance.ServerConfig)),
                    DatabaseRequest.ConfigDatabase => new(clientPacket, ToJson(ConfigManager.Instance.DatabaseConfig)),
                    DatabaseRequest.UpdateEquip => await DBResult(clientPacket, EquipmentDB.GetAll()),
                    DatabaseRequest.UpdateSkill => await DBResult(clientPacket, SkillDB.GetAll()),
                    DatabaseRequest.UpdateMonster => await DBResult(clientPacket, MonsterDB.GetAll(ConfigManager.Instance.GameConfig.ProgressMaxFloor)),
                    _ => LogErrorPacket(this, clientPacket, "Invalid database request"),
                };

        private async Task<DataPacket> HandleUserPacket(DataPacket clientPacket)
            => (UserRequest) clientPacket.Request switch
                {
                    UserRequest.UsernameAvailable => await DBResult(clientPacket, UserDB.CheckUsername(clientPacket.Payload)),
                    UserRequest.UserRegister => await Register(clientPacket),
                    UserRequest.UserGet => await DBResult(clientPacket, UserDB.Get(clientPacket.Payload)),
                    UserRequest.UserUpdate => await UserUpdate(clientPacket),
                    UserRequest.UserDelete => await Delete(clientPacket),
                    UserRequest.Login => Login(clientPacket),
                    UserRequest.Logout => Logout(clientPacket),
                    _ => LogErrorPacket(this, clientPacket, "Invalid user request"),
                };

        private async Task<DataPacket> HandleGamePacket(DataPacket clientPacket)
            => (GameRequest) clientPacket.Request switch
                {
                    GameRequest.UploadSave => await PersonalDBResult(clientPacket, GameSaveDB.Save(_mainUser!.UserID, clientPacket.Payload)),
                    GameRequest.DownloadSave => await PersonalDBResult(clientPacket, GameSaveDB.Load(_mainUser!.UserID)),
                    GameRequest.UploadScore => await PersonalDBResult(clientPacket, ScoreDB.Add(FromJson<Score>(clientPacket.Payload))),
                    GameRequest.PersonalScores => await PersonalDBResult(clientPacket, ScoreDB.GetPersonal(_mainUser!.UserID)),
                    GameRequest.MonthlyScores => await DBResult(clientPacket, ScoreDB.GetMonthly()),
                    GameRequest.AllTimeScores => await DBResult(clientPacket, ScoreDB.GetAllTime()),
                    _ => LogErrorPacket(this, clientPacket, "Invalid game request"),
                };

        private async Task<DataPacket> DBResult<T>(DataPacket clientPacket, Task<(T?, string)> dbTask, bool logError = true)
        {
            var (result, error) = await dbTask;

            if (result == null || error != "")
                return LogErrorPacket(this, clientPacket, error, logError);

            return new(clientPacket, ToJson(result));
        }

        private async Task<DataPacket> PersonalDBResult<T>(DataPacket clientPacket, Task<(T?, string)> dbTask, bool logError = true)
        {
            if (_mainUser == null || _mainUser.UserID < 1)
                return LogErrorPacket(this, clientPacket, "Invalid logged-in user", logError);

            return await DBResult(clientPacket, dbTask, logError);
        }

        private async Task<DataPacket> Register(DataPacket clientPacket)
        {
            User? registeredUser = FromJson<User>(clientPacket.Payload);

            if (registeredUser == null)
                return LogErrorPacket(this, clientPacket, "Invalid registering user");

            var (success, error) = await UserDB.Add(registeredUser);

            if (!success)
                return LogErrorPacket(this, clientPacket, error);

            LogHandler.AddLog($"Registered with Username '{registeredUser.Username}'", this);
            return new(clientPacket);
        }

        private DataPacket Login(DataPacket clientPacket)
        {
            User? requestedUser = FromJson<User>(clientPacket.Payload);

            if (_mainUser != null || requestedUser == null || requestedUser.UserID < 1)
                return LogErrorPacket(this, clientPacket, "Invalid login");

            _mainUser = requestedUser;
            LogHandler.AddLog($"Logged in as {_mainUser}", _endPoint);
            return new(clientPacket);
        }

        private DataPacket Logout(DataPacket clientPacket)
        {
            if (_mainUser == null)
                return LogErrorPacket(this, clientPacket, "Invalid logout");

            LogHandler.AddLog($"Logged out from {_mainUser}", _endPoint);
            _mainUser = null;
            return new(clientPacket);
        }

        private async Task<DataPacket> Delete(DataPacket clientPacket)
        {
            if (_mainUser == null || _mainUser.UserID < 1)
                return LogErrorPacket(this, clientPacket, "Invalid user");

            var (success, error) = await UserDB.Delete(_mainUser.UserID);

            if (!success)
                return LogErrorPacket(this, clientPacket, error);

            LogHandler.AddLog($"Deleted account of {_mainUser}", this);
            _mainUser = null;
            return new(clientPacket);
        }

        private async Task<DataPacket> UserUpdate(DataPacket clientPacket, bool logError = true)
        {
            var updatingUser = FromJson<User>(clientPacket.Payload);

            if (updatingUser == null || updatingUser.UserID < 1)
                return LogErrorPacket(this, clientPacket, "Invalid updating user", logError);

            var (success, error) = await UserDB.Update(updatingUser);

            if (!success)
                return LogErrorPacket(this, clientPacket, error, logError);

            LogHandler.AddLog($"Updated info of {updatingUser.ToString(true)}", this);
            return new(clientPacket);
        }
    }
}