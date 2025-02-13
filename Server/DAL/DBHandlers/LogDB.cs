using System.Data;
using MySql.Data.MySqlClient;
using DAL.DataModels;

namespace DAL.DBHandlers
{
    public static class LogDB
    {
        private static DBManager DBManager => DBManager.Instance;
        
        public static async Task<(bool success, string error)> Add(string? source, string logContent)
        {
            string query = @"
                INSERT INTO ActivityLog (Source, Content) 
                VALUES (@source, @content);
            ";

            try
            {    
                using MySqlConnection conn = new(DBManager.ConnectionString);
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
        
        public static async Task<(List<Log>? requestedLogList, string error)> GetAll()
        {
            string query = "SELECT LogTime, Source, Content FROM ActivityLog ORDER BY LogTime";

            List<Log> logList = [];
            
            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
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

        public static async Task<(bool success, string error)> Clear()
        {
            string query = @"
                DELETE FROM ActivityLog;
                ALTER TABLE ActivityLog AUTO_INCREMENT = 1;
            ";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
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