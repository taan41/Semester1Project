using System.Data;
using DAL.Persistence.GameComponents.Item;
using MySql.Data.MySqlClient;

namespace DAL.DBHandlers
{
    public static class SkillDB
    {
        public static async Task<(bool success, string error)> Add(Skill skill)
        {
            string query = @"
                INSERT INTO Skills (
                    SkillID,
                    Name,
                    Rarity,
                    Price,
                    Type,
                    DmgPoint,
                    HealPoint,
                    MPCost
                )
                VALUES (
                    @id,
                    @name,
                    @rarity,
                    @price,
                    @type,
                    @dmgPt,
                    @healPt,
                    @mpCost
                )
                ON DUPLICATE KEY UPDATE
                    Name = @name,
                    Rarity = @rarity,
                    Price = @price,
                    Type = @type,
                    DmgPoint = @dmgPt,
                    HealPoint = @healPt,
                    MPCost = @mpCost
            ";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@id", skill.ID);
                cmd.Parameters.AddWithValue("@name", skill.Name);
                cmd.Parameters.AddWithValue("@rarity", (int) skill.ItemRarity);
                cmd.Parameters.AddWithValue("@price", skill.Price);
                cmd.Parameters.AddWithValue("@type", (int) skill.SkillType);
                cmd.Parameters.AddWithValue("@dmgPt", skill.DamagePoint);
                cmd.Parameters.AddWithValue("@healPt", skill.HealPoint);
                cmd.Parameters.AddWithValue("@mpCost", skill.MPCost);

                await cmd.ExecuteNonQueryAsync();
                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(Skill? requestedSkill, string error)> Get(int skillID)
        {
            string query = @"
                SELECT 
                    SkillID,
                    Name,
                    Rarity,
                    Price,
                    Type,
                    DmgPoint,
                    HealPoint,
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
                    ItemRarity = (GameItem.Rarity) reader.GetInt32("Rarity"),
                    Price = reader.GetInt32("Price"),
                    SkillType = (Skill.Type) reader.GetInt32("Type"),
                    DamagePoint = reader.GetInt32("DmgPoint"),
                    HealPoint = reader.GetInt32("HealPoint"),
                    MPCost = reader.GetInt32("MPCost")
                }, "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }

        public static async Task<(bool success, string error)> Update(Skill skill, int dmgPoint, int healPoint)
        {
            string query = @"
                UPDATE Skills
                SET 
                    Name = @name,
                    Rarity = @rarity,
                    Price = @price,
                    Type = @type,
                    DmgPoint = @dmgPt,
                    HealPoint = @healPt,
                    MPCost = @mpCost
                WHERE 
                    SkillID = @id
            ";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@id", skill.ID);
                cmd.Parameters.AddWithValue("@name", skill.Name);
                cmd.Parameters.AddWithValue("@rarity", (int) skill.ItemRarity);
                cmd.Parameters.AddWithValue("@price", skill.Price);
                cmd.Parameters.AddWithValue("@type", (int) skill.SkillType);
                cmd.Parameters.AddWithValue("@dmgPt", dmgPoint);
                cmd.Parameters.AddWithValue("@healPt", healPoint);
                cmd.Parameters.AddWithValue("@mpCost", skill.MPCost);

                await cmd.ExecuteNonQueryAsync();
                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(bool success, string error)> UpdateID(int oldID, int newID)
        {
            string query = @"
                UPDATE Skills
                SET 
                    SkillID = @newID
                WHERE 
                    SkillID = @oldID
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

        public static async Task<(bool success, string error)> Remove(int skillID)
        {
            string query = @"
                DELETE FROM Skills
                WHERE SkillID = @skillID
            ";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@skillID", skillID);

                await cmd.ExecuteNonQueryAsync();
                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(Dictionary<int, Skill>? skills, string error)> GetAll()
        {
            string query = @"
                SELECT 
                    SkillID,
                    Name,
                    Rarity,
                    Price,
                    Type,
                    DmgPoint,
                    HealPoint,
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

                Dictionary<int, Skill> skills = [];
                while (await reader.ReadAsync())
                {
                    skills[reader.GetInt32("SkillID")] = new Skill
                    {
                        ID = reader.GetInt32("SkillID"),
                        Name = reader.GetString("Name"),
                        ItemRarity = (GameItem.Rarity) reader.GetInt32("Rarity"),
                        Price = reader.GetInt32("Price"),
                        SkillType = (Skill.Type) reader.GetInt32("Type"),
                        DamagePoint = reader.GetInt32("DmgPoint"),
                        HealPoint = reader.GetInt32("HealPoint"),
                        MPCost = reader.GetInt32("MPCost")
                    };
                }

                return (skills, "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }
    }
}