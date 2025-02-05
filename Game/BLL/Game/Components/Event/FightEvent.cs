using BLL.Game.Components.Item;
using BLL.Game.Components.Entity;

namespace BLL.Game.Components.Event
{
    public class FightEvent : GameEvent
    {
        public List<Monster> Monsters { get; set; } = [];
        public List<GameItem> Rewards { get; set; } = [];

        public FightEvent() : base(Type.Fight) {}

        public FightEvent(List<Monster> monsters, List<GameItem> rewards) : base(Type.Fight)
        {
            Monsters.AddRange(monsters);
            Rewards.AddRange(rewards);

            Monster.Type maxType = Monster.Type.Normal;

            Monsters.ForEach(monster =>
            {
                if (monster.MonsterType > maxType)
                    maxType = monster.MonsterType;
            });

            Name = maxType switch
            {
                Monster.Type.Boss => "(!!!) Boss Fight",
                Monster.Type.Elite => "(!) Elite Fight",
                _ => "Normal Fight"
            };
        }
    }
}