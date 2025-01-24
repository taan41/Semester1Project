namespace BLL.GameComponents.EventComponents
{
    public class RandomEvent : Event
    {
        public Event ChildEvent { get; set; } = new(Type.Camp);

        public RandomEvent() : base(Type.Random)
        {
            Name = "(?) Random Event";
        }
    }
}