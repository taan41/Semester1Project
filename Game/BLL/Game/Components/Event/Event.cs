namespace BLL.Game.Components.Event
{
    public class GameEvent : GameComponent
    {
        public enum Type
        {
            Fight, Shop, Camp, Treasure, Random
        }

        public Type EventType { get; set; }

        public GameEvent() {}

        public GameEvent(Type type) : base(type.ToString())
        {
            EventType = type;
        }
    }
}