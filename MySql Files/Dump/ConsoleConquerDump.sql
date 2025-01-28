CREATE DATABASE  IF NOT EXISTS `consoleconquer` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `consoleconquer`;
-- MySQL dump 10.13  Distrib 8.0.40, for Win64 (x86_64)
--
-- Host: localhost    Database: consoleconquer
-- ------------------------------------------------------
-- Server version	8.0.40

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `activitylog`
--

DROP TABLE IF EXISTS `activitylog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `activitylog` (
  `LogID` int NOT NULL AUTO_INCREMENT,
  `LogTime` datetime DEFAULT CURRENT_TIMESTAMP,
  `Source` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Content` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`LogID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `activitylog`
--

LOCK TABLES `activitylog` WRITE;
/*!40000 ALTER TABLE `activitylog` DISABLE KEYS */;
/*!40000 ALTER TABLE `activitylog` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `configs`
--

DROP TABLE IF EXISTS `configs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configs` (
  `ConfigName` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `ConfigValue` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`ConfigName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configs`
--

LOCK TABLES `configs` WRITE;
/*!40000 ALTER TABLE `configs` DISABLE KEYS */;
INSERT INTO `configs` VALUES ('AssetConfig','{\"EquipPtPerRarityPerType\":[[3,6,10,15],[2,4,7,10],[1,2,3,5]]}'),('DatabaseConfig','{\"UsernameMin\":4,\"UsernameMax\":50,\"PasswordMin\":6,\"PasswordMax\":50,\"NicknameMin\":1,\"NicknameMax\":25,\"EmailMin\":1,\"EmailMax\":254}'),('GameConfig','{\"EntityMPRegenPercentage\":15,\"PlayerDefaultATK\":3,\"PlayerDefaultDEF\":0,\"PlayerDefaultHP\":25,\"PlayerDefaultMP\":10,\"PlayerDefaultGold\":100,\"PlayerMaxSkillCount\":3,\"MonsterDefaultATK\":1,\"MonsterDefaultHP\":10,\"MonsterPowerATKPercentage\":400,\"MonsterPowerHPPercentage\":100,\"ItemPriceBase\":200,\"ItemPriceRarityBonusPercentage\":100,\"ItemPriceEquipBonusPercentage\":30,\"ItemPriceSkillBonusPercentage\":15,\"ItemPriceSellingPercentage\":15,\"EquipPtATKPercentage\":200,\"EquipPtDEFPercentage\":100,\"EquipPtHPPercentage\":800,\"EquipPtMPPercentage\":400,\"SkillPtDmgPercentage\":150,\"SkillPtHealPercentage\":100,\"SkillRarityNormalPercentage\":100,\"SkillRarityRarePercentage\":150,\"SkillRarityEpicPercentage\":250,\"SkillRarityLegendaryPercentage\":400,\"SkillTypeSinglePercentage\":100,\"SkillTypeRandomPercentage\":120,\"SkillTypeAllPercentage\":50,\"ProgressMaxFloor\":3,\"ProgressMaxRoom\":16,\"EventPowerPerRoom\":100,\"EventPowerPerFloorRatio\":50,\"EventPowerElitePercentage\":200,\"EventPowerBossPercentage\":400,\"EventRoomCountTreasure\":10,\"EventRoomCountCamp\":5,\"EventRoomCountShop\":5,\"EventGoldBase\":75,\"EventGoldPerNormal\":5,\"EventGoldPerElite\":15,\"EventGoldPerBoss\":50,\"EventGoldFloorPercentage\":25,\"EventGoldTreasure\":150,\"EventGoldTreasurePerFloorPercentage\":50}'),('ServerConfig','{\"ServerIP\":\"26.244.97.115\",\"Port\":6969}');
/*!40000 ALTER TABLE `configs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `equipments`
--

DROP TABLE IF EXISTS `equipments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `equipments` (
  `EquipID` int NOT NULL,
  `Name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Rarity` int NOT NULL,
  `Price` int NOT NULL,
  `Type` int NOT NULL,
  `ATKPoint` int NOT NULL,
  `DEFPoint` int NOT NULL,
  `HPPoint` int NOT NULL,
  `MPPoint` int NOT NULL,
  PRIMARY KEY (`EquipID`),
  KEY `idx_equipID` (`EquipID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `equipments`
--

LOCK TABLES `equipments` WRITE;
/*!40000 ALTER TABLE `equipments` DISABLE KEYS */;
INSERT INTO `equipments` VALUES (1,'Starter Sword',0,-1,0,1,1,0,0),(2,'Starter Bow',0,-1,0,2,0,0,0),(3,'Starter Staff',0,-1,0,1,0,0,1),(4,'Rusted Iron Sword',0,-1,0,1,1,1,0),(5,'Cracked Slingshot',0,-1,0,3,0,0,0),(6,'Worn Staff',0,-1,0,1,0,0,2),(7,'Rusted Chainmail',0,-1,1,0,0,2,0),(8,'Torn Leather Vest',0,-1,1,1,0,1,0),(9,'Apprentice Cloak',0,-1,1,0,0,1,1),(10,'Tarnished Ring',0,-1,2,0,0,0,1),(11,'Worn Leather Gloves',0,-1,2,1,0,0,0),(12,'Cracked Wooden Shield',0,-1,2,0,1,0,0),(101,'Gleaming Blade',1,-1,0,3,1,2,0),(102,'Reinforced Crossbow',1,-1,0,5,0,1,0),(103,'Enchanted Staff',1,-1,0,2,0,0,4),(104,'Reinforced Scale Mail',1,-1,1,0,1,3,0),(105,'Sturdy Leather Jacket',1,-1,1,1,0,2,1),(106,'Enchanted Mantle',1,-1,1,0,0,2,2),(107,'Tempered Iron Shield',1,-1,2,0,1,1,0),(108,'Polished Wooden Quiver',1,-1,2,2,0,0,0),(109,'Blessed Ring',1,-1,2,0,0,0,2),(201,'Radiant Greatsword',2,-1,0,5,2,3,0),(202,'Mastercrafted Longbow',2,-1,0,8,0,1,1),(203,'Runed Staff',2,-1,0,3,0,1,6),(204,'Royal Half-Plate',2,-1,1,1,2,4,0),(205,'Royal Light Armour',2,-1,1,3,1,2,1),(206,'Royal Magic Robe',2,-1,1,0,1,2,4),(207,'Royal Steel Shield',2,-1,2,0,2,1,0),(208,'Royal Steel Quiver',2,-1,2,3,0,0,0),(209,'Royal Enchanted Ring',2,-1,2,0,0,0,3),(301,'Godforged Warhammer',3,-1,0,8,3,4,0),(302,'Celestial Bow',3,-1,0,10,0,2,3),(303,'Divine Orb',3,-1,0,5,0,0,10),(304,'Godforged Bulwark',3,-1,1,0,3,7,0),(305,'Celestial Cloak',3,-1,1,5,1,2,2),(306,'Divine Shroud',3,-1,1,0,1,4,5),(307,'Godforged Shield',3,-1,2,0,3,2,0),(308,'Celestial Quiver',3,-1,2,5,0,0,0),(309,'Divine Ring',3,-1,2,0,0,0,5);
/*!40000 ALTER TABLE `equipments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `gamesaves`
--

DROP TABLE IF EXISTS `gamesaves`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `gamesaves` (
  `SaveID` int NOT NULL AUTO_INCREMENT,
  `UserID` int NOT NULL,
  `UploadedTime` datetime DEFAULT CURRENT_TIMESTAMP,
  `SaveContent` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`SaveID`),
  UNIQUE KEY `UserID` (`UserID`),
  KEY `idx_userID` (`UserID`),
  CONSTRAINT `gamesaves_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `users` (`UserID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `gamesaves`
--

LOCK TABLES `gamesaves` WRITE;
/*!40000 ALTER TABLE `gamesaves` DISABLE KEYS */;
/*!40000 ALTER TABLE `gamesaves` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `monsters`
--

DROP TABLE IF EXISTS `monsters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `monsters` (
  `MonsterID` int NOT NULL,
  `Name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Type` int NOT NULL,
  `Floor` int NOT NULL,
  `ATK` int NOT NULL,
  `DEF` int NOT NULL,
  `HP` int NOT NULL,
  PRIMARY KEY (`MonsterID`),
  KEY `idx_monsterID` (`MonsterID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `monsters`
--

LOCK TABLES `monsters` WRITE;
/*!40000 ALTER TABLE `monsters` DISABLE KEYS */;
INSERT INTO `monsters` VALUES (1,'Slime',0,1,10,0,100),(2,'Rat',0,1,15,0,100),(101,'Mother Slime',1,1,15,1,100),(201,'King Slime',2,1,10,3,100),(1001,'Goldfish',0,2,10,1,100),(1101,'Shark',1,2,10,3,100),(1201,'Ocean God Neptune',2,2,10,6,100),(2001,'Skeleton',0,3,10,2,100),(2101,'Grave Digger',1,3,10,5,100),(2201,'Supreme Necromancer',2,3,10,9,100);
/*!40000 ALTER TABLE `monsters` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `scores`
--

DROP TABLE IF EXISTS `scores`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `scores` (
  `RunID` int NOT NULL,
  `UserID` int NOT NULL,
  `ClearTime` time(3) NOT NULL,
  `UploadedTime` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`RunID`),
  KEY `idx_userID` (`UserID`),
  CONSTRAINT `scores_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `users` (`UserID`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `scores`
--

LOCK TABLES `scores` WRITE;
/*!40000 ALTER TABLE `scores` DISABLE KEYS */;
INSERT INTO `scores` VALUES (-41292310,5,'00:11:44.145','2025-01-28 01:02:37');
/*!40000 ALTER TABLE `scores` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `skills`
--

DROP TABLE IF EXISTS `skills`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `skills` (
  `SkillID` int NOT NULL,
  `Name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Rarity` int NOT NULL,
  `Price` int NOT NULL,
  `Type` int NOT NULL,
  `DmgPoint` int NOT NULL,
  `HealPoint` int NOT NULL,
  `MPCost` int NOT NULL,
  PRIMARY KEY (`SkillID`),
  KEY `idx_skillID` (`SkillID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `skills`
--

LOCK TABLES `skills` WRITE;
/*!40000 ALTER TABLE `skills` DISABLE KEYS */;
INSERT INTO `skills` VALUES (1,'Bandage',0,-1,0,0,5,5),(2,'Weak Stab',0,-1,0,5,0,5),(3,'Careless Slash',0,-1,2,5,0,5),(101,'Cure',1,-1,0,0,5,5),(102,'Piercing Strike',1,-1,0,5,0,5),(103,'Shattering Blow',1,-1,2,5,0,5),(104,'Flameburst',1,-1,2,10,0,10),(201,'Revitalize',2,-1,0,0,10,10),(202,'Precision Thrust',2,-1,0,5,0,5),(203,'Whirlwind Slash',2,-1,2,5,0,5),(204,'Frost Nova',2,-1,2,15,0,15),(301,'Re-life',3,-1,0,0,10,10),(302,'Godslayer Strike',3,-1,0,5,0,5),(303,'Earthshatter',3,-1,2,5,0,5),(304,'Meteor Storm',3,-1,2,20,0,20);
/*!40000 ALTER TABLE `skills` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `UserID` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Nickname` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Email` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `PasswordHash` varbinary(32) DEFAULT NULL,
  `Salt` varbinary(16) DEFAULT NULL,
  PRIMARY KEY (`UserID`),
  UNIQUE KEY `Username` (`Username`),
  KEY `idx_userID` (`UserID`),
  KEY `idx_username` (`Username`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (2,'huhu','catre','hung12@gmail.com',_binary '7�HR\�ZnS\�vb\�	F�\��J\�C�h_,\\z\�U\�',_binary '\��.�\�mrA\0�\"�3t'),(3,'vutien1406','BlueMoon','anhkhongdoiqua@gmail.com',_binary '.�G�ɽ{�\\6���|A�\��\�\�\�S�A�',_binary '��\�\�\�s��Kj�f\�\�X'),(4,'taan41','taan','consoleconquer@erm.vn',_binary 'Md��=�\�,޸ڽM\�\�<k\�-�xí�',_binary '2:�\�c\� &2�\�Cõ�'),(5,'taan','taan','taan',_binary '\�\��Q�\��D�\�V�\"\����d]�\�\�\r�f>��',_binary 'gF���\�\�Dd��B'),(6,'player00001','Player01','player01@gmail.com',_binary '����!w\r\�T���\r[jO{\�J\�\Z\Z8��\�\�',_binary 'D\�(\�\n$Hϳ�U�\�r');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-01-28  2:02:56
