using System.Data;
using MySql.Data.MySqlClient;

static class EquipmentDB
{
    public static async Task<(bool success, string errorMessage)> Add(Equipment equipment)
    {
        string query = @"
            INSERT INTO Equipments (EquipID, Name, Rarity, Price, Type, ATKPoint, DEFPoint, HPPoint, MPPoint)
            VALUES (@id, @name, @rarity, @price, @type, @atk, @def, @hp, @mp);
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
            cmd.Parameters.AddWithValue("@atk", equipment.BonusATK / AssetMetadata.Instance.EquipPointMultiplier[0]);
            cmd.Parameters.AddWithValue("@def", equipment.BonusDEF / AssetMetadata.Instance.EquipPointMultiplier[1]);
            cmd.Parameters.AddWithValue("@hp", equipment.BonusHP / AssetMetadata.Instance.EquipPointMultiplier[2]);
            cmd.Parameters.AddWithValue("@mp", equipment.BonusMP / AssetMetadata.Instance.EquipPointMultiplier[3]);

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
                ATKPoint,
                DEFPoint,
                HPPoint,
                MPPoint
            FROM 
                Equipments
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
                BonusATK = reader.GetInt32("ATKPoint") * AssetMetadata.Instance.EquipPointMultiplier[0],
                BonusDEF = reader.GetInt32("DEFPoint") * AssetMetadata.Instance.EquipPointMultiplier[1],
                BonusHP = reader.GetInt32("HPPoint") * AssetMetadata.Instance.EquipPointMultiplier[2],
                BonusMP = reader.GetInt32("MPPoint") * AssetMetadata.Instance.EquipPointMultiplier[3]
            }, "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    }

    public static async Task<(bool success, string errorMessage)> Update(Equipment equipment)
    {
        string query = @"
            UPDATE Equipments
            SET 
                Name = @name,
                Rarity = @rarity,
                Price = @price,
                Type = @type,
                ATKPoint = @atk,
                DEFPoint = @def,
                HPPoint = @hp,
                MPPoint = @mp
            WHERE 
                EquipID = @id
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
            cmd.Parameters.AddWithValue("@atk", equipment.BonusATK / AssetMetadata.Instance.EquipPointMultiplier[0]);
            cmd.Parameters.AddWithValue("@def", equipment.BonusDEF / AssetMetadata.Instance.EquipPointMultiplier[1]);
            cmd.Parameters.AddWithValue("@hp", equipment.BonusHP / AssetMetadata.Instance.EquipPointMultiplier[2]);
            cmd.Parameters.AddWithValue("@mp", equipment.BonusMP / AssetMetadata.Instance.EquipPointMultiplier[3]);

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
            UPDATE Equipments
            SET 
                EquipID = @newID
            WHERE 
                EquipID = @oldID
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

    public static async Task<(bool success, string errorMessage)> Delete(int equipID)
    {
        string query = @"
            DELETE FROM Equipments
            WHERE EquipID = @equipID
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@equipID", equipID);

            await cmd.ExecuteNonQueryAsync();
            return (true, "");
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(Dictionary<int, Equipment>? equipments, string errorMessage)> GetAll()
    {
        string query = @"
            SELECT 
                EquipID,
                Name,
                Rarity,
                Price,
                Type,
                ATKPoint,
                DEFPoint,
                HPPoint,
                MPPoint
            FROM 
                Equipments
        ";

        try
        {
            using MySqlConnection conn = new(DBManager.ConnectionString);
            await conn.OpenAsync();

            using MySqlCommand cmd = new(query, conn);

            using var reader = await cmd.ExecuteReaderAsync();

            Dictionary<int, Equipment> equipments = [];
            while (await reader.ReadAsync())
            {
                equipments[reader.GetInt32("EquipID")] = new()
                {
                    ID = reader.GetInt32("EquipID"),
                    Name = reader.GetString("Name"),
                    Rarity = (ItemRarity) reader.GetInt32("Rarity"),
                    Price = reader.GetInt32("Price"),
                    Type = (EquipType) reader.GetInt32("Type"),
                    BonusATK = reader.GetInt32("ATKPoint") * AssetMetadata.Instance.EquipPointMultiplier[0],
                    BonusDEF = reader.GetInt32("DEFPoint") * AssetMetadata.Instance.EquipPointMultiplier[1],
                    BonusHP = reader.GetInt32("HPPoint") * AssetMetadata.Instance.EquipPointMultiplier[2],
                    BonusMP = reader.GetInt32("MPPoint") * AssetMetadata.Instance.EquipPointMultiplier[3]
                };
            }

            return (equipments, "");
        }
        catch (MySqlException ex)
        {
            return (null, ex.Message);
        }
    } 
}