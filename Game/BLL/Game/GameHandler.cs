using BLL.Game.Components.Entity;
using BLL.Game.Components.Event;
using BLL.Game.Components.Item;
using BLL.Game.Components.Others;
using BLL.Server;

namespace BLL.Game
{
    public class GameHandler
    {
        private static ServerHandler ServerHandler => ServerHandler.Instance;
        
        private readonly RunData _runData;
        private readonly EventGenerator _events;
        private readonly List<Monster> _prefightMonsters = [];
        private int _prefightHP, _prefightMP;

        public readonly RunProgress Progress;
        public readonly Player Player;

        public int RerollCost => Progress.Floor * 100;

        public GameHandler(GameSave save)
        {
            _runData = save.RunData;

            _events = new(_runData.Seed);
            Progress = _runData.Progress;
            Player = _runData.Player;
        }

        public GameHandler(string? seed)
        {
            _runData = new RunData(seed);

            _events = new EventGenerator(_runData.Seed);
            Progress = _runData.Progress;
            Player = _runData.Player;
        }

        public List<GameEvent> GetEvents()
            => _events.GetRoomEvents(Progress);

        public void Timer(bool start)
            => _runData.TimerControl(start);

        public TimeSpan GetElapsedTime()
            => _runData.GetElapsedTime();

        public void SaveAs(string saveName, bool saveToServer = false)
        {
            GameSave save = new(_runData, saveName);
            GameSave.SaveLocal(save);

            if (saveToServer && ServerHandler.IsConnected)
            {
                save.Name = "CloudSave";
                ServerHandler.UploadSave(save, out _);
            }
        }

        public void AddStarters(Equipment equip, Skill skill)
        {
            Player.AddItem(equip);
            Player.AddItem(skill);
        }

        public void SavePrefightState(FightEvent fightEvent)
        {
            _prefightHP = Player.HP;
            _prefightMP = Player.MP;
            foreach (var monster in fightEvent.Monsters)
                _prefightMonsters.Add(new(monster));
        }

        public void LoadPrefightState(FightEvent fightEvent)
        {
            Player.SetStats(null, null, null, _prefightHP, null, _prefightMP, null);
            fightEvent.Monsters.Clear();
            fightEvent.Monsters.AddRange(_prefightMonsters);
            _prefightMonsters.Clear();
        }

        public void RunWin()
        {
            Timer(false);
            if (ServerHandler.IsLoggedIn)
                ServerHandler.UploadScore(_runData.RunID, GetElapsedTime(), out _);
        }

        public void RerollShop(ShopEvent shop)
        {
            _events.RerollShop(Progress, shop);
            Player.Gold.Quantity -= RerollCost;
        }
    }
}