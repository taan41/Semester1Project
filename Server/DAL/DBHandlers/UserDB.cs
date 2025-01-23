using System.Data;
using System.Text;
using DAL.Persistence.DataTransferObjects;
using MySql.Data.MySqlClient;

namespace DAL.DBHandlers
{
    public static class UserDB
    {
        public static async Task<(bool success, string errorMessage)> CheckUsername(string username)
        {
            string query = "SELECT * FROM Users WHERE Username = @username";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
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
            string query = "INSERT INTO Users (Username, Nickname, Email, PasswordHash, Salt) VALUES (@username, @nickname, @email, @pwdHash, @salt)";

            if (userToAdd.PwdSet == null)
                return (false, "Null user password");

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@username", userToAdd.Username);
                cmd.Parameters.AddWithValue("@nickname", userToAdd.Nickname);
                cmd.Parameters.AddWithValue("@email", userToAdd.Email);
                cmd.Parameters.AddWithValue("@pwdHash", userToAdd.PwdSet.PwdHash);
                cmd.Parameters.AddWithValue("@salt", userToAdd.PwdSet.PwdSalt);

                await cmd.ExecuteNonQueryAsync();

                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(User? requestedUser, string errorMessage)> Get(string username, bool getPwd = true)
        {
            string query = getPwd ?
                "SELECT UserID, Username, Nickname, PasswordHash, Salt, Email FROM Users WHERE Username = @username" :
                "SELECT UserID, Username, Nickname, Email FROM Users WHERE Username = @username";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync()) // User not found
                    return (null, $"No user with username '{username}' found");

                PasswordSet? pwdSet = null;

                if (getPwd)
                {
                    byte[] pwdHash = new byte[32];
                    byte[] salt = new byte[16];

                    reader.GetBytes("PasswordHash", 0, pwdHash, 0, 32);
                    reader.GetBytes("Salt", 0, salt, 0, 16);

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
        
        public static async Task<(User? requestedUser, string errorMessage)> Get(int userID, bool getPwd = true)
        {
            string query = getPwd ?
                "SELECT UserID, Username, Nickname, PasswordHash, Salt, Email FROM Users WHERE UserID = @userID" :
                "SELECT UserID, Username, Nickname, Email FROM Users WHERE UserID = @userID";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@userID", userID);

                using var reader = await cmd.ExecuteReaderAsync();

                if (! await reader.ReadAsync()) // User not found
                    return (null, $"No user with ID '{userID}' found");

                PasswordSet? pwdSet = null;

                if (getPwd)
                {
                    byte[] pwdHash = new byte[32];
                    byte[] salt = new byte[16];

                    reader.GetBytes("PasswordHash", 0, pwdHash, 0, 32);
                    reader.GetBytes("Salt", 0, salt, 0, 16);

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
                using MySqlConnection conn = new(DBManager.ConnectionString);
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
                    cmd.Parameters.AddWithValue("@newSalt", newPwd.PwdSalt);
                }

                await cmd.ExecuteNonQueryAsync();

                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(bool success, string errorMessage)> Update(string username, string? newNickname, string? newEmail, PasswordSet? newPwd)
        {
            if (newNickname == null && newEmail == null && newPwd == null)
                return (false, "Invalid updated data");

            List<string> querySets = [];
            if (newNickname != null) querySets.Add(" Nickname = @newNickname");
            if (newEmail != null) querySets.Add(" Email = @newEmail");
            if (newPwd != null) querySets.Add(" PasswordHash = @newPwdHash, Salt = @newSalt");

            StringBuilder query = new("UPDATE Users SET");
            query.AppendJoin(',', querySets);
            query.Append(" WHERE Username = @username");

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@username", username);

                if (newNickname != null)
                    cmd.Parameters.AddWithValue("@newNickname", newNickname);
                
                if (newEmail != null)
                    cmd.Parameters.AddWithValue("@newEmail", newEmail);

                if (newPwd != null)
                {
                    cmd.Parameters.AddWithValue("@newPwdHash", newPwd.PwdHash);
                    cmd.Parameters.AddWithValue("@newSalt", newPwd.PwdSalt);
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
}