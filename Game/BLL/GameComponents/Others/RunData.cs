using System.Diagnostics;
using System.Security.Cryptography;
using BLL.GameComponents.EntityComponents;

namespace BLL.GameComponents.Others
{
    [Serializable]
    public class RunData
    {
        public int RunID { get; set; } = DateTime.Now.GetHashCode();
        public int Seed { get; set; } = 42;
        public Player Player { get; set; } = Player.DefaultPlayer();
        public RunProgress Progress { get; set; } = new();
        public TimeSpan SavedTime { get; set; } = new(0);

        private readonly Stopwatch _stopwatch = new();
        private TimeSpan _lastSavedTime = new(0);

        public RunData() {}

        public RunData(string? seedString)
        {
            Seed = seedString != null ? seedString.GetHashCode() & int.MaxValue : GenerateSeed();
        }

        public RunData(int? seed)
        {
            Seed = seed.HasValue ? (int) seed & int.MaxValue : GenerateSeed();
        }

        public void Timer(bool start)
        {
            if (start)
                _stopwatch.Start();
            else
                _stopwatch.Stop();
        }
            
        public void SaveTime()
        {
            SavedTime += _stopwatch.Elapsed - _lastSavedTime;
            _lastSavedTime = _stopwatch.Elapsed;
        }

        public TimeSpan GetElapsedTime()
            => SavedTime + _stopwatch.Elapsed - _lastSavedTime;

        private static int GenerateSeed()
        {
            byte[] seedBytes = new byte[4];
            RandomNumberGenerator.Fill(seedBytes);
            return BitConverter.ToInt32(seedBytes, 0) & int.MaxValue;
        }
    }
}