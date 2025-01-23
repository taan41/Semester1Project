namespace BLL.GameComponents.EventComponents
{
    public class Event : ComponentAbstract
    {
        public enum Type
        {
            Fight, Shop, Camp, Treasure
        }

        public Type EventType { get; set; }

        public Event() {}

        public Event(Type type) : base(type.ToString())
        {
            EventType = type;
        }
    }
}