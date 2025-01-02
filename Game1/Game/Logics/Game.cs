using System.Security.Cryptography;
using static UIHelper;

class Game
{
    public static void Start()
    {
        List<string> welcomeOptions = ["PLAY ONLINE", "PLAY OFFLINE", "EXIT"];
        GameUI.WelcomeScreen(welcomeOptions, true);

        while(true)
        {
            GameUI.StartTitleAnim();

            Console.ReadKey(true);
            switch(InteractiveUI.PickOption(CursorPos.MainMenuLeft, CursorPos.MainMenuTop, welcomeOptions))
            {
                case 0:
                case 1:
                    StartGame();
                    break;
                
                case 2: case null:
                    return;
            }

            GameUI.WelcomeScreen(welcomeOptions);
        }
    }

    private static void StartGame()
    {
        List<string> playOptions = ["NEW GAME", "LOAD GAME", "RETURN"];
        GameUI.StartScreen(playOptions);

        while (true)
        {
            GameUI.StartTitleAnim();

            Console.ReadKey(true);
            switch(InteractiveUI.PickOption(CursorPos.MainMenuLeft, CursorPos.MainMenuTop, playOptions))
            {
                case 0:
                    GameUI.StopTitleAnim();
                    NewGame();
                    return;

                case 1:
                    GameUI.StopTitleAnim();
                    if (!LoadGame())
                    {
                        GameUI.StartScreen(playOptions, true);
                        continue;
                    }
                    return;
                
                case 2: case null:
                    return;
            }
        }
    }

    private static void NewGame()
    {
        AssetManager assetManager = new();

        List<Equipment> starters =
        [
            assetManager.GetEquipment("Starter Sword"),
            assetManager.GetEquipment("Starter Bow"),
            assetManager.GetEquipment("Starter Staff")
        ];
        GameData gameData = new(42);
        GameUI.GenericGameScreen(gameData);

        GameUI.PrintComponents(starters, UIConstants.MainZoneHeight, " -- Choose Starter Item:", CursorPos.MainZoneTop);
        
        int? pickedEquipInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, starters);
        if (pickedEquipInd == null)
            return;

        Equipment pickedEquip = starters[(int) pickedEquipInd];

        gameData.Player.Equip(new(pickedEquip));
        gameData.Progress.Next();
        GameLoop(gameData);
    }

    private static bool LoadGame()
    {
        if (GameData.Load(out GameData? loadedData))
        {
            if (loadedData != null)
            {
                GameLoop(loadedData);
                return true;
            }

            GameUI.WarningPopup("Corrupted Data");
        }
        else
            GameUI.WarningPopup("No Save Found");
        
        Console.ReadKey(true);
        return false;
    }

    private static void GameLoop(GameData gameData)
    {
        AssetManager assetManager = new();
        EventManager eventManager = new(gameData, assetManager);
        List<string> invOptions = ["Change Equipment", "Change Skill"];
        bool pickFromEnd = false;
        gameData.SetTime(true);

        while (true)
        {
            List<Event> routes = eventManager.GetEvents();
            
            GameUI.RouteScreen(gameData, routes, invOptions);

            int? pickedRouteInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, routes, false, true, pickFromEnd);
            pickFromEnd = false;
            if (pickedRouteInd == null)
            {
                if (HanldePause(gameData)) continue;
                else 
                {
                    gameData.Save();
                    return;
                }
            }

            if (pickedRouteInd == routes.Count) // Move down to inventory options
            {
                int? pickedInvInd = InteractiveUI.PickOption(CursorPos.SubZoneTop + 1, invOptions, true);
                switch (pickedInvInd)
                {
                    case null:
                        if (HanldePause(gameData)) continue;
                        else
                        {
                            gameData.Save();
                            return;
                        }

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
            {
                gameData.Save();
                if (!HandleFightEvent(gameData, fightEvent))
                {
                    return;
                }
            }

            else switch (pickedRoute.Type)
            {
                case EventType.Camp:
                    gameData.Player.Regenerate();
                    break;
            }
            
            gameData.Progress.Next();
        }
    }

    // return false if quit
    private static bool HanldePause(GameData gameData)
    {
        List<string> pauseOptions = ["Resume", "Save & Exit"];

        gameData.SetTime(false);
        GameUI.PausePopup(pauseOptions, gameData.GetElapsedTime());

        int? optionInd = InteractiveUI.PickOption(CursorPos.PauseOptionLeft, CursorPos.PauseOptionTop, pauseOptions);
        switch(optionInd)
        {
            case 1:
                return false;

            case 0: default:
                gameData.SetTime(true);
                return true;
        }
    }

    private static void HandleInventory<T>(GameData gameData, List<T> inv, List<T> equipped) where T : Item
    {
        int displayCount = UIConstants.MainZoneHeight - 1;
        int startInd = 0, endInd = inv.Count > displayCount ? displayCount : inv.Count;
        bool pickFromEnd = false;

        while(true)
        {
            GameUI.InventoryScreen(gameData, inv[startInd..endInd], equipped);

            int? pickedEquipInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, inv[startInd..endInd], startInd > 0, endInd < inv.Count, pickFromEnd);

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
            {
                gameData.Player.Equip(pickedEquip);
            }
            else if (pickedItem is Skill pickedSkill)
            {
                if (equipped.Count < 3)
                {
                    gameData.Player.AddSkill(pickedSkill);
                    continue;
                }

                int? equippedSkillInd = InteractiveUI.PickComponent(CursorPos.SubZoneTop + 1, equipped);
                if (equippedSkillInd != null)
                    gameData.Player.ChangeSkill((int) equippedSkillInd, pickedSkill);

            }
        }
    }

    // Return false if quit mid-fight
    private static bool HandleFightEvent(GameData gameData, FightEvent fightEvent)
    {
        List<string> fightActions = ["Attack", "Use Skill"];

        while (fightEvent.Monsters.Count > 0)
        {
            GameUI.FightScreen(gameData, fightEvent.Monsters, fightActions);

            switch (InteractiveUI.PickOption(CursorPos.SubZoneTop + 1, fightActions))
            {
                case 0:
                    int? pickedMonsterInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop, fightEvent.Monsters);
                    if (pickedMonsterInd == null)
                        continue;
                    
                    Monster pickedMonster = fightEvent.Monsters[(int) pickedMonsterInd];
                    
                    gameData.Player.Attack(pickedMonster);
                    break;

                case 1:
                    GameUI.FightSkillScreen(gameData, fightEvent.Monsters, gameData.Player.Skills);
                    int? pickedSkillInd = InteractiveUI.PickComponent(CursorPos.SubZoneTop, gameData.Player.Skills);
                    if (pickedSkillInd == null)
                        continue;

                    Skill pickedSkill = gameData.Player.Skills[(int) pickedSkillInd];

                    if (pickedSkill.MPCost > gameData.Player.MP)
                    {
                        Console.SetCursorPosition(0, CursorPos.SubZoneTop + 3);
                        Console.Write(" Not enough MP!");
                        Console.ReadKey(true);
                        goto case 1;
                    }

                    if (pickedSkill.Damage > 0)
                    {
                        switch (pickedSkill.Type)
                        {
                            case TargetType.Single:
                                pickedMonsterInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop, fightEvent.Monsters);
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
                    if (HanldePause(gameData)) continue;
                    else return false;
            }
            
            fightEvent.Monsters.RemoveAll(monster => monster.HP == 0);
        }

        while(fightEvent.Rewards.Count > 0)
        {
            GameUI.RewardScreen(gameData, fightEvent.Rewards);
            int? pickedRewardInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, fightEvent.Rewards);
            if (pickedRewardInd == null)
                if (HanldePause(gameData)) continue;
                else return false;
            
            Item pickedReward = fightEvent.Rewards[(int) pickedRewardInd];

            gameData.Player.AddItem(pickedReward);
            fightEvent.Rewards.Remove(pickedReward);
        }

        return true;
    }
}