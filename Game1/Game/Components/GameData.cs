using System.Diagnostics;
using System.Text.Json;

[Serializable]
class GameData
{
    public const string DirPath = FileHelper.FileConstants.DirPath + @"Saves\";
    public const string DataFile = "GameData.json";

    private static readonly JsonSerializerOptions jsonOption = new(){ WriteIndented = true };

    public int? Seed { get; set; } = null;
    public GameProgress Progress { get; set; } = new();
    public Player Player { get; set; } = Player.DefaultPlayer();
    public TimeSpan SavedTime { get; set; } = new(0);

    private readonly Stopwatch _stopwatch = new();
    private readonly CancellationTokenSource _stopwatchTokenSource = new();

    private TimeSpan _lastSavedTime = new(0);
    private bool _showingTime = false;

    public GameData() {}

    public GameData(int? seed)
    {
        Seed = seed;
        // _ = Task.Run(() => ShowTime(_stopwatchTokenSource.Token));
    }

    public void SetTime(bool start)
    {
        if (start) _stopwatch.Start();
        else _stopwatch.Stop();
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
        => SavedTime + _stopwatch.Elapsed - _lastSavedTime;
        
    public void Save()
    {
        SavedTime += _stopwatch.Elapsed - _lastSavedTime;
        _lastSavedTime = _stopwatch.Elapsed;
        
        Directory.CreateDirectory(DirPath);
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
                string time = $"{SavedTime + _stopwatch.Elapsed - _lastSavedTime:hh\\:mm\\:ss}";
                Console.SetCursorPosition(UIHelper.UIConstants.UIWidth - time.Length, 3);
                Console.Write(time);
            }

            await Task.Delay(1000, CancellationToken.None);
        }
    }
}