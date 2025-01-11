using System.Data;
using MySql.Data.MySqlClient;

static class MonsterDB
{
    public static async Task<(bool success, string errorMessage)> Add(Monster monster)
    {
        string query = @"
            INSERT INTO Monsters (MonsterID, Name, Type, Floor, ATK, DEF, HP)
            VALUES (@id, @name, @type, @floor, @atk, @def, @hp);
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
            cmd.Parameters.AddWithValue("@def", monster.DEF);
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
                DEF,
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
                DEF = reader.GetInt32("DEF"),
                MaxHP = reader.GetInt32("HP"),
                HP = reader.GetInt32("HP")
            }, "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    }

    public static async Task<(bool success, string errorMessage)> Update(Monster monster)
    {
        string query = @"
            UPDATE Monsters
            SET 
                Name = @name,
                Type = @type,
                Floor = @floor,
                ATK = @atk,
                DEF = @def,
                HP = @hp
            WHERE 
                MonsterID = @id
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
            cmd.Parameters.AddWithValue("@def", monster.DEF);
            cmd.Parameters.AddWithValue("@hp", monster.HP);

            await cmd.ExecuteNonQueryAsync();
            return (true, "");
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(bool success, string errorMessage)> UpdateID(int oldID, int newID)
    {
        string query = @"
            UPDATE Monsters
            SET 
                MonsterID = @newID
            WHERE 
                MonsterID = @oldID
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@oldID", oldID);
            cmd.Parameters.AddWithValue("@newID", newID);

            await cmd.ExecuteNonQueryAsync();
            return (true, "");
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(bool success, string errorMessage)> Delete(int monsterID)
    {
        string query = @"
            DELETE FROM Monsters
            WHERE 
                MonsterID = @monsterID
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@monsterID", monsterID);

            await cmd.ExecuteNonQueryAsync();
            return (true, "");
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(Dictionary<int, Monster>? monsters, string errorMessage)> GetAll()
    {
        string query = @"
            SELECT 
                MonsterID,
                Name,
                Type,
                Floor,
                ATK,
                DEF,
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

            Dictionary<int, Monster> monsters = [];
            while (await reader.ReadAsync())
            {
                monsters[reader.GetInt32("MonsterID")] = new Monster
                {
                    ID = reader.GetInt32("MonsterID"),
                    Name = reader.GetString("Name"),
                    Type = (MonsterType) reader.GetInt32("Type"),
                    Floor = reader.GetInt32("Floor"),
                    ATK = reader.GetInt32("ATK"),
                    DEF = reader.GetInt32("DEF"),
                    MaxHP = reader.GetInt32("HP"),
                    HP = reader.GetInt32("HP")
                };
            }

            return (monsters, "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    }
}