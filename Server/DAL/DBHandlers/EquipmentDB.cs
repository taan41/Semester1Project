using System.Data;
using MySql.Data.MySqlClient;

static class EquipmentDB
{
    public static async Task<(bool success, string errorMessage)> Add(Equipment equipment)
    {
        string query = @"
            INSERT INTO Equipment (EquipID, Name, Rarity, Price, Type, BonusATK, BonusHP, BonusMP)
            VALUES (@id, @name, @rarity, @price, @type, @bonusATK, @bonusHP, @bonusMP);
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@id", equipment.ID);
            cmd.Parameters.AddWithValue("@name", equipment.Name);
            cmd.Parameters.AddWithValue("@rarity", (int) equipment.Rarity);
            cmd.Parameters.AddWithValue("@price", equipment.Price);
            cmd.Parameters.AddWithValue("@type", (int) equipment.Type);
            cmd.Parameters.AddWithValue("@bonusATK", equipment.BonusATK);
            cmd.Parameters.AddWithValue("@bonusHP", equipment.BonusHP);
            cmd.Parameters.AddWithValue("@bonusMP", equipment.BonusMP);

            await cmd.ExecuteNonQueryAsync();
            return (true, "");
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(Equipment? requestedEquipment, string errorMessage)> Get(int equipID)
    {
        string query = @"
            SELECT 
                EquipID,
                Name,
                Rarity,
                Price,
                Type,
                BonusATK,
                BonusHP,
                BonusMP
            FROM 
                Equipment
            WHERE 
                EquipID = @equipID
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@equipID", equipID);

            using var reader = await cmd.ExecuteReaderAsync();

            if (! await reader.ReadAsync()) // Equipment not found
                return (null, $"No equipment with ID '{equipID}' found");

            return (new()
            {
                ID = reader.GetInt32("EquipID"),
                Name = reader.GetString("Name"),
                Rarity = (ItemRarity) reader.GetInt32("Rarity"),
                Price = reader.GetInt32("Price"),
                Type = (EquipType) reader.GetInt32("Type"),
                BonusATK = reader.GetInt32("BonusATK"),
                BonusHP = reader.GetInt32("BonusHP"),
                BonusMP = reader.GetInt32("BonusMP")
            }, "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    }

    public static async Task<(List<Equipment>? equipments, string errorMessage)> GetAll()
    {
        string query = @"
            SELECT 
                EquipID,
                Name,
                Rarity,
                Price,
                Type,
                BonusATK,
                BonusHP,
                BonusMP
            FROM 
                Equipment
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);

            using var reader = await cmd.ExecuteReaderAsync();

            List<Equipment> equipments = [];
            while (await reader.ReadAsync())
            {
                equipments.Add(new()
                {
                    ID = reader.GetInt32("EquipID"),
                    Name = reader.GetString("Name"),
                    Rarity = (ItemRarity) reader.GetInt32("Rarity"),
                    Price = reader.GetInt32("Price"),
                    Type = (EquipType) reader.GetInt32("Type"),
                    BonusATK = reader.GetInt32("BonusATK"),
                    BonusHP = reader.GetInt32("BonusHP"),
                    BonusMP = reader.GetInt32("BonusMP")
                });
            }

            return (equipments, "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    } 
}