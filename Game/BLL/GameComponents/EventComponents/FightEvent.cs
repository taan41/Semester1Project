using BLL.GameComponents.ItemComponents;
using BLL.GameComponents.EntityComponents;

namespace BLL.GameComponents.EventComponents
{
    public class FightEvent : Event
    {
        public List<Monster> Monsters { get; set; } = [];
        public List<Item> Rewards { get; set; } = [];

        public FightEvent() : base(Type.Fight) {}

        public FightEvent(List<Monster> monsters, List<Item> rewards) : base(Type.Fight)
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