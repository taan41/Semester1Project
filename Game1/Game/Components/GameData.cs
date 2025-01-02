using System.Diagnostics;
using System.Text.Json;

[Serializable]
class GameData
{
    public const string DirPath = FileHelper.FileConstants.DirPath + @"Saves\";
    public const string DataFile = "GameData.json";

    public int Seed { get; set; } = 42;
    public GameProgress Progress { get; set; } = new();
    public Player Player { get; set; } = Player.DefaultPlayer();
    public TimeSpan SavedTime { get; set; } = new(0);

    private readonly Stopwatch _stopwatch = new();
    private readonly CancellationTokenSource _stopwatchTokenSource = new();
    private bool _showingTime = false;

    public GameData()
    {
        // _ = Task.Run(() => ShowTime(_stopwatchTokenSource.Token));
    }

    public GameData(int seed = 42)
    {
        Seed = seed;
        Progress = new();
        Player = new("Hero", 3, 25, 10, 100);
        for (int i = 0; i < 15; i++)
            Player.AddSkill(new("Heal", 0, 5, 5){ Damage = i, Rarity = (ItemRarity) (i % 4) });
        
        // _ = Task.Run(() => ShowTime(_stopwatchTokenSource.Token));
    }

    public void SetTime(bool start)
    {
        if (start)
            _stopwatch.Start();
        else
            _stopwatch.Stop();
    }

    public void ToggleShowingTime(bool? toggle = null)
    {
        toggle ??= !_showingTime;
        _showingTime = (bool) toggle;
        if (!(bool) toggle)
        {
            Console.SetCursorPosition(UIHelper.UIConstants.UIWidth - 15, 4);
            Console.Write(new string(' ', 15));
        }
    }

    public TimeSpan GetElapsedTime()
        => SavedTime + _stopwatch.Elapsed;
        
    public void Save()
    {
        SavedTime += _stopwatch.Elapsed;
        
        Directory.CreateDirectory(DirPath);
        var jsonOption = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(DirPath + DataFile, JsonSerializer.Serialize(this, jsonOption));
    }

    public static bool Load(out GameData? loadedData)
    {
        loadedData = null;
        if (!File.Exists(DirPath + DataFile))
            return false;

        loadedData = JsonSerializer.Deserialize<GameData>(File.ReadAllText(DirPath + DataFile));
        return true;
    }

    private async Task ShowTime(CancellationToken stopToken)
    {
        while (!stopToken.IsCancellationRequested)
        {
            if (_showingTime)
            {
                string time = $"{SavedTime + _stopwatch.Elapsed:hh\\:mm\\:ss}";
                Console.SetCursorPosition(UIHelper.UIConstants.UIWidth - time.Length, 3);
                Console.Write(time);
            }

            await Task.Delay(1000, CancellationToken.None);
        }
    }
}