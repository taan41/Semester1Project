using System.Diagnostics;
using System.Security.Cryptography;

[Serializable]
class GameData(int? seed = null)
{
    public int Seed { get; set; } = seed.HasValue ? (int)seed & int.MaxValue : GenerateSeed();
    public Player Player { get; set; } = Player.DefaultPlayer();
    public GameProgress Progress { get; set; } = new();
    public TimeSpan SavedTime { get; set; } = new(0);

    private readonly Stopwatch _stopwatch = new();
    private TimeSpan _lastSavedTime = new(0);

    public void SetTime(bool start)
    {
        if (start) _stopwatch.Start();
        else _stopwatch.Stop();
    }

    public TimeSpan GetElapsedTime()
        => SavedTime + _stopwatch.Elapsed - _lastSavedTime;
        
    public void Save()
    {
        SavedTime += _stopwatch.Elapsed - _lastSavedTime;
        _lastSavedTime = _stopwatch.Elapsed;
        
        FileManager.SaveData(this);
    }

    public static bool Load(out GameData? loadedData)
        => FileManager.LoadData(out loadedData);

    private static int GenerateSeed()
    {
        byte[] seedBytes = new byte[4];
        RandomNumberGenerator.Fill(seedBytes);
        return BitConverter.ToInt32(seedBytes, 0) & int.MaxValue;
    }
}