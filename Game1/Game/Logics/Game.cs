using System.Security.Cryptography;
using static UIHelper;

class Game
{
    public static void Start()
    {
        List<string> welcomeOptions = ["PLAY ONLINE", "PLAY OFFLINE", "EXIT"];
        while(true)
        {
            var (cursorLeft, cursorTop) = GameUI.WelcomeScreen(welcomeOptions);
            
            switch(InteractiveUI.PickOption(cursorLeft, cursorTop, welcomeOptions))
            {
                case 0:
                case 1:
                    StartGame();
                    continue;
                
                case 2: case null:
                    return;
            }
        }
    }

    private static void StartGame()
    {
        List<string> playOptions = ["NEW GAME", "LOAD GAME", "RETURN"];
        var (cursorLeft, cursorTop) = GameUI.StartGameScreen(playOptions);
        
        switch(InteractiveUI.PickOption(cursorLeft, cursorTop, playOptions))
        {
            case 0:
                NewGame();
                break;

            case 1:
                StartGame();
                break;
            
            case 2: case null:
                return;
        }
    }

    private static void NewGame()
    {
        GameData gameData = new();
        List<Equipment> starters =
        [
            GameAssets.GetEquipment("Starter Sword"),
            GameAssets.GetEquipment("Starter Bow"),
            GameAssets.GetEquipment("Starter Staff")
        ];

        var (cursorLeft, cursorTop) = GameUI.PickComponentScreen(gameData, starters, " Choose one starter item:");
        
        Equipment? pickedEquipment = InteractiveUI.PickComponent(cursorLeft, cursorTop, starters);
        if (pickedEquipment == null)
            return;

        gameData.Player.Equip(new(pickedEquipment));
        gameData.Progress.Next();
        GameLoop(gameData);
    }

    private static void GameLoop(GameData gameData)
    {
        EventManager eventManager = new(gameData.Seed);

        while (true)
        {
            List<Event> events = eventManager.GenerateEvents(gameData);
            var (routeCurLeft, routeCurTop) = GameUI.PickComponentScreen(gameData, events, " Pick a route:");

            Event? pickedEvent = InteractiveUI.PickComponent(routeCurLeft, routeCurTop, events);
            if (pickedEvent == null)
                return;

            if (pickedEvent is FightEvent fightEvent)
                HandleFightEvent(gameData, fightEvent);
            else switch (pickedEvent.Type)
            {
                case EventType.Camp:
                    gameData.Player.Regenerate();
                    break;
            }
            
            gameData.Progress.Next();
        }
    }

    private static void HandleFightEvent(GameData gameData, FightEvent fightEvent)
    {
        List<string> fightActions = ["Attack", "Skills"];

        while (fightEvent.Monsters.Count > 0)
        {
            var (monsterCurTop, actionCurTop) = GameUI.FightScreen(gameData, fightEvent, fightActions);

            switch (InteractiveUI.PickOption(actionCurTop, fightActions))
            {
                case 0:
                    Monster? pickedMonster = InteractiveUI.PickComponent(monsterCurTop, fightEvent.Monsters);
                    if (pickedMonster == null)
                        continue;
                    
                    gameData.Player.Attack(pickedMonster);
                    break;

                case 1:
                    var (skillCurTop, endCurTop) = GameUI.SkillFightScreen(gameData, fightEvent);
                    Skill? pickedSkill = InteractiveUI.PickComponent(skillCurTop, gameData.Player.Skills);
                    if (pickedSkill == null)
                        continue;

                    if (pickedSkill.MPCost > gameData.Player.MP)
                    {
                        Console.SetCursorPosition(0, endCurTop);
                        Console.Write(" Not enough MP!");
                        Console.ReadKey(true);
                        goto case 1;
                    }

                    if (pickedSkill.Damage > 0)
                    {
                        switch (pickedSkill.Type)
                        {
                            case TargetType.Single:
                                pickedMonster = InteractiveUI.PickComponent(monsterCurTop, fightEvent.Monsters);
                                if (pickedMonster == null)
                                    continue;

                                gameData.Player.UseSkill(pickedSkill, [pickedMonster]);
                                break;

                            case TargetType.Random:
                                pickedMonster = fightEvent.Monsters[RandomNumberGenerator.GetInt32(fightEvent.Monsters.Count - 1)];
                                gameData.Player.UseSkill(pickedSkill, [pickedMonster]);
                                break;

                            case TargetType.All:
                                gameData.Player.UseSkill(pickedSkill, fightEvent.Monsters);
                                break;
                        }
                    }
                    else
                        gameData.Player.UseSkill(pickedSkill, []);
                    
                    break;
                
                case null:
                    return;
            }
            
            fightEvent.Monsters.RemoveAll(monster => monster.HP == 0);
        }

        while(fightEvent.Rewards.Count > 0)
        {
            var (_, rewardCurTop) = GameUI.PickComponentScreen(gameData, fightEvent.Rewards, " Rewards:");
            Item? pickedReward = InteractiveUI.PickComponent(rewardCurTop, fightEvent.Rewards);
            if (pickedReward == null)
                return;

            gameData.Player.AddItem(pickedReward);
            fightEvent.Rewards.Remove(pickedReward);
        }

    }
}