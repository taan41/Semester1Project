using System.Data;
using DAL.Persistence.GameComponents.Others;
using MySql.Data.MySqlClient;

using static DAL.Utitlities;

namespace DAL.DBHandlers
{
    public static class GameSaveDB
    {
        public static async Task<(bool success, string errorMessage)> Save(int userID, GameSave save)
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
                cmd.Parameters.AddWithValue("@content", ToJson(save));

                await cmd.ExecuteNonQueryAsync();
                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(GameSave? data, string errorMessage)> Load(int userID)
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
                return (FromJson<GameSave>(reader.GetString("SaveContent")), "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }
    }
}