using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using DAL;
using DAL.DBHandlers;
using DAL.Persistence.DataTransferObjects;
using DAL.Persistence.GameComponents.Others;

using static BLL.ServerHelper;

namespace BLL
{
    public class ClientHandler
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
            try
            {
                byte[] buffer = new byte[2048];
                Memory<byte> memory = new(buffer, 0, buffer.Length);
                int bytesRead;

                Command? receivedCmd;
                Command cmdToSend = new();

                User? tempUser = null;

                while((bytesRead = await stream.ReadAsync(memory, token)) > 0)
                {
                    receivedCmd = FromJson<Command>(Encode.ToString(buffer, 0, bytesRead));

                    switch (receivedCmd?.CommandType)
                    {
                        case Command.Type.Ping:
                            cmdToSend = new(Command.Type.Ping, null);
                            break;

                        case Command.Type.CheckUsername:
                            cmdToSend = await CheckUsername(receivedCmd);
                            break;

                        case Command.Type.Register:
                            cmdToSend = await Register(receivedCmd);
                            break;
                        
                        case Command.Type.GetUserPwd:
                            (cmdToSend, tempUser) = await GetUserPwd(receivedCmd);
                            break;

                        case Command.Type.Login:
                            cmdToSend = Login(receivedCmd, tempUser);
                            break;

                        case Command.Type.Logout:
                            cmdToSend = Logout(receivedCmd);
                            break;

                        case Command.Type.ValidateEmail:
                            cmdToSend = await ValidateEmail(receivedCmd);
                            break;

                        case Command.Type.ResetPwd:
                            cmdToSend = await ResetPwd(receivedCmd);
                            break;

                        case Command.Type.ChangeNickname:
                            cmdToSend = await ChangeNickname(receivedCmd);
                            break;

                        case Command.Type.ChangeEmail:
                            cmdToSend = await ChangeEmail(receivedCmd);
                            break;

                        case Command.Type.ChangePassword:
                            cmdToSend = await ChangePassword(receivedCmd);
                            break;

                        case Command.Type.GameConfig:
                            cmdToSend = new(Command.Type.GameConfig, ToJson(ConfigManager.Instance.GameConfig));
                            break;

                        case Command.Type.ServerConfig:
                            cmdToSend = new(Command.Type.ServerConfig, ToJson(ConfigManager.Instance.ServerConfig));
                            break;

                        case Command.Type.DatabaseConfig:
                            cmdToSend = new(Command.Type.DatabaseConfig, ToJson(ConfigManager.Instance.DatabaseConfig));
                            break;

                        case Command.Type.UpdateEquip:
                            cmdToSend = await UpdateEquip(receivedCmd);
                            break;

                        case Command.Type.UpdateSkill:
                            cmdToSend = await UpdateSkill(receivedCmd);
                            break;

                        case Command.Type.UpdateMonster:
                            cmdToSend = await UpdateMonster(receivedCmd);
                            break;

                        case Command.Type.UploadSave:    
                            cmdToSend = await UploadSave(receivedCmd);
                            break;

                        case Command.Type.DownloadSave:
                            cmdToSend = await DownloadSave(receivedCmd);
                            break;

                        case Command.Type.UploadScore:
                            cmdToSend = await UploadScore(receivedCmd);
                            break;

                        case Command.Type.PersonalScores:
                            cmdToSend = await GetUserScores(receivedCmd);
                            break;

                        case Command.Type.MonthlyScores:
                            cmdToSend = await GetMonthlyScores(receivedCmd);
                            break;

                        case Command.Type.AllTimeScores:
                            cmdToSend = await GetAllTimeScores(receivedCmd);
                            break;

                        case Command.Type.Disconnect:
                            cmdToSend = Disconnect(receivedCmd);
                            await stream.WriteAsync(Encode.ToBytes(ToJson(cmdToSend)), token);
                            return;

                        default:
                            cmdToSend = Helper.ErrorCmd(this, new(Command.Type.Error, null), "Received invalid cmd");
                            break;
                    }

                    if (cmdToSend.CommandType != Command.Type.Empty)
                    {
                        await stream.WriteAsync(Encode.ToBytes(ToJson(cmdToSend)), token);
                        cmdToSend.Set(Command.Type.Empty, null);
                    }
                }
            }
            catch(OperationCanceledException) {}
            catch(Exception ex)
            {
                LogHandler.AddLog($"Error while handling client: {ex}", this);
            }
            finally
            {
                Server.RemoveClient(this);
                stream.Close();
                client.Close();
            }
        }

        private async Task<Command> CheckUsername(Command cmd)
        {
            var (success, errorMessage) = await UserDB.CheckUsername(cmd.Payload);

            if (!success)
                return Helper.ErrorCmd(this, cmd, errorMessage, false);

            return new(cmd.CommandType);
        }

        private async Task<Command> Register(Command cmd)
        {
            User? registeredUser = FromJson<User>(cmd.Payload);

            if (registeredUser == null)
                return Helper.ErrorCmd(this, cmd, "Invalid registering user");

            var (success, errorMessage) = await UserDB.Add(registeredUser);

            if (!success)
                return Helper.ErrorCmd(this, cmd, errorMessage);

            LogHandler.AddLog($"Registered with Username '{registeredUser.Username}'", this);
            return new(cmd.CommandType);
        }

        private async Task<(Command cmdToSend, User? requestedUser)> GetUserPwd(Command cmd)
        {
            var (requestedUser, errorMessage) = await UserDB.Get(cmd.Payload);

            if (requestedUser == null)
                return (Helper.ErrorCmd(this, cmd, errorMessage, false), null);

            if (requestedUser.PwdSet == null)
                return (Helper.ErrorCmd(this, cmd, "No password found"), null);

            return (new(cmd.CommandType, ToJson(requestedUser.PwdSet)), requestedUser);
        }

        private Command Login(Command cmd, User? tempUser)
        {
            if (user != null || tempUser == null)
                return Helper.ErrorCmd(this, cmd, "Invalid user/tempUser");

            user = tempUser;
            LogHandler.AddLog($"Logged in as {user}", endPoint);
            return new(cmd.CommandType, ToJson(user));
        }

        private Command Logout(Command cmd)
        {
            if (user == null || user.UserID < 1)
                return Helper.ErrorCmd(this, cmd, "Invalid user");

            LogHandler.AddLog($"Logged out from {user}", endPoint);
            user = null;
            return new(cmd.CommandType);
        }

        private async Task<Command> ValidateEmail(Command cmd)
        {
            User? requestedUser = FromJson<User>(cmd.Payload);

            if (requestedUser == null)
                return Helper.ErrorCmd(this, cmd, "Invalid user data", false);

            var (dbUser, errorMessage) = await UserDB.Get(requestedUser.Username);

            if (dbUser == null)
                return Helper.ErrorCmd(this, cmd, errorMessage, false);

            if (dbUser.Email != requestedUser.Email)
                return Helper.ErrorCmd(this, cmd, "Invalid email", false);

            return new(cmd.CommandType);
        }

        private async Task<Command> ResetPwd(Command cmd)
        {
            User? requestedUser = FromJson<User>(cmd.Payload);

            if (requestedUser == null)
                return Helper.ErrorCmd(this, cmd, "Invalid user data", false);

            var (success, updateErrorMessage) = await UserDB.Update(requestedUser.Username, null, null, requestedUser.PwdSet);

            if (!success)
                return Helper.ErrorCmd(this, cmd, updateErrorMessage);

            LogHandler.AddLog($"Reset password of {requestedUser}", this);
            return new(cmd.CommandType);
        }

        private async Task<Command> ChangeNickname(Command cmd)
        {
            if (user == null || user.UserID < 1)
                return Helper.ErrorCmd(this, cmd, "Invalid user");

            string newNickname = cmd.Payload;

            var (success, errorMessage) = await UserDB.Update(user.UserID, newNickname, null, null);

            if (!success)
                return Helper.ErrorCmd(this, cmd, errorMessage);
            
            LogHandler.AddLog($"Changed nickname of {user} from '{user.Nickname}' to '{newNickname}'", this);
            user.Nickname = newNickname;
            return new(cmd.CommandType);
        }

        private async Task<Command> ChangeEmail(Command cmd)
        {
            if (user == null || user.UserID < 1)
                return Helper.ErrorCmd(this, cmd, "Invalid user");

            string newEmail = cmd.Payload;

            var (success, errorMessage) = await UserDB.Update(user.UserID, newEmail, null, null);

            if (!success)
                return Helper.ErrorCmd(this, cmd, errorMessage);
            
            LogHandler.AddLog($"Changed email of {user} from '{user.Email}' to '{newEmail}'", this);
            user.Nickname = newEmail;
            return new(cmd.CommandType);
        }

        private async Task<Command> ChangePassword(Command cmd)
        {
            if (user == null || user.UserID < 1)
                return Helper.ErrorCmd(this, cmd, "Invalid user");

            var newPwd = FromJson<PasswordSet>(cmd.Payload);

            var (success, errorMessage) = await UserDB.Update(user.UserID, null, null, newPwd);

            if (!success)
                return Helper.ErrorCmd(this, cmd, errorMessage);
            
            LogHandler.AddLog($"Changed password of {user}", this);
            user.PwdSet = newPwd;
            return new(cmd.CommandType);
        }

        private async Task<Command> UpdateEquip(Command cmd)
        {
            var (equipments, errorMessage) = await EquipmentDB.GetAll();

            if (equipments == null)
                return Helper.ErrorCmd(this, cmd, errorMessage);

            return new(cmd.CommandType, JsonSerializer.Serialize(equipments));
        }

        private async Task<Command> UpdateSkill(Command cmd)
        {
            var (skills, errorMessage) = await SkillDB.GetAll();

            if (skills == null)
                return Helper.ErrorCmd(this, cmd, errorMessage);

            return new(cmd.CommandType, JsonSerializer.Serialize(skills));
        }

        private async Task<Command> UpdateMonster(Command cmd)
        {
            var (monsters, errorMessage) = await MonsterDB.GetAll(ConfigManager.Instance.GameConfig.ProgressMaxFloor);

            if (monsters == null)
                return Helper.ErrorCmd(this, cmd, errorMessage);

            return new(cmd.CommandType, JsonSerializer.Serialize(monsters));
        }

        private async Task<Command> UploadSave(Command cmd)
        {
            if (user == null || user.UserID < 1)
                return Helper.ErrorCmd(this, cmd, "Invalid user");

            // GameSave? gameSave = FromJson<GameSave>(cmd.Payload);

            // if (gameSave == null)
            //     return Helper.ErrorCmd(this, cmd, "Invalid game save");

            // gameSave.Name = $"{user.Username}Save";

            var (success, errorMessage) = await GameSaveDB.Save(user.UserID, cmd.Payload);

            if (!success)
                return Helper.ErrorCmd(this, cmd, errorMessage);

            LogHandler.AddLog($"Uploaded save", this);
            return new(cmd.CommandType);
        }

        private async Task<Command> DownloadSave(Command cmd)
        {
            if (user == null || user.UserID < 1)
                return Helper.ErrorCmd(this, cmd, "Invalid user");

            var (saveContent, errorMessage) = await GameSaveDB.Load(user.UserID);

            if (saveContent == null)
                return Helper.ErrorCmd(this, cmd, errorMessage, false);

            return new(cmd.CommandType, saveContent);
        }

        private async Task<Command> UploadScore(Command cmd)
        {
            if (user == null || user.UserID < 1)
                return Helper.ErrorCmd(this, cmd, "Invalid user");

            Score? score = FromJson<Score>(cmd.Payload);

            if (score == null)
                return Helper.ErrorCmd(this, cmd, "Invalid score");

            var (success, errorMessage) = await ScoreDB.Add(score);

            if (!success)
                return Helper.ErrorCmd(this, cmd, errorMessage);

            LogHandler.AddLog($"Uploaded score", this);
            return new(cmd.CommandType);
        }

        private async Task<Command> GetUserScores(Command cmd)
        {
            if (user == null || user.UserID < 1)
                return Helper.ErrorCmd(this, cmd, "Invalid user");

            var (personal, errorMessage) = await ScoreDB.GetPersonal(user.UserID);

            if (personal == null)
                return Helper.ErrorCmd(this, cmd, errorMessage);

            return new(cmd.CommandType, JsonSerializer.Serialize(personal));
        }

        private async Task<Command> GetMonthlyScores(Command cmd)
        {
            if (user == null || user.UserID < 1)
                return Helper.ErrorCmd(this, cmd, "Invalid user");

            var (monthly, errorMessage) = await ScoreDB.GetMonthly();

            if (monthly == null)
                return Helper.ErrorCmd(this, cmd, errorMessage);

            return new(cmd.CommandType, JsonSerializer.Serialize(monthly));
        }

        private async Task<Command> GetAllTimeScores(Command cmd)
        {
            if (user == null || user.UserID < 1)
                return Helper.ErrorCmd(this, cmd, "Invalid user");

            var (alltime, errorMessage) = await ScoreDB.GetAllTime();

            if (alltime == null)
                return Helper.ErrorCmd(this, cmd, errorMessage);

            return new(cmd.CommandType, JsonSerializer.Serialize(alltime));
        }

        private Command Disconnect(Command cmd)
        {
            LogHandler.AddLog($"Disconnected", this);
            user = null;
            return new(cmd.CommandType);
        }

        private class Helper
        {
            /// <summary>
            /// Set error command & log error based on 'errorMessage'
            /// </summary>
            public static Command ErrorCmd(ClientHandler client, Command sourceCmd, string errorDetail, bool addLog = true)
            {
                string logContent = $"Error: (Cmd: {sourceCmd.Name()}) (Detail: {errorDetail})";

                if (addLog) LogHandler.AddLog(logContent, client);
                return new(Command.Type.Error, errorDetail);
            }
        }
    }
}