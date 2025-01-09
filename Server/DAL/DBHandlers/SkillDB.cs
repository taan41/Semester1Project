using System.Data;
using MySql.Data.MySqlClient;

static class SkillDB
{
    public static async Task<(bool success, string errorMessage)> Add(Skill skill)
    {
        string query = @"
            INSERT INTO Skills (SkillID, Name, Rarity, Price, Type, Damage, Heal, MPCost)
            VALUES (@id, @name, @rarity, @price, @type, @damage, @heal, @mpCost);
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", skill.ID);
            cmd.Parameters.AddWithValue("@name", skill.Name);
            cmd.Parameters.AddWithValue("@rarity", (int) skill.Rarity);
            cmd.Parameters.AddWithValue("@price", skill.Price);
            cmd.Parameters.AddWithValue("@type", (int) skill.Type);
            cmd.Parameters.AddWithValue("@damage", skill.Damage);
            cmd.Parameters.AddWithValue("@heal", skill.Heal);
            cmd.Parameters.AddWithValue("@mpCost", skill.MPCost);

            await cmd.ExecuteNonQueryAsync();
            return (true, "");
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(Skill? requestedSkill, string errorMessage)> Get(int skillID)
    {
        string query = @"
            SELECT 
                SkillID,
                Name,
                Rarity,
                Price,
                Type,
                Damage,
                Heal,
                MPCost
            FROM 
                Skills
            WHERE 
                SkillID = @skillID
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@skillID", skillID);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows)
                return (null, "No skill found");

            await reader.ReadAsync();
            return (new Skill
            {
                ID = reader.GetInt32("SkillID"),
                Name = reader.GetString("Name"),
                Rarity = (ItemRarity) reader.GetInt32("Rarity"),
                Price = reader.GetInt32("Price"),
                Type = (SkillType) reader.GetInt32("Type"),
                Damage = reader.GetInt32("Damage"),
                Heal = reader.GetInt32("Heal"),
                MPCost = reader.GetInt32("MPCost")
            }, "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    }

    public static async Task<(List<Skill>? skills, string errorMessage)> GetAll()
    {
        string query = @"
            SELECT 
                SkillID,
                Name,
                Rarity,
                Price,
                Type,
                Damage,
                Heal,
                MPCost
            FROM 
                Skills
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows)
                return (null, "No skills found");

            List<Skill> skills = [];
            while (await reader.ReadAsync())
            {
                skills.Add(new Skill
                {
                    ID = reader.GetInt32("SkillID"),
                    Name = reader.GetString("Name"),
                    Rarity = (ItemRarity) reader.GetInt32("Rarity"),
                    Price = reader.GetInt32("Price"),
                    Type = (SkillType) reader.GetInt32("Type"),
                    Damage = reader.GetInt32("Damage"),
                    Heal = reader.GetInt32("Heal"),
                    MPCost = reader.GetInt32("MPCost")
                });
            }

            return (skills, "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    }
}