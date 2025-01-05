class ShopEvent : Event
{
    public List<Item> SellingItems { get; set; } = [];

    public ShopEvent() {}

    public ShopEvent(List<Item> sellingItems) : base(EventType.Shop)
        => SellingItems = sellingItems;
}