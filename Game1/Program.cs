using System.Text.Json;
using static System.Console;

class Program
{
    public static void Main()
    {
        try
        {
            CursorVisible = false;
            Game.Start();
            // AssetCreator.Start();
            // TestGameSavePrint();
        }
        catch (Exception ex)
        {
            WriteLine(ex);
        }
    }

    static void TestGameSavePrint()
    {
        // FileManager.LoadSaves(out List<GameSave> saves, out _);

        // foreach (var save in saves)
        //     save.Print();

        GameSave save = new(new GameData(), "Test Save");
        save.Print();

        File.WriteAllText("test.json", JsonSerializer.Serialize(save, new JsonSerializerOptions(){ WriteIndented = true }));

        GameSave? testSave = JsonSerializer.Deserialize<GameSave>(File.ReadAllText("test.json"));

        testSave?.Print();
    }
}