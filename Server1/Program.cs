class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            await Server.Start();
            // AssetCreator.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
    }
}