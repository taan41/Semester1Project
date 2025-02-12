using BLL.Game.Components.Item;

namespace BLL.Game.Components.Event
{
    public class TreasureEvent : GameEvent
    {
        public List<GameItem> Treasures { get; set; } = [];

        public TreasureEvent() : base(Type.Treasure) {}

        public TreasureEvent(List<GameItem> treasures) : base(Type.Treasure)
            => Treasures = treasures;
    }
}