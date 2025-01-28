using System.Data;
using MySql.Data.MySqlClient;

namespace DAL.DBHandlers
{
    public static class GameSaveDB
    {
        public static async Task<(bool success, string error)> Save(int userID, string saveContent)
        {
            string query = @"
                INSERT INTO GameSaves (UserID, SaveContent)
                VALUES (@userID, @content)
                ON DUPLICATE KEY UPDATE SaveContent = @content;
            ";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@userID", userID);
                cmd.Parameters.AddWithValue("@content", saveContent);

                await cmd.ExecuteNonQueryAsync();
                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(string? saveContent, string error)> Load(int userID)
        {
            string query = @"
                SELECT SaveContent
                FROM GameSaves
                WHERE UserID = @userID;
            ";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@userID", userID);

                using var reader = await cmd.ExecuteReaderAsync();
                
                if (!reader.HasRows)
                    return (null, "No data found");

                await reader.ReadAsync();
                return (reader.GetString("SaveContent"), "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }
    }
}