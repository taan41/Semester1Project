using System.Text;
using MySql.Data.MySqlClient;

namespace DAL.DBHandlers
{
    public static class DBManager
    {
        public static class Tables
        {
            public const string Users = "Users";
            public const string Scores = "Scores";
            public const string GameSaves = "GameSaves";
            public const string Equipments = "Equipments";
            public const string Skills = "Skills";
            public const string Monsters = "Monsters";
            public const string ActivityLog = "ActivityLogs";
            public const string Configs = "Configs";
        }

        public static class ConfigNames
        {
            public const string ServerConfig = "ServerConfig";
            public const string GameConfig = "GameConfig";
            public const string DatabaseConfig = "DatabaseConfig";
            public const string AssetConfig = "AssetConfig";
        }
        
        public static string ConnectionString { get; private set; } = "";

        public static bool Initialize(string server, string db, string uid, string? password, out string error)
        {
            error = "";

            try
            {
                StringBuilder sb = new();

                sb.Append($"server={server};uid={uid}");

                if (password != null)
                    sb.Append($";pwd={password};");

                using MySqlConnection conn = new(sb.ToString());
                conn.Open();

                using MySqlCommand checkDBCmd = new($"SHOW DATABASES LIKE '{db}';", conn);

                using MySqlDataReader reader = checkDBCmd.ExecuteReader();
                if (!reader.HasRows) // Database does not exist
                {
                    reader.Close();
                    CreateDB(conn, db);
                }

                sb.Append($"database={db};");
                ConnectionString = sb.ToString();
                return true;
            }
            catch (MySqlException ex)
            {
                // error = ex.Message;
                error = ex.ToString();
                return false;
            }
        }
        
        private static void CreateDB(MySqlConnection conn, string dbName)
        {
            using (MySqlCommand createDB = new(@$"
                CREATE DATABASE {dbName}
                CHARACTER SET = utf8mb4
                COLLATE = utf8mb4_unicode_ci;
                ", conn))
            {
                createDB.ExecuteNonQuery();
            }

            conn.ChangeDatabase(dbName);

            using (MySqlCommand createUsers = new(@$"
                CREATE TABLE {Tables.Users} (
                    UserID INT AUTO_INCREMENT PRIMARY KEY,
                    Username VARCHAR(255) NOT NULL UNIQUE,
                    Nickname VARCHAR(255) NOT NULL,
                    Email VARCHAR(255) NOT NULL,
                    PasswordHash VARBINARY(32) DEFAULT NULL,
                    Salt VARBINARY(16) DEFAULT NULL,
                    INDEX idx_userID (UserID),
                    INDEX idx_username (Username)
                )", conn))
            {
                createUsers.ExecuteNonQuery();
            }

            using (MySqlCommand createScores = new(@$"
                CREATE TABLE {Tables.Scores} (
                    RunID INT PRIMARY KEY,
                    UserID INT NOT NULL,
                    ClearTime TIME(3) NOT NULL,
                    UploadedTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON UPDATE CASCADE ON DELETE CASCADE,
                    INDEX idx_userID (UserID)
                )", conn))
            {
                createScores.ExecuteNonQuery();
            }

            using (MySqlCommand createDatas = new(@$"
                CREATE TABLE {Tables.GameSaves} (
                    SaveID INT AUTO_INCREMENT PRIMARY KEY,
                    UserID INT NOT NULL UNIQUE,
                    UploadedTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                    SaveContent TEXT,
                    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON UPDATE CASCADE ON DELETE CASCADE,
                    INDEX idx_userID (UserID)
                )", conn))
            {
                createDatas.ExecuteNonQuery();
            }

            using (MySqlCommand createEquipments = new(@$"
                CREATE TABLE {Tables.Equipments} (
                    EquipID INT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Rarity INT NOT NULL,
                    Price INT NOT NULL,
                    Type INT NOT NULL,
                    ATKPoint INT NOT NULL,
                    DEFPoint INT NOT NULL,
                    HPPoint INT NOT NULL,
                    MPPoint INT NOT NULL,
                    INDEX idx_equipID (EquipID)
                )", conn))
            {
                createEquipments.ExecuteNonQuery();
            }

            using (MySqlCommand createSkills = new(@$"
                CREATE TABLE {Tables.Skills} (
                    SkillID INT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Rarity INT NOT NULL,
                    Price INT NOT NULL,
                    Type INT NOT NULL,
                    DmgPoint INT NOT NULL,
                    HealPoint INT NOT NULL,
                    MPCost INT NOT NULL,
                    INDEX idx_skillID (SkillID)
                )", conn))
            {
                createSkills.ExecuteNonQuery();
            }

            using (MySqlCommand createMonsters = new(@$"
                CREATE TABLE {Tables.Monsters} (
                    MonsterID INT PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Type INT NOT NULL,
                    Floor INT NOT NULL,
                    ATK INT NOT NULL,
                    DEF INT NOT NULL,
                    HP INT NOT NULL,
                    INDEX idx_monsterID (MonsterID)
                )", conn))
            {
                createMonsters.ExecuteNonQuery();
            }

            using (MySqlCommand createLog = new(@$"
                CREATE TABLE {Tables.ActivityLog} (
                    LogID INT AUTO_INCREMENT PRIMARY KEY,
                    LogTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Source VARCHAR(255),
                    Content TEXT
                )", conn))
            {
                createLog.ExecuteNonQuery();
            }

            using (MySqlCommand createConfigs = new(@$"
                CREATE TABLE {Tables.Configs} (
                    ConfigName VARCHAR(255) PRIMARY KEY,
                    ConfigValue TEXT
                )", conn))
            {
                createConfigs.ExecuteNonQuery();
            }
        }
    }
}