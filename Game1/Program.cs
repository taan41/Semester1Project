using System.Text.Json;

using static System.Console;

class Game
{
    public static void Main()
    {
        CursorVisible = false;
        UIHandler.Menu.Welcome();
    }

    static void JsonTest()
    {
        Monster monster = new()
        {
            Attack = 10,
            MaxHealth = 10,
            Health = 10,
            MaxMana = 10,
            Mana = 10
        };

        WriteLine(JsonSerializer.Serialize(monster));
    }
    
    static void UIDemo()
    {
        CursorVisible = false;
        Clear();
        WriteLine("|  1  ---------------");
        for(int i = 2; i < 20; i++)
            WriteLine($"|  {i}");

        WriteLine("|  Fix console's size until all lines are straight and fully visible");
        Write(new string('-', UIHandler.Numbers.UIWidth));
        ReadKey(true);

        Monster slime = new()
        {
            Attack = 1,
            MaxHealth = 10,
            Health = 10,
            Name = "Slime"
        };

        Monster motherSlime = new()
        {
            Attack = 5,
            MaxHealth = 50,
            Health = 40,
            Name = "Mother Slime",
            Type = MonsterType.Elite
        };
        
        Monster kingSlime = new()
        {
            Attack = 15,
            MaxHealth = 100,
            Health = 60,
            Name = "King Slime",
            Type = MonsterType.Boss
        };

        Player player = new()
        {
            Attack = 15,
            MaxHealth = 100,
            Health = 95,
            MaxMana = 40,
            Mana = 15,
            Name = "Hero"
        };

        Skill fireBall = new()
        {
            Damage = 20,
            MPCost = 10,
            Name = "Fire Ball"
        };

        Skill vampiricSlash = new()
        {
            Damage = 20,
            Heal = 10,
            MPCost = 10,
            Rarity = ItemRarity.Rare,
            Name = "Vampiric Slash"
        };

        Skill heal = new()
        {
            Heal = 25,
            MPCost = 10,
            Rarity = ItemRarity.Legendary,
            Name = "Super Heal"
        };

        List<Monster> monsters = [slime, slime, motherSlime, kingSlime];
        List<string> actions = ["Attack", "Skills"];
        List<Skill>skills = [fireBall, vampiricSlash, heal];

        while (true)
        {
            Clear();
            WriteLine("GAME".PadLeft(32).PadRight(28));
            UIHandler.Misc.DrawLine('=');
            WriteLine(" Progress: 6/16");
            WriteLine(" •  •  •  •  ●  • ");
            UIHandler.Misc.DrawLine('-');
            int monsterCursorTop = CursorTop;
            foreach(var monster in monsters)
                monster.Print();
            UIHandler.Misc.DrawLine('-');
            player.Print();
            UIHandler.Misc.DrawLine('-');
            int actionCursorTop = CursorTop;
            actions.ForEach(action => WriteLine($" {action}"));

            while (true)
            {
                switch(UIHandler.Misc.PickOption(actionCursorTop, actions))
                {
                    case 0:
                        break;
                    
                    case 1:
                        WriteLine();
                        UIHandler.Misc.DrawLine('-');
                        int skillCursorTop = CursorTop;
                        foreach(var skill in skills)
                            skill.Print();
                        UIHandler.Misc.PickComponent(skillCursorTop, skills);
                        break;

                    default:
                        continue;
                }
                
                UIHandler.Misc.PickComponent(monsterCursorTop, monsters);
                break;
            }
        }
    }
}