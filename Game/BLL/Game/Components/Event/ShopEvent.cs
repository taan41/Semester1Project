using BLL.Game.Components.Item;

namespace BLL.Game.Components.Event
{
    public class ShopEvent : GameEvent
    {
        public List<Item.GameItem> SellingItems { get; set; } = [];

        public ShopEvent() : base(Type.Shop) {}

        public ShopEvent(List<Item.GameItem> sellingItems) : base(Type.Shop)
            => SellingItems = sellingItems;
    }
}