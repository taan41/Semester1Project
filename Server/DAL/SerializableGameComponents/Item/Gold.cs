using System.Text.Json;
using System.Text.Json.Serialization;

[Serializable]
class Gold : Item
{
    [JsonIgnore]
    public override string Name { get => base.Name; set => base.Name = value; }
    [JsonIgnore]
    public override int ID { get => base.ID; set => base.ID = value; }
    [JsonIgnore]
    public override ItemRarity Rarity { get => base.Rarity; set => base.Rarity = value; }
    [JsonIgnore]
    public override int Price { get => base.Price; set => base.Price = value; }

    public int Quantity { get; set; } = 0;

    public Gold(int quantity = 0)
    {
        Quantity = quantity;
        Name = "Gold";
    }
    public override string ToJson()
        => JsonSerializer.Serialize(this);
}