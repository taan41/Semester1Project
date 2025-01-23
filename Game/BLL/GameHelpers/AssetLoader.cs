using BLL.GameComponents.EntityComponents;
using BLL.GameComponents.ItemComponents;
using DAL;

namespace BLL.GameHelpers
{
    public class AssetLoader
    {
        public static Dictionary<int, Equipment> Equipments { get; protected set; } = [];
        public static Dictionary<int, Skill> Skills { get; protected set; } = [];
        public static Dictionary<int, Monster> Monsters { get; protected set; } = [];
        
        static AssetLoader()
            => Load();

        public static void Load()
        {
            Equipments = FileManager.ReadJson<Dictionary<int, Equipment>>(FileManager.FolderNames.Assets, FileManager.FileNames.Equips) ?? [];
            Skills = FileManager.ReadJson<Dictionary<int, Skill>>(FileManager.FolderNames.Assets, FileManager.FileNames.Skills) ?? [];
            Monsters = FileManager.ReadJson<Dictionary<int, Monster>>(FileManager.FolderNames.Assets, FileManager.FileNames.Monsters) ?? [];

            foreach(Equipment equip in Equipments.Values)
                if (equip.ID >= IDTracker.Equip[(int) equip.ItemRarity])
                    IDTracker.Equip[(int) equip.ItemRarity] = equip.ID + 1;

            foreach(Skill skill in Skills.Values)
                if (skill.ID >= IDTracker.Skill[(int) skill.SkillType])
                    IDTracker.Skill[(int) skill.SkillType] = skill.ID + 1;

            foreach(Monster monster in Monsters.Values)
                if (monster.ID >= IDTracker.Monster[monster.Floor - 1][(int) monster.MonsterType])
                    IDTracker.Monster[monster.Floor - 1][(int) monster.MonsterType] = monster.ID + 1;
        }
    }
}