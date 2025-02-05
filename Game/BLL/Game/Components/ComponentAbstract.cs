using BLL.Config;

namespace BLL.Game.Components
{
    public abstract class GameComponent
    {
        protected static GameConfig GameConfig => ConfigManager.Instance.GameConfig;

        public virtual string Name { get; set; } = "Temp name";

        public GameComponent() {}

        public GameComponent(string name)
        {
            Name = name;
        }
    }
}