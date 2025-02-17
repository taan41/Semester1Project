using System.Text;
using BLL.Config;

namespace BLL.Game.Components.Others
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

        /// <summary>
        /// Move forwards to the next room
        /// </summary>
        /// <returns> False if player reached the end </returns>
        public bool Next()
        {
            if (Room == MaxRoom)
            {
                if (Floor == MaxFloor)
                    return false;
                    
                Room = 1;
                Floor++;
            }
            else
                Room++;
            
            return true;
        }

        public void Print()
        {
            StringBuilder sb = new();
            sb.Append($" Progress: {_room}/{MaxRoom} - Floor {_floor}\n");
            for (int i = 1; i <= _room; i++)
            {
                if (i == MaxRoom)
                    sb.Append(" X ");
                else if (i % 5 == 0)
                    sb.Append(" + ");
                else
                    sb.Append(" - ");
            }
            Console.WriteLine(sb);
        }

        public override string ToString()
            => $"Room {_room}/{MaxRoom} - Floor {_floor}";
    }
}