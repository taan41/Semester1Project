using BLL.GameComponents.ItemComponents;

namespace BLL.GameComponents.EventComponents
{
    public class ShopEvent : Event
    {
        public List<Item> SellingItems { get; set; } = [];

        public ShopEvent() : base(Type.Shop) {}

        public ShopEvent(List<Item> sellingItems) : base(Type.Shop)
            => SellingItems = sellingItems;
    }
}