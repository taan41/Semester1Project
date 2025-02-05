using System.Data;
using System.Text;
using DAL.Persistence.DataModels;
using MySql.Data.MySqlClient;

namespace DAL.DBHandlers
{
    public static class UserDB
    {
        public static async Task<(bool success, string error)> CheckUsername(string username)
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

        public static async Task<(bool success, string error)> Add(User? userToAdd)
        {
            string query = "INSERT INTO Users (Username, Nickname, Email, PasswordHash, Salt) VALUES (@username, @nickname, @email, @pwdHash, @salt)";

            if (userToAdd == null || userToAdd.PwdSet == null)
                return (false, "Invalid adding user data");

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

        public static async Task<(User? requestedUser, string error)> Get(string username, bool getPwd = true)
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
        
        public static async Task<(User? requestedUser, string error)> Get(int userID, bool getPwd = true)
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

        public static async Task<(bool success, string error)> Update(User? updatingUser)
        {
            if (updatingUser == null || updatingUser.UserID < 1)
                return (false, "Invalid updating user");

            List<string> querySets = [];
            if (updatingUser.Nickname != null) querySets.Add(" Nickname = @newNickname");
            if (updatingUser.Email != null) querySets.Add(" Email = @newEmail");
            if (updatingUser.PwdSet != null) querySets.Add(" PasswordHash = @newPwdHash, Salt = @newSalt");

            StringBuilder query = new("UPDATE Users SET");
            query.AppendJoin(',', querySets);
            query.Append(" WHERE UserID = @userID");

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@userID", updatingUser.UserID);

                if (updatingUser.Nickname != null)
                    cmd.Parameters.AddWithValue("@newNickname", updatingUser.Nickname);
                
                if (updatingUser.Email != null)
                    cmd.Parameters.AddWithValue("@newEmail", updatingUser.Email);

                if (updatingUser.PwdSet != null)
                {
                    cmd.Parameters.AddWithValue("@newPwdHash", updatingUser.PwdSet.PwdHash);
                    cmd.Parameters.AddWithValue("@newSalt", updatingUser.PwdSet.PwdSalt);
                }

                await cmd.ExecuteNonQueryAsync();

                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(bool success, string error)> Delete(int userID)
        {
            if (userID < 1)
                return (false, "Invalid user ID");

            string query = "DELETE FROM Users WHERE UserID = @userID";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@userID", userID);

                await cmd.ExecuteNonQueryAsync();

                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(List<User>? users, string error)> GetAll()
        {
            string query = "SELECT UserID, Username, Nickname, Email FROM Users";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);

                using var reader = await cmd.ExecuteReaderAsync();

                List<User> users = [];

                while (await reader.ReadAsync())
                {
                    users.Add(new()
                    {
                        UserID = reader.GetInt32("UserID"),
                        Username = reader.GetString("Username"),
                        Nickname = reader.GetString("Nickname"),
                        Email = reader.GetString("Email")
                    });
                }

                return (users, "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }
    }
}