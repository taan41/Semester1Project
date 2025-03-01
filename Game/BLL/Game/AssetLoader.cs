using BLL.Game.Components.Entity;
using BLL.Game.Components.Item;
using DAL;

namespace BLL.Game;

public class AssetLoader
{
    private static Dictionary<int, Equipment> Equipments = [];
    private static Dictionary<int, Skill> Skills = [];
    private static Dictionary<int, Monster> Monsters = [];
    
    static AssetLoader()
        => LoadAsset();

    public static void LoadAsset()
    {
        var equipmentsJson = FileManager.ReadJson<Dictionary<int, Equipment>>(FileManager.FolderNames.Assets, FileManager.FileNames.Equips);
        if (equipmentsJson != null)
        {
            Equipments = equipmentsJson;
        }
        else 
        {
            Equipments = DefaultAsset.Equipments;
            FileManager.WriteJson(FileManager.FolderNames.Assets, FileManager.FileNames.Equips, Equipments);
        }

        var skillsJson = FileManager.ReadJson<Dictionary<int, Skill>>(FileManager.FolderNames.Assets, FileManager.FileNames.Skills);
        if (skillsJson != null)
        {
            Skills = skillsJson;
        }
        else 
        {
            Skills = DefaultAsset.Skills;
            FileManager.WriteJson(FileManager.FolderNames.Assets, FileManager.FileNames.Skills, Skills);
        }

        var monstersJson = FileManager.ReadJson<Dictionary<int, Monster>>(FileManager.FolderNames.Assets, FileManager.FileNames.Monsters);
        if (monstersJson != null)
        {
            Monsters = monstersJson;
        }
        else 
        {
            Monsters = DefaultAsset.Monsters;
            FileManager.WriteJson(FileManager.FolderNames.Assets, FileManager.FileNames.Monsters, Monsters);
        }

        foreach(Equipment equip in Equipments.Values)
            if (equip.ID >= IDTracker.EquipIDs[(int) equip.ItemRarity])
                IDTracker.EquipIDs[(int) equip.ItemRarity] = equip.ID + 1;

        foreach(Skill skill in Skills.Values)
            if (skill.ID >= IDTracker.SkillIDs[(int) skill.ItemRarity])
                IDTracker.SkillIDs[(int) skill.ItemRarity] = skill.ID + 1;

        foreach(Monster monster in Monsters.Values)
            if (monster.ID >= IDTracker.MonsterIDs[monster.Floor - 1][(int) monster.MonsterType])
                IDTracker.MonsterIDs[monster.Floor - 1][(int) monster.MonsterType] = monster.ID + 1;
    }

    public static Equipment GetEquip(int id)
        => Equipments.TryGetValue(id, out Equipment? equip) ? equip : Equipment.DefaultEquipment();

    public static Skill GetSkill(int id)
        => Skills.TryGetValue(id, out Skill? skill) ? skill : Skill.DefaultSkill();

    public static Monster GetMonster(int id)
        => Monsters.TryGetValue(id, out Monster? monster) ? monster : Monster.DefaultMonster();
}