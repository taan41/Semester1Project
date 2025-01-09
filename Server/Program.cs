class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Server.Start();
            // AssetCreator.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
    }
}