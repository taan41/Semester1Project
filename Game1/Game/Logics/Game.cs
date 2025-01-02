using System.Security.Cryptography;
using static UIHelper;

class Game
{
    public static void Start()
    {
        bool clear = true;
        List<string> welcomeOptions = ["PLAY ONLINE", "PLAY OFFLINE", "EXIT"];

        while(true)
        {
            GameUI.WelcomeScreen(welcomeOptions, clear);
            GameUI.StartTitleAnim();

            Console.ReadKey(true);
            switch(InteractiveUI.PickAction(CursorPos.MainMenuLeft, CursorPos.MainMenuTop, welcomeOptions))
            {
                case 0:
                case 1:
                    clear = StartGame();
                    break;
                
                case 2: case null:
                    return;
            }
        }
    }

    // Return false if return without starting a game
    private static bool StartGame()
    {
        List<string> playOptions = ["NEW GAME", "LOAD GAME", "RETURN"];
        GameUI.StartScreen(playOptions);

        while (true)
        {
            GameUI.StartTitleAnim();

            Console.ReadKey(true);
            switch(InteractiveUI.PickAction(CursorPos.MainMenuLeft, CursorPos.MainMenuTop, playOptions))
            {
                case 0:
                    GameUI.StopTitleAnim();
                    NewGame();
                    return true;

                case 1:
                    GameUI.StopTitleAnim();
                    if (!LoadGame())
                    {
                        GameUI.StartScreen(playOptions, true);
                        continue;
                    }
                    return true;
                
                case 2: case null:
                    return false;
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
                int? pickedInvInd = InteractiveUI.PickAction(CursorPos.SubZoneTop + 1, invOptions, true);
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
                        HandleEquipInventory(gameData);
                        continue;

                    case 1:
                        HandleSkillInventory(gameData);
                        continue;

                    default:
                        continue;
                }
            }

            Event pickedRoute = routes[(int) pickedRouteInd];

            if (pickedRoute is FightEvent fightEvent)
            {
                if (!HandleFightEvent(gameData, fightEvent))
                    return;
            }
            else if (pickedRoute is ShopEvent shopEvent)
                HandleShop(gameData, shopEvent);

            else switch (pickedRoute.Type)
            {
                case EventType.Camp:
                    gameData.Player.Regenerate();
                    GameUI.CampfireScreen(gameData);
                    break;

                case EventType.Treasure:
                    gameData.Save();
                    GameUI.TreasureOpening(gameData);
                    if (!HandleRewards(gameData, [new Gold(1000)]))
                        return;
                    break;
            }
            
            gameData.Progress.Next();
            gameData.Save();
        }
    }

    // return false if quit
    private static bool HanldePause(GameData gameData)
    {
        List<string> pauseOptions = ["Resume", "Save & Exit"];

        gameData.SetTime(false);
        GameUI.PausePopup(pauseOptions, gameData.GetElapsedTime());

        int? optionInd = InteractiveUI.PickAction(CursorPos.PauseOptionLeft, CursorPos.PauseOptionTop, pauseOptions);
        switch(optionInd)
        {
            case 1:
                return false;

            case 0: default:
                gameData.SetTime(true);
                return true;
        }
    }

    private static void HandleEquipInventory(GameData gameData)
    {
        List<Equipment> equipInv = gameData.Player.EquipInventory;
        int displayCount = UIConstants.MainZoneHeight - 1;
        int startInd = 0, endInd = equipInv.Count > displayCount ? displayCount : equipInv.Count;
        bool pickFromEnd = false;

        while(true)
        {
            GameUI.InventoryScreen(gameData, equipInv[startInd..endInd], gameData.Player.GetEquipped());

            int? pickedEquipInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, equipInv[startInd..endInd], startInd > 0, endInd < equipInv.Count, pickFromEnd);

            if (pickedEquipInd == null)
                return;

            if (pickedEquipInd == -1 && startInd > 0)
            {
                pickFromEnd = false;
                startInd--;
                endInd--;
                continue;
            }

            if (pickedEquipInd == displayCount && endInd < equipInv.Count)
            {
                pickFromEnd = true;
                startInd++;
                endInd++;
                continue;
            }

            Equipment pickedEquip = equipInv[startInd + (int) pickedEquipInd];
            gameData.Player.Equip(pickedEquip);
        }
    }

    private static void HandleSkillInventory(GameData gameData)
    {
        List<Skill> skillInv = gameData.Player.SkillInventory;
        int displayCount = UIConstants.MainZoneHeight - 1;
        int startInd = 0, endInd = skillInv.Count > displayCount ? displayCount : skillInv.Count;
        bool pickFromEnd = false;

        while(true)
        {
            GameUI.InventoryScreen(gameData, skillInv[startInd..endInd], gameData.Player.Skills);

            int? pickedEquipInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, skillInv[startInd..endInd], startInd > 0, endInd < skillInv.Count, pickFromEnd);

            if (pickedEquipInd == null)
                return;

            if (pickedEquipInd == -1 && startInd > 0)
            {
                pickFromEnd = false;
                startInd--;
                endInd--;
                continue;
            }

            if (pickedEquipInd == displayCount && endInd < skillInv.Count)
            {
                pickFromEnd = true;
                startInd++;
                endInd++;
                continue;
            }

            Skill pickedSkill = skillInv[startInd + (int) pickedEquipInd];

            if (gameData.Player.Skills.Count < 3)
            {
                gameData.Player.AddSkill(pickedSkill);
                continue;
            }

            int? equippedSkillInd = InteractiveUI.PickComponent(CursorPos.SubZoneTop + 1, gameData.Player.Skills);
            if (equippedSkillInd != null)
                gameData.Player.ChangeSkill((int) equippedSkillInd, pickedSkill);
        }
    }

    // Return false if quit mid-fight
    private static bool HandleFightEvent(GameData gameData, FightEvent fightEvent)
    {
        List<string> fightActions = ["Attack", "Use Skill"];

        while (fightEvent.Monsters.Count > 0)
        {
            GameUI.FightScreen(gameData, fightEvent.Monsters, fightActions);

            switch (InteractiveUI.PickAction(CursorPos.SubZoneTop + 1, fightActions))
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

        return HandleRewards(gameData, fightEvent.Rewards);
    }

    // Return false if quit during picking rewards
    private static bool HandleRewards(GameData gameData, List<Item> rewards)
    {
        while(rewards.Count > 0)
        {
            GameUI.RewardScreen(gameData, rewards);
            int? pickedRewardInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, rewards);
            if (pickedRewardInd == null)
                if (HanldePause(gameData)) continue;
                else return false;
            
            Item pickedReward = rewards[(int) pickedRewardInd];

            gameData.Player.AddItem(pickedReward);
            rewards.Remove(pickedReward);
        }

        return true;
    }

    private static void HandleShop(GameData gameData, ShopEvent shopEvent)
    {
        List<string> shopActions = ["Buy", "Sell", "Inventory"];
        List<string> invActions = ["Change Equipment", "Change Skill"];

        while (true)
        {
            GameUI.ShopMainScreen(gameData, shopActions);
            
            switch (InteractiveUI.PickAction(CursorPos.SubZoneTop, shopActions))
            {
                case null:
                    return;
                    
                case 0:
                    HandleShopBuy(gameData, shopEvent);
                    break;

                case 1:
                    HandleShopSell(gameData);
                    break;

                case 2:
                    GameUI.ShopMainScreen(gameData, invActions);

                    int? pickedInvInd = InteractiveUI.PickAction(CursorPos.SubZoneTop, invActions);
                    switch (pickedInvInd)
                    {
                        case 0:
                            HandleEquipInventory(gameData);
                            break;

                        case 1:
                            HandleSkillInventory(gameData);
                            break;

                        default:
                            break;
                    }
                    break;
            }
        }
    }

    private static void HandleShopBuy(GameData gameData, ShopEvent shopEvent)
    {
        List<Item> shopItems = shopEvent.SellingItems;
        int displayCount = UIConstants.MainZoneHeight + UIConstants.SubZoneHeight;
        int startInd = 0, endInd = shopItems.Count > displayCount ? displayCount : shopItems.Count;
        bool pickFromEnd = false;

        while (true)
        {
            GameUI.ShopTradingScreen(gameData, shopItems[startInd..endInd], true);

            int? pickedEquipInd = InteractiveUI.TradeItem(CursorPos.MainZoneTop + 1, shopItems[startInd..endInd], true, startInd > 0, endInd < shopItems.Count, pickFromEnd);

            if (pickedEquipInd == null)
                return;

            if (pickedEquipInd == -1 && startInd > 0)
            {
                pickFromEnd = false;
                startInd--;
                endInd--;
                continue;
            }

            if (pickedEquipInd == displayCount && endInd < shopItems.Count)
            {
                pickFromEnd = true;
                startInd++;
                endInd++;
                continue;
            }

            Item pickedItem = shopItems[startInd + (int) pickedEquipInd];
            if (pickedItem.Price <= gameData.Player.PlayerGold.Quantity)
            {
                gameData.Player.TradeItem(pickedItem, true);
                shopItems.Remove(pickedItem);
                endInd = shopItems.Count > displayCount ? displayCount : shopItems.Count;
            }
        }
    }

    private static void HandleShopSell(GameData gameData)
    {
        List<Item> playerItems = [];
        playerItems.AddRange(gameData.Player.EquipInventory);
        playerItems.AddRange(gameData.Player.SkillInventory);

        int displayCount = UIConstants.MainZoneHeight + UIConstants.SubZoneHeight;
        int startInd = 0, endInd = playerItems.Count > displayCount ? displayCount : playerItems.Count;
        bool pickFromEnd = false;

        while (true)
        {
            GameUI.ShopTradingScreen(gameData, playerItems[startInd..endInd], false);

            int? pickedEquipInd = InteractiveUI.TradeItem(CursorPos.MainZoneTop + 1, playerItems[startInd..endInd], false, startInd > 0, endInd < playerItems.Count, pickFromEnd);

            if (pickedEquipInd == null)
                return;

            if (pickedEquipInd == -1 && startInd > 0)
            {
                pickFromEnd = false;
                startInd--;
                endInd--;
                continue;
            }

            if (pickedEquipInd == displayCount && endInd < playerItems.Count)
            {
                pickFromEnd = true;
                startInd++;
                endInd++;
                continue;
            }

            Item pickedItem = playerItems[startInd + (int) pickedEquipInd];
            gameData.Player.TradeItem(pickedItem, false);
            playerItems.Remove(pickedItem);
        }
    }
}