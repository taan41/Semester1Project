using System.Data;
using MySql.Data.MySqlClient;

static class MonsterDB
{
    public static async Task<(bool success, string errorMessage)> Add(Monster monster)
    {
        string query = @"
            INSERT INTO Monsters (MonsterID, Name, Type, Floor, ATK, HP)
            VALUES (@id, @name, @type, @floor, @atk, @hp);
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", monster.ID);
            cmd.Parameters.AddWithValue("@name", monster.Name);
            cmd.Parameters.AddWithValue("@type", (int) monster.Type);
            cmd.Parameters.AddWithValue("@floor", monster.Floor);
            cmd.Parameters.AddWithValue("@atk", monster.ATK);
            cmd.Parameters.AddWithValue("@hp", monster.HP);

            await cmd.ExecuteNonQueryAsync();
            return (true, "");
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(Monster? requestedMonster, string errorMessage)> Get(int monsterID)
    {
        string query = @"
            SELECT 
                MonsterID,
                Name,
                Type,
                Floor,
                ATK,
                HP
            FROM 
                Monsters
            WHERE 
                MonsterID = @monsterID
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@monsterID", monsterID);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows)
                return (null, "No data found");

            await reader.ReadAsync();
            return (new Monster
            {
                ID = reader.GetInt32("MonsterID"),
                Name = reader.GetString("Name"),
                Type = (MonsterType) reader.GetInt32("Type"),
                Floor = reader.GetInt32("Floor"),
                ATK = reader.GetInt32("ATK"),
                HP = reader.GetInt32("HP")
            }, "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    }

    public static async Task<(List<Monster>? monsters, string errorMessage)> GetAll()
    {
        string query = @"
            SELECT 
                MonsterID,
                Name,
                Type,
                Floor,
                ATK,
                HP
            FROM 
                Monsters
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            List<Monster> monsters = [];
            while (await reader.ReadAsync())
            {
                monsters.Add(new Monster
                {
                    ID = reader.GetInt32("MonsterID"),
                    Name = reader.GetString("Name"),
                    Type = (MonsterType) reader.GetInt32("Type"),
                    Floor = reader.GetInt32("Floor"),
                    ATK = reader.GetInt32("ATK"),
                    HP = reader.GetInt32("HP")
                });
            }

            return (monsters, "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    }
}