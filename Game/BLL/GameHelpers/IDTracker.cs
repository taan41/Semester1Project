using BLL.GameComponents.EntityComponents;
using BLL.GameComponents.ItemComponents;
using DAL;
using DAL.ConfigClasses;

namespace BLL.GameHelpers
{
    public class IDTracker
    {
        private static GameConfig Config => ConfigManager.Instance.GameConfig;

        public static readonly int[] EquipIDs;
        public static readonly int[] SkillIDs;
        public static readonly int[][] MonsterIDs;

        static IDTracker()
        {
            EquipIDs = new int[Enum.GetValues(typeof(Item.Rarity)).Length];
            SkillIDs = new int[Enum.GetValues(typeof(Item.Rarity)).Length];

            MonsterIDs = new int[Config.ProgressMaxFloor][];
            for (int i = 0; i < Config.ProgressMaxFloor; i++)
            {
                MonsterIDs[i] = new int[Enum.GetValues(typeof(Monster.Type)).Length];
            }

            Initialize();
        }

        public static void Initialize()
        {
            for (int i = 0; i < Enum.GetValues(typeof(Equipment.Type)).Length; i++)
            {
                EquipIDs[i] = i * 100 + 1;
            }

            for (int i = 0; i < Enum.GetValues(typeof(Skill.Type)).Length; i++)
            {
                SkillIDs[i] = i * 100 + 1;
            }

            for (int i = 0; i < Config.ProgressMaxFloor; i++)
            {
                for (int j = 0; j < Enum.GetValues(typeof(Monster.Type)).Length; j++)
                {
                    MonsterIDs[i][j] = i * 1000 + j * 100 + 1;
                }
            }
        }
    }
}