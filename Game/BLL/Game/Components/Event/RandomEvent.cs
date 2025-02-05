namespace BLL.Game.Components.Event
{
    public class RandomEvent : GameEvent
    {
        public GameEvent ChildEvent { get; set; } = new(Type.Camp);

        public RandomEvent() : base(Type.Random)
        {
            Name = "(?) Random Event";
        }
    }
}