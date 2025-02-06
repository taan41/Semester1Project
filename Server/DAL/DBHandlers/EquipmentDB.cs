using System.Data;
using DAL.GameComponents.Item;
using MySql.Data.MySqlClient;

namespace DAL.DBHandlers
{
    public static class EquipmentDB
    {
        public static async Task<(bool success, string error)> Add(Equipment equipment)
        {
            string query = @"
                INSERT INTO Equipments (
                    EquipID,
                    Name,
                    Rarity,
                    Price,
                    Type,
                    ATKPoint,
                    DEFPoint,
                    HPPoint,
                    MPPoint
                )
                VALUES (
                    @id,
                    @name,
                    @rarity,
                    @price,
                    @type,
                    @atk,
                    @def,
                    @hp,
                    @mp
                )
                ON DUPLICATE KEY UPDATE
                    Name = @name,
                    Rarity = @rarity,
                    Price = @price,
                    Type = @type,
                    ATKPoint = @atk,
                    DEFPoint = @def,
                    HPPoint = @hp,
                    MPPoint = @mp;
            ";

            try
            {
                using MySqlConnection conn = new(DBManager.ConnectionString);
                await conn.OpenAsync();

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@id", equipment.ID);
                cmd.Parameters.AddWithValue("@name", equipment.Name);
                cmd.Parameters.AddWithValue("@rarity", (int) equipment.ItemRarity);
                cmd.Parameters.AddWithValue("@price", equipment.Price);
                cmd.Parameters.AddWithValue("@type", (int) equipment.EquipType);
                cmd.Parameters.AddWithValue("@atk", equipment.BonusATKPoint);
                cmd.Parameters.AddWithValue("@def", equipment.BonusDEFPoint);
                cmd.Parameters.AddWithValue("@hp", equipment.BonusHPPoint);
                cmd.Parameters.AddWithValue("@mp", equipment.BonusMPPoint);

                await cmd.ExecuteNonQueryAsync();
                return (true, "");
            }
            catch (MySqlException ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(Equipment? requestedEquipment, string error)> Get(int equipID)
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
                    ItemRarity = (GameItem.Rarity) reader.GetInt32("Rarity"),
                    Price = reader.GetInt32("Price"),
                    EquipType = (Equipment.Type) reader.GetInt32("Type"),
                    BonusATKPoint = reader.GetInt32("ATKPoint"),
                    BonusDEFPoint = reader.GetInt32("DEFPoint"),
                    BonusHPPoint = reader.GetInt32("HPPoint"),
                    BonusMPPoint = reader.GetInt32("MPPoint")
                }, "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }

        public static async Task<(Dictionary<int, Equipment>? equipments, string error)> GetAll()
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
                        ItemRarity = (GameItem.Rarity) reader.GetInt32("Rarity"),
                        Price = reader.GetInt32("Price"),
                        EquipType = (Equipment.Type) reader.GetInt32("Type"),
                        BonusATKPoint = reader.GetInt32("ATKPoint"),
                        BonusDEFPoint = reader.GetInt32("DEFPoint"),
                        BonusHPPoint = reader.GetInt32("HPPoint"),
                        BonusMPPoint = reader.GetInt32("MPPoint")
                    };
                }

                return (equipments, "");
            }
            catch (MySqlException ex)
            {
                return (null, ex.Message);
            }
        }

        public static async Task<(bool success, string error)> UpdateID(int oldID, int newID)
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

        public static async Task<(bool success, string error)> Remove(int equipID)
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
    }
}