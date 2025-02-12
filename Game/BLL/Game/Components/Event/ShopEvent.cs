using BLL.Game.Components.Item;

namespace BLL.Game.Components.Event
{
    public class ShopEvent : GameEvent
    {
        public List<GameItem> SellingItems { get; set; } = [];

        public ShopEvent() : base(Type.Shop) {}

        public ShopEvent(List<GameItem> sellingItems) : base(Type.Shop)
            => SellingItems = sellingItems;
    }
}