using MySql.Data.MySqlClient;

using static DAL.Utitlities;

namespace DAL.DBHandlers
{
    public static class ConfigDB
    {
        public static async Task<(bool success, string errorMessage)> Add(string configName, string configValue)
        {
            string query = @"
                INSERT INTO Configs (ConfigName, ConfigValue)
                VALUES (@name, @value)
                ON DUPLICATE KEY UPDATE
                    ConfigValue = @value;
            ";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@name", configName);
                cmd.Parameters.AddWithValue("@value", configValue);

                await cmd.ExecuteNonQueryAsync();
                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(T? config, string errorMessage)> Get<T>(string configName)
        {
            string query = "SELECT ConfigValue FROM Configs WHERE ConfigName = @name";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@name", configName);

                using var reader = await cmd.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    await reader.ReadAsync();
                    string configValue = reader.GetString(0);
                    return (FromJson<T>(configValue), "");
                }
                else
                {
                    return (default, "Config not found");
                }
            }
            catch (MySqlException ex)
            {
                return (default, ex.Message);
            }
        }
    }
}