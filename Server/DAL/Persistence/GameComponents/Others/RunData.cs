using DAL.Persistence.GameComponents.EntityComponents;

namespace DAL.Persistence.GameComponents.Others
{
    [Serializable]
    public class RunData
    {
        public int Seed { get; set; } = 42;
        public Player Player { get; set; } = Player.DefaultPlayer();
        public RunProgress Progress { get; set; } = new();
        public TimeSpan SavedTime { get; set; } = new(0);

        public RunData() {}
    }
}