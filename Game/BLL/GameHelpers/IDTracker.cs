using BLL.GameComponents.EntityComponents;
using BLL.GameComponents.ItemComponents;
using DAL;
using DAL.ConfigClasses;

namespace BLL.GameHelpers
{
    public class IDTracker
    {
        private static GameConfig Config => ConfigManager.Instance.GameConfig;

        public static readonly int[] Equip;
        public static readonly int[] Skill;
        public static readonly int[][] Monster;

        static IDTracker()
        {
            Equip = new int[Enum.GetValues(typeof(Item.Rarity)).Length];
            Skill = new int[Enum.GetValues(typeof(Item.Rarity)).Length];

            Monster = new int[Config.ProgressMaxFloor][];
            for (int i = 0; i < Config.ProgressMaxFloor; i++)
            {
                Monster[i] = new int[Enum.GetValues(typeof(Monster.Type)).Length];
            }

            Initialize();
        }

        public static void Initialize()
        {
            for (int i = 0; i < Enum.GetValues(typeof(Equipment.Type)).Length; i++)
            {
                Equip[i] = i * 100 + 1;
            }

            for (int i = 0; i < Enum.GetValues(typeof(Skill.Type)).Length; i++)
            {
                Skill[i] = i * 100 + 1;
            }

            for (int i = 0; i < Config.ProgressMaxFloor; i++)
            {
                for (int j = 0; j < Enum.GetValues(typeof(Monster.Type)).Length; j++)
                {
                    Monster[i][j] = i * 1000 + j * 100 + 1;
                }
            }
        }
    }
}