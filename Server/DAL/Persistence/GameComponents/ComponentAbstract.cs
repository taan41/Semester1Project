using DAL.Persistence.ConfigClasses;

namespace DAL.Persistence.GameComponents
{
    public abstract class GameComponent
    {
        protected static GameConfig Config => ConfigManager.Instance.GameConfig;

        public virtual string Name { get; set; } = "Temp name";

        public GameComponent() {}

        public GameComponent(string name)
        {
            Name = name;
        }
    }
}