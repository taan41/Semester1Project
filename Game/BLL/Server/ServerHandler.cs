using System.Net.Sockets;
using System.Security.Cryptography;
using BLL.Game;
using BLL.Game.Components.Entity;
using BLL.Game.Components.Item;
using BLL.Game.Components.Others;
using BLL.Server.DataModels;
using DAL;
using BLL.Config;

using static BLL.Server.DataPacket;
using static BLL.Utilities.GenericUtilities;
using static BLL.Utilities.NetworkUtilities;

namespace BLL.Server
{
    public class ServerHandler
    {
        #region Properties
        private static ServerConfig ServerConfig => ConfigManager.Instance.ServerConfig;

        private TcpClient? _client;
        private NetworkStream? _stream;
        private Aes? _aes;

        private User? mainUser;
        private byte[] buffer = new byte[2048];

        public static ServerHandler Instance { get; private set; } = new();

        public bool IsConnected => _client != null && _client.Connected && _stream != null;
        public bool IsLoggedIn => IsConnected && mainUser != null;
        public string Username => mainUser?.Username ?? "Guest";
        public string Nickname => mainUser?.Nickname ?? "Guest";
        public string Email => mainUser?.Email ?? "Guest Email";
        #endregion

        public ServerHandler() {}

        #region PacketHandler
        private bool PacketHandler(DataPacket packet, out string result, bool getResponse = true)
        {
            if (!IsConnected)
            {
                result = "Can't connect to server";
                return false;
            }

            if (_aes == null)
            {
                result = "AES key is not set";
                return false;
            }

            int bytesRead, totalRead = 0;
            Array.Clear(buffer);

            try
            {
                _stream!.Write(Security.EncryptString(ToJson(packet), _aes));

                if (!getResponse)
                {
                    result = string.Empty;
                    return true;
                }

                while((bytesRead = _stream.Read(buffer, totalRead, 1024)) > 0)
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

            DataPacket? returnPacket = FromJson<DataPacket>(Security.DecryptString(buffer[..totalRead], _aes));

            if (returnPacket == null)
            {
                result = "Can't parse server response";
                return false;
            }

            if (returnPacket.Type == 0 && returnPacket.Request == 0)
            {
                result = "Corrupted response";
                return false;
            }
            
            result = returnPacket.Payload;

            if (returnPacket.Type == (int) PacketType.Generic && returnPacket.Request == (int) GenericRequest.Error)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Server Requests
        public bool Connect(out string error)
        {
            try
            {
                string ip = CheckIPv4(ServerConfig.ServerIP) ? ServerConfig.ServerIP : "127.0.0.1";
                _client = new TcpClient(ip, ServerConfig.Port);
                _stream = _client.GetStream();

                Security.InitAESClient(_stream, out _aes);

                error = "";
                return true;
            }
            catch (Exception ex) when (ex is SocketException or IOException)
            {
                Close();

                error = "Can't connect to server";
                return false;
            }
        }

        public void Close()
        {
            if (IsConnected)
            {
                PacketHandler(CreateGenericPacket(GenericRequest.Disconnect), out _, false);
            }

            _stream?.Close();
            _client?.Close();
            _stream = null;
            _client = null;
        }

        public bool UpdateConfig(out string error)
        {
            if (!PacketHandler(CreateDatabasePacket(DatabaseRequest.ConfigGame), out string result))
            {
                error = result;
                return false;
            }

            var gameConfig = FromJson<GameConfig>(result);
            if (gameConfig != null)
            {
                FileManager.WriteJson(FileManager.FolderNames.Configs, FileManager.FileNames.GameConfig, gameConfig);
            }

            if (!PacketHandler(CreateDatabasePacket(DatabaseRequest.ConfigDatabase), out result))
            {
                error = result;
                return false;
            }

            var dbConfig = FromJson<DatabaseConfig>(result);
            if (dbConfig != null)
            {
                FileManager.WriteJson(FileManager.FolderNames.Configs, FileManager.FileNames.DatabaseConfig, dbConfig);
            }

            ConfigManager.Instance.LoadConfig();

            error = "";
            return true;
        }

        public bool UpdateAssets(out string error)
        {
            if (!PacketHandler(CreateDatabasePacket(DatabaseRequest.UpdateEquip), out string result))
            {
                error = result;
                return false;
            }

            var equipments = FromJson<Dictionary<int, Equipment>>(result);
            if (equipments != null)
            {
                FileManager.WriteJson(FileManager.FolderNames.Assets, FileManager.FileNames.Equips, equipments);
            }

            if (!PacketHandler(CreateDatabasePacket(DatabaseRequest.UpdateSkill), out result))
            {
                error = result;
                return false;
            }

            var skills = FromJson<Dictionary<int, Skill>>(result);
            if (skills != null)
            {
                FileManager.WriteJson(FileManager.FolderNames.Assets, FileManager.FileNames.Skills, skills);
            }

            if (!PacketHandler(CreateDatabasePacket(DatabaseRequest.UpdateMonster), out result))
            {
                error = result;
                return false;
            }

            var monsters = FromJson<Dictionary<int, Monster>>(result);
            if (monsters != null)
            {
                FileManager.WriteJson(FileManager.FolderNames.Assets, FileManager.FileNames.Monsters, monsters);
            }

            AssetLoader.LoadAsset();

            error = "";
            return true;
        }
        
        public bool CheckUsername(string username, out string error)
            => PacketHandler(CreateUserPacket(UserRequest.UsernameAvailable, username), out error);

        public bool Register(string username, string nickname, string password, string email, out string error)
            => PacketHandler(CreateUserPacket(UserRequest.UserRegister, ToJson(new User(username, nickname, password, email))), out error);

        public bool ResetPassword(string username, string email, string password, out string error)
        {
            var requestedUser = GetUser(username, out error);

            if (requestedUser == null)
                return false;

            if (requestedUser.Email != email)
            {
                error = "Invalid email";
                return false;
            }

            requestedUser.PwdSet = new(password);
            return PacketHandler(CreateUserPacket(UserRequest.UserUpdate, ToJson(requestedUser)), out error);
        }

        public bool Login(string username, string password, out string error)
        {
            var requestedUser = GetUser(username, out error);

            if (requestedUser == null)
                return false;

            var pwdSet = requestedUser?.PwdSet;

            if (pwdSet == null || !Security.VerifyPassword(password, pwdSet))
            {
                error = "Invalid password";
                return false;
            }

            if (!PacketHandler(CreateUserPacket(UserRequest.Login, ToJson(requestedUser)), out error))
                return false;

            mainUser = requestedUser;
            return true;
        }

        public bool Logout(out string error)
        {
            if (!IsLoggedIn)
            {
                error = "Not logged in";
                return false;
            }

            if (!PacketHandler(CreateUserPacket(UserRequest.Logout), out error))
                return false;

            mainUser = null;
            return true;
        }

        public User? GetUser(string username, out string error)
        {
            if (!PacketHandler(CreateUserPacket(UserRequest.UserGet, username), out string result))
            {
                error = result;
                return null;
            }

            var requestedUser = FromJson<User>(result);
            if (requestedUser == null)
            {
                error = "Can't parse server response";
                return null;
            }

            error = "";
            return requestedUser;
        }

        public bool VerifyPassword(string currentPassword)
            => mainUser != null && mainUser.PwdSet != null && Security.VerifyPassword(currentPassword, mainUser.PwdSet);

        public bool UpdateMainUser(string? nickname, string? email, string? password, out string error)
        {
            if (!IsLoggedIn)
            {
                error = "Not logged in";
                return false;
            }

            User tempUser = new()
            {
                UserID = mainUser!.UserID,
                Username = mainUser.Username,
                Nickname = nickname ?? mainUser.Nickname,
                Email = email ?? mainUser.Email,
                PwdSet = password != null ? new(password) : mainUser.PwdSet
            };

            if (!PacketHandler(CreateUserPacket(UserRequest.UserUpdate, ToJson(tempUser)), out error))
                return false;
            
            mainUser = tempUser;
            return true;
        }

        public bool DeleteAccount(out string error)
        {
            if (!IsLoggedIn)
            {
                error = "Not logged in";
                return false;
            }

            if (!PacketHandler(CreateUserPacket(UserRequest.UserDelete), out error))
                return false;

            mainUser = null;
            return true;
        }

        public bool DownloadSave(out string error)
        {
            if (!PacketHandler(CreateGamePacket(GameRequest.DownloadSave), out string result))
            {
                error = result;
                return false;
            }

            var cloudSave = FromJson<GameSave>(result);
            if (cloudSave == null)
            {
                error = "Can't parse server response";
                return false;
            }

            cloudSave.Name = "CloudSave";
            GameSave.SaveLocal(cloudSave);
            
            error = "";
            return true;
        }

        public bool UploadSave(GameSave save, out string error)
            => PacketHandler(CreateGamePacket(GameRequest.UploadSave, ToJson(save)), out error);

        public bool UploadScore(int runID, TimeSpan clearTime, out string error)
        {
            if (!IsLoggedIn)
            {
                error = "Not logged in";
                return false;
            }

            Score score = new()
            {
                RunID = runID,
                UserID = mainUser!.UserID,
                Nickname = mainUser.Nickname!,
                ClearTime = clearTime
            };

            return PacketHandler(CreateGamePacket(GameRequest.UploadScore, ToJson(score)), out error);
        }

        private List<string> RequestScores(GameRequest request, out string error)
        {
            if (!PacketHandler(CreateGamePacket(request), out string result))
            {
                error = result;
                return [];
            }

            var scores = FromJson<List<Score>>(result);

            if (scores == null)
            {
                error = "Can't parse server response";
                return [];
            }

            error = "";
            return scores.ConvertAll(score => score.ToString());
        }

        public bool GetAllScores(out List<string> personal, out List<string> monthly, out List<string> allTime, out string error)
        {
            error = "";

            personal = RequestScores(GameRequest.PersonalScores, out string personalError);
            error += personalError != "" ? personalError + "|" : "";

            monthly = RequestScores(GameRequest.MonthlyScores, out string monthlyError);
            error += monthlyError != "" ? monthlyError + "|" : "";

            allTime = RequestScores(GameRequest.AllTimeScores, out string allTimeError);
            error += allTimeError != "" ? allTimeError + "|" : "";

            return error == "";
        }
        #endregion
    }
}