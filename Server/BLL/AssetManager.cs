using DAL;
using DAL.DBHandlers;
using DAL.Persistence.GameComponents.EntityComponents;
using DAL.Persistence.GameComponents.ItemComponents;

namespace BLL
{
    public class AssetManager
    {
        private static ConfigManager Config => ConfigManager.Instance;

        public static AssetManager Instance { get; } = new(true);

        public Dictionary<int, Equipment> Equipments = [];
        public Dictionary<int, Skill> Skills  = [];
        public Dictionary<int, Monster> Monsters = [];

        public int[] EquipIDs = new int[Enum.GetValues(typeof(Item.Rarity)).Length];
        public int[] SkillIDs = new int[Enum.GetValues(typeof(Item.Rarity)).Length];
        public int[][] MonsterIDs = new int[Config.GameConfig.ProgressMaxFloor][];

        private AssetManager(bool load = false)
        {
            for (int i = 0; i < EquipIDs.Length; i++)
            {
                EquipIDs[i] = i * 100 + 1;
            }

            for (int i = 0; i < SkillIDs.Length; i++)
            {
                SkillIDs[i] = i * 100 + 1;
            }

            for (int i = 0; i < MonsterIDs.Length; i++)
            {
                MonsterIDs[i] = new int[Enum.GetValues(typeof(Monster.Type)).Length];

                for (int j = 0; j < MonsterIDs[i].Length; j++)
                {
                    MonsterIDs[i][j] = i * 1000 + j * 100 + 1;
                }
            }

            if (load)
                LoadAssets().Wait();
        }

        public async Task LoadAssets()
        {
            Equipments = (await EquipmentDB.GetAll()).equipments ?? [];
            Skills = (await SkillDB.GetAll()).skills ?? [];
            Monsters = (await MonsterDB.GetAll(Config.GameConfig.ProgressMaxFloor)).monsters ?? [];

            foreach (var equipment in Equipments.Values)
                if (equipment.ID >= EquipIDs[(int) equipment.ItemRarity])
                    EquipIDs[(int) equipment.ItemRarity] = equipment.ID + 1;

            foreach (var skill in Skills.Values)
                if (skill.ID >= SkillIDs[(int) skill.ItemRarity])
                    SkillIDs[(int) skill.ItemRarity] = skill.ID + 1;

            foreach (var monster in Monsters.Values)
                if (monster.ID >= MonsterIDs[monster.Floor - 1][(int) monster.MonsterType])
                    MonsterIDs[monster.Floor - 1][(int) monster.MonsterType] = monster.ID + 1;
        }

        public async Task<bool> Add(Equipment equip)
        {
            if (!(await EquipmentDB.Add(equip)).success)
                return false;

            Equipments[equip.ID] = equip;
            if (equip.ID >= EquipIDs[(int) equip.ItemRarity])
                EquipIDs[(int) equip.ItemRarity] = equip.ID + 1;
            return true;
        }

        public async Task<bool> Add(Skill skill)
        {
            if (!(await SkillDB.Add(skill)).success)
                return false;

            Skills[skill.ID] = skill;
            if (skill.ID >= SkillIDs[(int) skill.ItemRarity])
                SkillIDs[(int) skill.ItemRarity] = skill.ID + 1;
            return true;
        }

        public async Task<bool> Add(Monster monster)
        {
            if (!(await MonsterDB.Add(monster)).success)
                return false;

            Monsters[monster.ID] = monster;
            if (monster.ID >= MonsterIDs[monster.Floor - 1][(int) monster.MonsterType])
                MonsterIDs[monster.Floor - 1][(int) monster.MonsterType] = monster.ID + 1;
            return true;
        }

        public async Task<bool> Remove(Equipment equip)
        {
            if (!(await EquipmentDB.Remove(equip.ID)).success)
                return false;

            Equipments.Remove(equip.ID);

            for (int i = equip.ID; i < EquipIDs[(int) equip.ItemRarity]; i++)
            {
                if (Equipments.ContainsKey(i + 1))
                {
                    Equipments[i] = Equipments[i + 1];
                    Equipments[i].ID = i;
                    Equipments.Remove(i + 1);
                    if (!(await EquipmentDB.UpdateID(i + 1, i)).success)
                        return false;
                }
            }
            EquipIDs[(int) equip.ItemRarity]--;

            return true;
        }

        public async Task<bool> Remove(Skill skill)
        {
            if (!(await SkillDB.Remove(skill.ID)).success)
                return false;

            Skills.Remove(skill.ID);

            for (int i = skill.ID; i < SkillIDs[(int) skill.ItemRarity]; i++)
            {
                if (Skills.ContainsKey(i + 1))
                {
                    Skills[i] = Skills[i + 1];
                    Skills[i].ID = i;
                    Skills.Remove(i + 1);
                    if (!(await SkillDB.UpdateID(i + 1, i)).success)
                        return false;
                }
            }
            SkillIDs[(int) skill.ItemRarity]--;

            return true;
        }

        public async Task<bool> Remove(Monster monster)
        {
            if (!(await MonsterDB.Remove(monster.ID)).success)
                return false;

            Monsters.Remove(monster.ID);

            for (int i = monster.ID; i < MonsterIDs[monster.Floor - 1][(int) monster.MonsterType]; i++)
            {
                if (Monsters.ContainsKey(i + 1))
                {
                    Monsters[i] = Monsters[i + 1];
                    Monsters[i].ID = i;
                    Monsters.Remove(i + 1);
                    if (!(await MonsterDB.UpdateID(i + 1, i)).success)
                        return false;
                }
            }
            MonsterIDs[monster.Floor - 1][(int) monster.MonsterType]--;

            return true;
        }
    }
}