namespace DAL.Persistence.GameComponents.Others
{
    [Serializable]
    public class GameSave : ComponentAbstract
    {
        public DateTime SaveTime { get; protected set; } = DateTime.Now;
        public RunData RunData { get; protected set; } = new();

        public GameSave() {}
    }
}