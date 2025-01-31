using BLL.GameComponents.ItemComponents;

namespace BLL.GameComponents.EventComponents
{
    public class TreasureEvent : Event
    {
        public List<Item> Treasures { get; set; } = [];

        public TreasureEvent() : base(Type.Treasure) {}

        public TreasureEvent(List<Item> treasures) : base(Type.Treasure)
            => Treasures = treasures;
    }
}