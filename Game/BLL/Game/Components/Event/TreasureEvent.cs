using BLL.Game.Components.Item;

namespace BLL.Game.Components.Event
{
    public class TreasureEvent : GameEvent
    {
        public List<Item.GameItem> Treasures { get; set; } = [];

        public TreasureEvent() : base(Type.Treasure) {}

        public TreasureEvent(List<Item.GameItem> treasures) : base(Type.Treasure)
            => Treasures = treasures;
    }
}