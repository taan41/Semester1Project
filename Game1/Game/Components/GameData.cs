using System.Diagnostics;

[Serializable]
class GameData
{
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
            Player.AddSkill(new(GameAssets.SkillList.ElementAt(0)){ Damage = i, Rarity = (ItemRarity) (i % 4) });
        
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