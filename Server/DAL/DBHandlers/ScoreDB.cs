using MySql.Data.MySqlClient;

static class ScoreDB
{
    public static async Task<(bool success, string errorMessage)> Add(Score score)
    {
        string query = "INSERT INTO Scores (UserID, ClearTime) VALUES (@userID, @clearTime)";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@userID", score.UserID);
            cmd.Parameters.AddWithValue("@clearTime", score.ClearTime);

            await cmd.ExecuteNonQueryAsync();

            return (true, "");
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(List<Score>? scores, string errorMessage)> GetUser(int userID)
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
                scores.Add(new(
                    userID, 
                    reader.GetString("Nickname"), 
                    reader.GetTimeSpan("ClearTime")
                ));

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
                scores.Add(new(
                    null,
                    reader.GetString("Nickname"), 
                    reader.GetTimeSpan("ClearTime")
                ));

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
                scores.Add(new(
                    null,
                    reader.GetString("Nickname"), 
                    reader.GetTimeSpan("ClearTime")
                ));

            return (scores, "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    }
}