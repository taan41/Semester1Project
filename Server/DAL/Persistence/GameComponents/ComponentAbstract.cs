using DAL.Persistence.ConfigClasses;

namespace DAL.Persistence.GameComponents
{
    public abstract class ComponentAbstract
    {
        protected static GameConfig Config => ConfigManager.Instance.GameConfig;

        public virtual string Name { get; set; } = "Temp name";

        public ComponentAbstract() {}

        public ComponentAbstract(string name)
        {
            Name = name;
        }
    }
}