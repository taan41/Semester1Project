using DAL.Persistence.DataTransferObjects;
using MySql.Data.MySqlClient;

namespace DAL.DBHandlers
{
    public static class ScoreDB
    {
        public static async Task<(bool success, string errorMessage)> Add(Score score)
        {
            string query = "INSERT INTO Scores (UserID, ClearTime, UploadedTime) VALUES (@userID, @clearTime, @uploadedTime)";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@userID", score.UserID);
                cmd.Parameters.AddWithValue("@clearTime", score.ClearTime);
                cmd.Parameters.AddWithValue("@uploadedTime", score.UploadedTime);

                await cmd.ExecuteNonQueryAsync();

                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(List<Score>? scores, string errorMessage)> GetPersonal(int userID)
        {
            string query = @"
                SELECT
                    U.Nickname,
                    S.ClearTime,
                    S.UploadedTime
                FROM 
                    Scores S
                JOIN
                    Users U ON S.UserID = U.UserID AND U.UserID = @userID
                WHERE 
                    S.UserID = @userID
                ORDER BY 
                    S.ClearTime ASC
                LIMIT 20";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@userID", userID);

                List<Score> scores = [];

                using var reader = (MySqlDataReader) await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    scores.Add(new(){
                        UserID = userID,
                        Nickname = reader.GetString("Nickname"),
                        ClearTime = reader.GetTimeSpan("ClearTime"),
                        UploadedTime = reader.GetDateTime("UploadedTime")
                    });

                return (scores, "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }
        
        public static async Task<(List<Score>? scores, string errorMessage)> GetMonthly()
        {
            string query = @"
                SELECT
                    U.Nickname,
                    S.ClearTime,
                    S.UploadedTime
                FROM 
                    Scores S
                JOIN
                    Users U ON S.UserID = U.UserID
                WHERE 
                    MONTH(S.UploadedTime) = MONTH(CURRENT_DATE())
                    AND YEAR(S.UploadedTime) = YEAR(CURRENT_DATE())
                ORDER BY 
                    S.ClearTime ASC
                LIMIT 10";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);

                List<Score> scores = [];

                using var reader = (MySqlDataReader) await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    scores.Add(new(){
                        UserID = -1,
                        Nickname = reader.GetString("Nickname"),
                        ClearTime = reader.GetTimeSpan("ClearTime"),
                        UploadedTime = reader.GetDateTime("UploadedTime")
                    });

                return (scores, "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }
        
        public static async Task<(List<Score>? scores, string errorMessage)> GetAllTime()
        {
            string query = @"
                SELECT
                    U.Nickname,
                    S.ClearTime,
                    S.UploadedTime
                FROM 
                    Scores S
                JOIN
                    Users U ON S.UserID = U.UserID
                ORDER BY 
                    S.ClearTime ASC
                LIMIT 10";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);

                List<Score> scores = [];

                using var reader = (MySqlDataReader) await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    scores.Add(new(){
                        UserID = -1,
                        Nickname = reader.GetString("Nickname"),
                        ClearTime = reader.GetTimeSpan("ClearTime"),
                        UploadedTime = reader.GetDateTime("UploadedTime")
                    });

                return (scores, "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }
    }
}