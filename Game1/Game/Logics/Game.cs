using System.Security.Cryptography;
using static UIHelper;

class Game
{
    public static void Start()
    {
        List<string> welcomeOptions = ["PLAY ONLINE", "PLAY OFFLINE", "EXIT"];
        while(true)
        {
            var (cursorLeft, cursorTop, animTokenSource) = GameUI.WelcomeScreen(welcomeOptions);
            Console.ReadKey(true);
            
            switch(InteractiveUI.PickOption(cursorLeft, cursorTop, welcomeOptions))
            {
                case 0:
                case 1:
                    animTokenSource.Cancel();
                    StartGame();
                    continue;
                
                case 2: case null:
                    animTokenSource.Cancel();
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
        
        int? pickedEquipInd = InteractiveUI.PickComponent(cursorLeft, cursorTop, starters);
        if (pickedEquipInd == null)
            return;

        Equipment pickedEquip = starters[(int) pickedEquipInd];

        gameData.Player.Equip(new(pickedEquip));
        gameData.Progress.Next();
        GameLoop(gameData);
    }

    private static void GameLoop(GameData gameData)
    {
        EventManager eventManager = new(gameData);
        List<string> inventoryOptions = ["Change equipment", "Change skill"];
        bool pickFromEnd = false;

        while (true)
        {
            List<Event> routes = eventManager.GetEvents();
            var (routeCurTop, invCurTop) = GameUI.PickRouteScreen(gameData, routes, inventoryOptions);

            int? pickedRouteInd = InteractiveUI.PickComponent(routeCurTop, routes, false, true, pickFromEnd);
            pickFromEnd = false;
            if (pickedRouteInd == null)
                return;

            if (pickedRouteInd == routes.Count) // Move down to inventory options
            {
                int? pickedInvInd = InteractiveUI.PickOption(invCurTop, inventoryOptions, true);
                switch (pickedInvInd)
                {
                    case null:
                        return;

                    case -1:
                        pickFromEnd = true;
                        continue;

                    case 0:
                        HandleInventory(gameData, gameData.Player.EquipInventory, gameData.Player.GetEquipped());
                        continue;

                    case 1:
                        HandleInventory(gameData, gameData.Player.SkillInventory, gameData.Player.Skills);
                        continue;

                    default:
                        continue;
                }
            }

            Event pickedRoute = routes[(int) pickedRouteInd];

            if (pickedRoute is FightEvent fightEvent)
                HandleFightEvent(gameData, fightEvent);

            else switch (pickedRoute.Type)
            {
                case EventType.Camp:
                    gameData.Player.Regenerate();
                    break;
            }
            
            gameData.Progress.Next();
        }
    }

    private static void HandleInventory<T>(GameData gameData, List<T> inv, List<T> equipped) where T : Item
    {
        int displayCount = 5;
        int startInd = 0, endInd = inv.Count > displayCount ? displayCount : inv.Count;
        bool pickFromEnd = false;
        while(true)
        {
            var (invCurTop, equippedCurTop) = GameUI.PickInventoryScreen(gameData, inv[startInd..endInd], equipped);
            int? pickedEquipInd = InteractiveUI.PickComponent(invCurTop, inv[startInd..endInd], startInd > 0, endInd < inv.Count, pickFromEnd);

            if (pickedEquipInd == null)
                return;

            if (pickedEquipInd == -1 && startInd > 0)
            {
                pickFromEnd = false;
                startInd--;
                endInd--;
                continue;
            }

            if (pickedEquipInd == displayCount && endInd < inv.Count)
            {
                pickFromEnd = true;
                startInd++;
                endInd++;
                continue;
            }

            Item pickedItem = inv[startInd + (int) pickedEquipInd];
            if (pickedItem is Equipment pickedEquip)
                gameData.Player.Equip(pickedEquip);
            else if (pickedItem is Skill pickedSkill)
            {
                if (equipped.Count < 3)
                {
                    gameData.Player.AddSkill(pickedSkill);
                    continue;
                }

                int? equippedSkillInd = InteractiveUI.PickComponent(equippedCurTop, equipped);
                if (equippedSkillInd != null)
                    gameData.Player.ChangeSkill((int) equippedSkillInd, pickedSkill);

            }
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
                    int? pickedMonsterInd = InteractiveUI.PickComponent(monsterCurTop, fightEvent.Monsters);
                    if (pickedMonsterInd == null)
                        continue;
                    
                    Monster pickedMonster = fightEvent.Monsters[(int) pickedMonsterInd];
                    
                    gameData.Player.Attack(pickedMonster);
                    break;

                case 1:
                    var (skillCurTop, endCurTop) = GameUI.SkillFightScreen(gameData, fightEvent);
                    int? pickedSkillInd = InteractiveUI.PickComponent(skillCurTop, gameData.Player.Skills);
                    if (pickedSkillInd == null)
                        continue;

                    Skill pickedSkill = gameData.Player.Skills[(int) pickedSkillInd];

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
                                pickedMonsterInd = InteractiveUI.PickComponent(monsterCurTop, fightEvent.Monsters);
                                if (pickedMonsterInd == null)
                                    continue;

                                pickedMonster = fightEvent.Monsters[(int) pickedMonsterInd];
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
            int? pickedRewardInd = InteractiveUI.PickComponent(rewardCurTop, fightEvent.Rewards);
            if (pickedRewardInd == null)
                return;
            
            Item pickedReward = fightEvent.Rewards[(int) pickedRewardInd];

            gameData.Player.AddItem(pickedReward);
            fightEvent.Rewards.Remove(pickedReward);
        }

    }
}