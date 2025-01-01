using System.Text.Json;

using static System.Console;

class Program
{
    public static void Main()
    {
        CursorVisible = false;
        // UIHelper.Menu.Welcome();
        Game.Start();
    }

    static void JsonTest()
    {
        Monster monster = new()
        {
            ATK = 10,
            MaxHP = 10,
            HP = 10,
            MaxMP = 10,
            MP = 10
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
        Write(new string('-', UIHelper.UIConstants.UIWidth));
        ReadKey(true);

        Monster slime = new()
        {
            ATK = 1,
            MaxHP = 10,
            HP = 10,
            Name = "Slime"
        };

        Monster motherSlime = new()
        {
            ATK = 5,
            MaxHP = 50,
            HP = 40,
            Name = "Mother Slime",
            Type = MonsterType.Elite
        };
        
        Monster kingSlime = new()
        {
            ATK = 15,
            MaxHP = 100,
            HP = 60,
            Name = "King Slime",
            Type = MonsterType.Boss
        };

        Player player = new()
        {
            ATK = 15,
            MaxHP = 100,
            HP = 95,
            MaxMP = 40,
            MP = 15,
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
            UIHelper.UIMisc.DrawLine('=');
            WriteLine(" Progress: 6/16");
            WriteLine(" •  •  •  •  ●  • ");
            UIHelper.UIMisc.DrawLine('-');
            int monsterCursorTop = CursorTop;
            foreach(var monster in monsters)
                monster.Print();
            UIHelper.UIMisc.DrawLine('-');
            player.Print();
            UIHelper.UIMisc.DrawLine('-');
            int actionCursorTop = CursorTop;
            actions.ForEach(action => WriteLine($" {action}"));

            while (true)
            {
                switch(UIHelper.InteractiveUI.PickOption(actionCursorTop, actions))
                {
                    case 0:
                        break;
                    
                    case 1:
                        WriteLine();
                        UIHelper.UIMisc.DrawLine('-');
                        int skillCursorTop = CursorTop;
                        foreach(var skill in skills)
                            skill.Print();
                        UIHelper.InteractiveUI.PickComponent(skillCursorTop, skills);
                        break;

                    default:
                        continue;
                }
                
                UIHelper.InteractiveUI.PickComponent(monsterCursorTop, monsters);
                break;
            }
        }
    }
}