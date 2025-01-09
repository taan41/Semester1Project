using System.Data;
using MySql.Data.MySqlClient;

static class GameDataDB
{
    public static async Task<(bool success, string errorMessage)> Save(int userID, GameData data)
    {
        string query = @"
            DELETE FROM GameSaves
            WHERE UserID = @userID;
            INSERT INTO GameSaves (UserID, Data)
            VALUES (@userID, @data);
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@userID", userID);
            cmd.Parameters.AddWithValue("@data", data.ToJson());

            await cmd.ExecuteNonQueryAsync();
            return (true, "");
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(GameData? data, string errorMessage)> Load(int userID)
    {
        string query = @"
            SELECT Data
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
            return (GameData.FromJson(reader.GetString("Data")), "");
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