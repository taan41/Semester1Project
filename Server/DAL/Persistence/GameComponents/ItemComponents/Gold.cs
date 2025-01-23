using System.Text.Json.Serialization;

namespace DAL.Persistence.GameComponents.ItemComponents
{
    [Serializable]
    public class Gold : Item
    {
        [JsonIgnore]
        public override string Name { get; set; } = "Gold";
        [JsonIgnore]
        public override int ID { get; set; } = -1;
        [JsonIgnore]
        public override Rarity ItemRarity { get; set; } = Rarity.Common;
        [JsonIgnore]
        public override int Price { get; set; } = 0;

        public int Quantity { get; set; } = 0;

        public Gold() {}

        public Gold(int quantity = 0)
        {
            Quantity = quantity;
        }
    }
}