using BLL.GameComponents.EntityComponents;
using BLL.GameComponents.EventComponents;
using BLL.GameComponents.ItemComponents;
using BLL.GameComponents.Others;
using BLL.GameHelpers;

namespace BLL.GameHandlers
{
    public class GameLoopHandler
    {
        private readonly GameSave _save;
        private readonly RunData _runData;

        private int _prefightHP, _prefightMP;
        private List<Monster> _prefightMonsters = [];

        public readonly EventGenerator Events;
        public readonly RunProgress Progress;
        public readonly Player Player;

        public GameLoopHandler(GameSave save)
        {
            _save = save;
            _runData = save.RunData;

            Events = new(_runData);
            Progress = _runData.Progress;
            Player = _runData.Player;
        }

        public GameLoopHandler(string? seed)
        {
            _runData = new RunData(seed);
            _save = new GameSave(_runData);

            Events = new EventGenerator(_runData);
            Progress = _runData.Progress;
            Player = _runData.Player;
        }

        public void Timer(bool start)
            => _runData.Timer(start);

        public TimeSpan GetElapsedTime()
            => _runData.GetElapsedTime();

        public void SaveAs(string saveName, bool saveToCloud = false)
        {
            _save.SaveAs(saveName);

            if (ServerHandler.IsConnected && saveToCloud)
            {
                _save.Name = "CloudSave";
                ServerHandler.UploadSave(_save, out _);
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
    }
}