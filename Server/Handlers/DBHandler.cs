using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

static class DBHandler
{
    private static string connectionString = "";

    public static bool Initialize(string server, string db, string uid, string password, out string errorMessage)
    {
        connectionString = $"server={server};uid={uid};pwd={password};";
        errorMessage = "";

        try
        {
            using MySqlConnection conn = new(connectionString);
            conn.Open();

            using MySqlCommand cmd = new($"SHOW DATABASES LIKE '{db}';", conn);

            using MySqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows) // Database does not exist
            {
                reader.Close();
                CreateDB(conn, db);
            }

            connectionString += $"database={db};";
            return true;
        }
        catch(MySqlException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
    
    private static void CreateDB(MySqlConnection conn, string dbName)
    {
        using (MySqlCommand createDB = new(@$"
            CREATE DATABASE {dbName}
            CHARACTER SET = utf8mb4
            COLLATE = utf8mb4_unicode_ci;
            ", conn))
        {
            createDB.ExecuteNonQuery();
        }

        conn.ChangeDatabase(dbName);

        using (MySqlCommand createUsers = new(@$"
            CREATE TABLE Users (
                UserID INT AUTO_INCREMENT PRIMARY KEY,
                Username VARCHAR({MagicNum.usernameMax}) NOT NULL UNIQUE,
                Nickname VARCHAR({MagicNum.nicknameMax}) NOT NULL,
                PasswordHash VARBINARY({MagicNum.pwdHashLen}) DEFAULT NULL,
                Salt VARBINARY({MagicNum.pwdSaltLen}) DEFAULT NULL,
                Email VARCHAR(255) NOT NULL,
                INDEX idx_userID (UserID),
                INDEX idx_username (Username)
            )", conn))
        {
            createUsers.ExecuteNonQuery();
        }

        using (MySqlCommand createScores = new(@"
            CREATE TABLE Highscores (
                ScoreID INT AUTO_INCREMENT PRIMARY KEY,
                UserID INT NOT NULL,
                TimeScore TIME(3) NOT NULL,
                UploadedTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (UserID) REFERENCES Users(UserID) ON UPDATE CASCADE ON DELETE CASCADE
            )", conn))
        {
            createScores.ExecuteNonQuery();
        }

        using (MySqlCommand createLog = new(@"
            CREATE TABLE ActivityLog (
                LogIndex INT AUTO_INCREMENT PRIMARY KEY,
                LogTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                Source VARCHAR(255) NOT NULL,
                Content TEXT NOT NULL
            )", conn))
        {
            createLog.ExecuteNonQuery();
        }
    }

    public class UserDB
    {
        public static async Task<(bool success, string errorMessage)> CheckUsername(string username)
        {
            string query = "SELECT * FROM Users WHERE Username = @username";

            try
            {
                using MySqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows) // No same username found
                    return (true, "");
                else
                    return (false, "Unavailable username");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(bool success, string errorMessage)> Add(User userToAdd)
        {
            string query = "INSERT INTO Users (Username, Nickname, PasswordHash, Salt, Email) VALUES (@username, @nickname, @pwdHash, @salt, @email)";

            if (userToAdd.PwdSet == null)
                return (false, "Null user password");

            try
            {
                using MySqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@username", userToAdd.Username);
                cmd.Parameters.AddWithValue("@nickname", userToAdd.Nickname);
                cmd.Parameters.AddWithValue("@pwdHash", userToAdd.PwdSet.PwdHash);
                cmd.Parameters.AddWithValue("@salt", userToAdd.PwdSet.Salt);
                cmd.Parameters.AddWithValue("@email", userToAdd.Email);

                await cmd.ExecuteNonQueryAsync();

                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(User? requestedUser, string errorMessage)> Get(string username, bool getPwd)
        {
            string query = getPwd ?
                "SELECT UserID, Username, Nickname, PasswordHash, Salt, Email FROM Users WHERE Username = @username" :
                "SELECT UserID, Username, Nickname, Email FROM Users WHERE Username = @username";;

            try
            {
                using MySqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                using var reader = await cmd.ExecuteReaderAsync();

                if (! await reader.ReadAsync()) // User not found
                    return (null, $"No user with username '{username}' found");

                PasswordSet? pwdSet = null;

                if (getPwd)
                {
                    byte[] pwdHash = new byte[MagicNum.pwdHashLen];
                    byte[] salt = new byte[MagicNum.pwdSaltLen];

                    reader.GetBytes("PasswordHash", 0, pwdHash, 0, MagicNum.pwdHashLen);
                    reader.GetBytes("Salt", 0, salt, 0, MagicNum.pwdSaltLen);

                    pwdSet = new(pwdHash, salt);
                }
                
                return (new()
                {
                    UserID = reader.GetInt32("UserID"),
                    Username = reader.GetString("Username"),
                    Nickname = reader.GetString("Nickname"),
                    PwdSet = pwdSet,
                    Email = reader.GetString("Email")
                }, "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }
        
        public static async Task<(User? requestedUser, string errorMessage)> Get(int userID, bool getPwd)
        {
            string query = getPwd ?
                "SELECT UserID, Username, Nickname, PasswordHash, Salt, Email FROM Users WHERE UserID = @userID" :
                "SELECT UserID, Username, Nickname, Email FROM Users WHERE UserID = @userID";

            try
            {
                using MySqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@userID", userID);

                using var reader = await cmd.ExecuteReaderAsync();

                if (! await reader.ReadAsync()) // User not found
                    return (null, $"No user with ID '{userID}' found");

                PasswordSet? pwdSet = null;

                if (getPwd)
                {
                    byte[] pwdHash = new byte[MagicNum.pwdHashLen];
                    byte[] salt = new byte[MagicNum.pwdSaltLen];

                    reader.GetBytes("PasswordHash", 0, pwdHash, 0, MagicNum.pwdHashLen);
                    reader.GetBytes("Salt", 0, salt, 0, MagicNum.pwdSaltLen);

                    pwdSet = new(pwdHash, salt);
                }
                
                return (new()
                {
                    UserID = reader.GetInt32("UserID"),
                    Username = reader.GetString("Username"),
                    Nickname = reader.GetString("Nickname"),
                    PwdSet = pwdSet,
                    Email = reader.GetString("Email")
                }, "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }

        public static async Task<(bool success, string errorMessage)> Update(int userID, string? newNickname, string? newEmail, PasswordSet? newPwd)
        {
            if (newNickname == null && newEmail == null && newPwd == null)
                return (false, "Invalid updated data");
                
            if (userID < 1)
                return (false, "Invalid user ID");

            List<string> querySets = [];
            if (newNickname != null) querySets.Add(" Nickname = @newNickname");
            if (newEmail != null) querySets.Add(" Email = @newEmail");
            if (newPwd != null) querySets.Add(" PasswordHash = @newPwdHash, Salt = @newSalt");

            StringBuilder query = new("UPDATE Users SET");
            query.AppendJoin(',', querySets);
            query.Append(" WHERE UserID = @userID");

            try
            {
                using MySqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@userID", userID);

                if (newNickname != null)
                    cmd.Parameters.AddWithValue("@newNickname", newNickname);
                
                if (newEmail != null)
                    cmd.Parameters.AddWithValue("@newEmail", newEmail);

                if (newPwd != null)
                {
                    cmd.Parameters.AddWithValue("@newPwdHash", newPwd.PwdHash);
                    cmd.Parameters.AddWithValue("@newSalt", newPwd.Salt);
                }

                await cmd.ExecuteNonQueryAsync();

                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }
    }

    public class HighscoreDB
    {
        
    }

    public class LogDB
    {
        public static async Task<(bool success, string errorMessage)> Add(string? source, string logContent)
        {
            string query = "INSERT INTO ActivityLog (Source, Content) VALUES (@source, @content)";

            try
            {    
                using MySqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@source", source ?? "null");
                cmd.Parameters.AddWithValue("@content", logContent);

                await cmd.ExecuteNonQueryAsync();
                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }
        
        public static async Task<(List<Log>? requestedLogList, string errorMessage)> GetAll()
        {
            string query = "SELECT LogTime, Source, Content FROM ActivityLog ORDER BY LogTime";

            List<Log> logList = [];
            
            try
            {
                using MySqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    logList.Add(new(reader.GetDateTime("LogTime"), reader.GetString("Source"), reader.GetString("Content")));
                }

                return (logList, "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }

        public static async Task<(bool success, string errorMessage)> Clear()
        {
            string query = @"
                DELETE FROM ActivityLog;
                ALTER TABLE ActivityLog AUTO_INCREMENT = 1;
            ";

            try
            {
                using MySqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                await cmd.ExecuteNonQueryAsync();

                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }
    }
}