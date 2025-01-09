using System.Data;
using MySql.Data.MySqlClient;

static class GameSaveDB
{
    public static async Task<(bool success, string errorMessage)> Save(int userID, GameSave save)
    {
        string query = @"
            DELETE FROM GameSaves
            WHERE UserID = @userID;
            INSERT INTO GameSaves (UserID, SaveData)
            VALUES (@userID, @data);
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@userID", userID);
            cmd.Parameters.AddWithValue("@data", save.ToJson());

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
            SELECT SaveData
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
            return (GameSave.FromJson(reader.GetString("SaveData")), "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    }

    public static async Task<(bool success, string errorMessage)> Clear(int userID)
    {
        string query = @"
            DELETE FROM GameSaves
            WHERE UserID = @userID;
        ";

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
}