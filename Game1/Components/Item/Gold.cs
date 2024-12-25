[Serializable]
class Gold : Item
{
    public int Amount { get; set; }

    public Gold(int amount)
    {
        Amount = amount;
        Name = "Gold";
    }
}