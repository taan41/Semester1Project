using System.Text.Json;
using BLL.GameComponents.EntityComponents;
using BLL.GameComponents.ItemComponents;
using BLL.GameComponents.Others;
using BLL.GameHelpers;
using DAL;
using DAL.ConfigClasses;
using NetworkLL;
using NetworkLL.DataTransferObjects;

namespace BLL.GameHandlers
{
    public class ServerHandler
    {
        public static bool IsConnected => NetworkHandler.IsConnected;
        public static bool IsLoggedIn => IsConnected && mainUser != null;
        public static string Username => mainUser?.Username ?? "Guest";
        public static string Nickname => mainUser?.Nickname ?? "Guest";
        public static string Email => mainUser?.Email ?? "Guest";

        private static NetworkHandler NetworkHandler => NetworkHandler.Instance;
        private static User? mainUser;

        public static bool Connect(out string error)
        {
            if (!NetworkHandler.Connect())
            {
                error = "Connection failed";
                return false;
            }

            error = "";
            return true;
        }

        public static void Close()
        {
            NetworkHandler.Close();
        }

        public static bool UpdateConfig(out string error)
        {
            if (!NetworkHandler.Communicate(new(Command.Type.GameConfig), out string result))
            {
                error = result;
                return false;
            }

            var gameConfig = JsonSerializer.Deserialize<GameConfig>(result);
            if (gameConfig != null)
            {
                FileManager.WriteJson(FileManager.FolderNames.Configs, FileManager.FileNames.GameConfig, gameConfig);
            }

            if (!NetworkHandler.Communicate(new(Command.Type.DatabaseConfig), out result))
            {
                error = result;
                return false;
            }

            var dbConfig = JsonSerializer.Deserialize<DatabaseConfig>(result);
            if (dbConfig != null)
            {
                FileManager.WriteJson(FileManager.FolderNames.Configs, FileManager.FileNames.DatabaseConfig, dbConfig);
            }

            ConfigManager.Instance.LoadConfig();

            error = "";
            return true;
        }

        public static bool UpdateAssets(out string error)
        {
            if (!NetworkHandler.Communicate(new(Command.Type.UpdateEquip), out string result))
            {
                error = result;
                return false;
            }

            var equipments = JsonSerializer.Deserialize<Dictionary<int, Equipment>>(result);
            if (equipments != null)
            {
                FileManager.WriteJson(FileManager.FolderNames.Assets, FileManager.FileNames.Equips, equipments);
            }

            if (!NetworkHandler.Communicate(new(Command.Type.UpdateSkill), out result))
            {
                error = result;
                return false;
            }

            var skills = JsonSerializer.Deserialize<Dictionary<int, Skill>>(result);
            if (skills != null)
            {
                FileManager.WriteJson(FileManager.FolderNames.Assets, FileManager.FileNames.Skills, skills);
            }

            if (!NetworkHandler.Communicate(new(Command.Type.UpdateMonster), out result))
            {
                error = result;
                return false;
            }

            var monsters = JsonSerializer.Deserialize<Dictionary<int, Monster>>(result);
            if (monsters != null)
            {
                FileManager.WriteJson(FileManager.FolderNames.Assets, FileManager.FileNames.Monsters, monsters);
            }

            AssetLoader.LoadAsset();

            error = "";
            return true;
        }
        
        public static bool CheckUsername(string username, out string error)
        {
            if (!NetworkHandler.Communicate(new(Command.Type.CheckUsername, username), out string result))
            {
                error = result;
                return false;
            }

            error = "";
            return true;
        }

        public static bool Register(string username, string nickname, string password, string email, out string error)
        {
            User registeredUser = new(username, nickname, password, email);

            if (!NetworkHandler.Communicate(new(Command.Type.Register, registeredUser.ToJson()), out string result))
            {
                error = result;
                return false;
            }

            error = "";
            return true;
        }

        public static bool ResetPassword(string username, string email, string password, out string error)
        {
            User userToReset = new() {Username = username, Email = email};
            if (!NetworkHandler.Communicate(new(Command.Type.ValidateEmail, userToReset.ToJson()), out string result))
            {
                error = result;
                return false;
            }

            userToReset.PwdSet = new(password);

            if (!NetworkHandler.Communicate(new(Command.Type.ResetPwd, userToReset.ToJson()), out result))
            {
                error = result;
                return false;
            }

            error = "";
            return true;
        }

        public static bool Login(string username, string password, out string error)
        {
            if (!NetworkHandler.Communicate(new(Command.Type.GetUserPwd, username), out string result))
            {
                error = result;
                return false;
            }

            PasswordSet? pwdSet = PasswordSet.FromJson(result);
            if (pwdSet == null || !Utilities.Security.VerifyPassword(password, pwdSet))
            {
                error = "Invalid password";
                return false;
            }

            if (!NetworkHandler.Communicate(new(Command.Type.Login, username), out result))
            {
                error = result;
                return false;
            }

            mainUser = User.FromJson(result);
            if (mainUser == null)
            {
                error = "Invalid user";
                return false;
            }

            error = "";
            return true;
        }

        public static bool ValidatePassword(string oldPasswod)
        {
            return mainUser != null && mainUser.PwdSet != null && Utilities.Security.VerifyPassword(oldPasswod, mainUser.PwdSet);
        }

        public static bool ChangePassword(string newPassword, out string error)
        {
            if (mainUser == null)
            {
                error = "Not logged in";
                return false;
            }

            PasswordSet newPwdSet = new(newPassword);
            if (!NetworkHandler.Communicate(new(Command.Type.ChangePassword, newPwdSet.ToJson()), out string result))
            {
                error = result;
                return false;
            }

            error = "";
            return true;
        }

        public static bool ChangeNickname(string newNickname, out string error)
        {
            if (mainUser == null)
            {
                error = "Not logged in";
                return false;
            }

            if (!NetworkHandler.Communicate(new(Command.Type.ChangeNickname, newNickname), out string result))
            {
                error = result;
                return false;
            }

            mainUser.Nickname = newNickname;
            error = "";
            return true;
        }

        public static bool ChangeEmail(string newEmail, out string error)
        {
            if (mainUser == null)
            {
                error = "Not logged in";
                return false;
            }

            if (!NetworkHandler.Communicate(new(Command.Type.ChangeEmail, newEmail), out string result))
            {
                error = result;
                return false;
            }

            mainUser.Email = newEmail;
            error = "";
            return true;
        }

        public static bool DeleteAccount(out string error)
        {
            if (mainUser == null)
            {
                error = "Not logged in";
                return false;
            }

            if (!NetworkHandler.Communicate(new(Command.Type.DeleteAccount), out string result))
            {
                error = result;
                return false;
            }

            mainUser = null;
            error = "";
            return true;
        }

        public static void DownloadSave()
        {
            if (!NetworkHandler.Communicate(new(Command.Type.DownloadSave), out string result))
                return;

            GameSave.FromJson(result)?.SaveAs("CloudSave");
        }

        public static bool UploadSave(GameSave save, out string error)
        {
            if (!NetworkHandler.Communicate(new(Command.Type.UploadSave, save.ToJson()), out string result))
            {
                error = result;
                return false;
            }

            error = "";
            return true;
        }

        public static bool UploadScore(TimeSpan clearTime, out string error)
        {
            if (!IsLoggedIn)
            {
                error = "Not logged in";
                return false;
            }

            Score score = new(mainUser!.UserID, mainUser.Nickname, clearTime);
            if (!NetworkHandler.Communicate(new(Command.Type.UploadScore, score.ToJson()), out string result))
            {
                error = result;
                return false;
            }

            error = "";
            return true;
        }

        public static bool GetScores(out List<string> personal, out List<string> monthly, out List<string> allTime, out string error)
        {
            error = "";

            personal = PersonalScores(out string personalError);
            if (personalError != "")
                error = personalError;
            
            monthly = MonthlyScores(out string monthlyError);
            if (monthlyError != "")
                error = monthlyError;
            
            allTime = AllTimeScores(out string allTimeError);
            if (allTimeError != "")
                error = allTimeError;

            return error == "";
        }

        public static List<string> PersonalScores(out string error)
        {
            if (!NetworkHandler.Communicate(new(Command.Type.PersonalScores), out string result))
            {
                error = result;
                return [];
            }

            error = "";
            List<Score> scores = JsonSerializer.Deserialize<List<Score>>(result) ?? [];
            return scores.ConvertAll(score => score.ToString());
        }

        public static List<string> MonthlyScores(out string error)
        {
            if (!NetworkHandler.Communicate(new(Command.Type.MonthlyScores), out string result))
            {
                error = result;
                return [];
            }

            error = "";
            List<Score> scores = JsonSerializer.Deserialize<List<Score>>(result) ?? [];
            return scores.ConvertAll(score => score.ToString());
        }

        public static List<string> AllTimeScores(out string error)
        {
            if (!NetworkHandler.Communicate(new(Command.Type.AllTimeScores), out string result))
            {
                error = result;
                return [];
            }

            error = "";
            List<Score> scores = JsonSerializer.Deserialize<List<Score>>(result) ?? [];
            return scores.ConvertAll(score => score.ToString());
        }

        public static bool Logout(out string error)
        {
            if (!NetworkHandler.Communicate(new(Command.Type.Logout), out string result))
            {
                error = result;
                return false;
            }

            mainUser = null;
            error = "";
            return true;
        }
    }
}