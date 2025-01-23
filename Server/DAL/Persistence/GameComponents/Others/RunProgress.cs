using DAL;
using DAL.Persistence.ConfigClasses;

namespace DAL.Persistence.GameComponents.Others
{
    [Serializable]
    public class RunProgress
    {
        private static GameConfig Config => ConfigManager.Instance.GameConfig;

        private static int MaxFloor => Config.ProgressMaxFloor;
        private static int MaxRoom => Config.ProgressMaxRoom;

        private int _floor = 1;
        private int _room = 0;

        public int Floor
        {
            get => _floor;
            set
            {
                if (value < 0) value = 0;
                if (value > MaxFloor) value = MaxFloor;
                _floor = value;
            }
        }

        public int Room
        {
            get => _room;
            set
            {
                if (value < 0) value = 0;
                if (value > MaxRoom) value = MaxRoom;
                _room = value;
            }
        }
    }
}