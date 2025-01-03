using System.Security.Cryptography;
using static UIHelper;

class Game
{
    public static void Start()
    {
        List<string> welcomeOptions = ["PLAY ONLINE", "PLAY OFFLINE", "EXIT"];

        while(true)
        {
            GameUI.WelcomeScreen(welcomeOptions);
            GameUI.StartTitleAnim();

            Console.ReadKey(true);
            switch(InteractiveUI.PickString(CursorPos.MainMenuLeft, CursorPos.MainMenuTop, welcomeOptions))
            {
                case 0:
                case 1:
                    StartGame();
                    break;
                
                case 2: case null:
                    return;
            }
        }
    }

    // Return false if return without starting a game
    private static bool StartGame()
    {
        List<string> playOptions = ["NEW GAME", "LOAD GAME", "CUSTOM SEED", "RETURN"];
        GameUI.StartScreen(playOptions);

        while (true)
        {
            GameUI.StartTitleAnim();

            Console.ReadKey(true);
            switch(InteractiveUI.PickString(CursorPos.MainMenuLeft, CursorPos.MainMenuTop, playOptions))
            {
                case 0:
                    GameUI.StopTitleAnim();
                    NewGame();
                    return true;

                case 1:
                    GameUI.StopTitleAnim();
                    if (!LoadGame())
                    {
                        GameUI.StartScreen(playOptions);
                        continue;
                    }
                    return true;

                case 2:
                    GameUI.StopTitleAnim();
                    GameUI.StartScreen(["", "", "", ""], false);
                    Console.CursorTop = CursorPos.MainMenuTop;
                    Console.CursorLeft = CursorPos.MainMenuLeft * 70 / 100;
                    Console.Write("ENTER SEED: ");
                    string? input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        GameUI.StartScreen(playOptions);
                        continue;
                    }
                    
                    NewGame(input);
                    return true;
                
                case 3: case null:
                    return false;
            }
        }
    }

    private static void NewGame(string? seed = null)
    {
        List<Equipment> startEquips =
        [
            AssetManager.Equipments[1],
            AssetManager.Equipments[2],
            AssetManager.Equipments[3],
        ];
        List<Skill> startSkills =
        [
            AssetManager.Skills[1],
            AssetManager.Skills[2],
            AssetManager.Skills[3]
        ];

        GameData gameData = new(seed?.GetHashCode() ?? null);

        GameUI.GenericGameScreen(gameData);
        GameUI.PrintComponents(startEquips, UIConstants.MainZoneHeight, " -- Choose Starting Item:", CursorPos.MainZoneTop);
        
        int? pickedEquipInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, startEquips);
        if (pickedEquipInd == null)
            return;

        Equipment pickedEquip = startEquips[(int) pickedEquipInd];
        gameData.Player.Equip(new(pickedEquip));

        GameUI.GenericGameScreen(gameData);
        GameUI.PrintComponents(startSkills, UIConstants.MainZoneHeight, " -- Choose Starting Skill:", CursorPos.MainZoneTop);
        
        int? pickedSkillInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, startSkills);
        if (pickedSkillInd == null)
            return;

        Skill pickedSkill = startSkills[(int) pickedEquipInd];
        gameData.Player.AddSkill(new(pickedSkill));

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
        EventManager eventManager = new(gameData);
        List<string> invOptions = ["Change Equipment", "Change Skill"];
        List<string> loseOptions = ["RETRY", "RETURN TO TITLE"];
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
                else return;
            }

            if (pickedRouteInd == routes.Count) // Move down to inventory options
            {
                int? pickedInvInd = InteractiveUI.PickString(CursorPos.SubZoneTop + 1, invOptions, true);
                switch (pickedInvInd)
                {
                    case null:
                        if (HanldePause(gameData)) continue;
                        else return;

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

            switch (pickedRoute)
            {
                case FightEvent fightEvent:
                    int hpPreFight = gameData.Player.HP;
                    int mpPreFight = gameData.Player.MP;
                    List<Monster> monstersPreFight = [];
                    fightEvent.Monsters.ForEach(monster => monstersPreFight.Add(new(monster)));

                    var fightResult = HandleFightEvent(gameData, fightEvent);

                    if (fightResult == null)
                        return;
                    else if (fightResult == false)
                    {
                        GameUI.GameOverScreen(loseOptions);

                        switch(InteractiveUI.PickString(CursorPos.MainMenuLeft, CursorPos.EndMenuTop, loseOptions))
                        {
                            case 0:
                                gameData.Player.HP = hpPreFight;
                                gameData.Player.MP = mpPreFight;
                                fightEvent.Monsters = monstersPreFight;
                                continue;
                            
                            case 1: case null: default: return;
                        }
                    }
                    break;

                    case ShopEvent shopEvent:
                        HandleShop(gameData, shopEvent);
                        break;

                    case TreasureEvent treasureEvent:
                        GameUI.TreasureOpening(gameData);
                        if (!HandleRewards(gameData, treasureEvent.Treasures))
                            return;
                        break;

                    default:
                        switch (pickedRoute.Type)
                        {
                            case EventType.Camp:
                                gameData.Player.Regenerate();
                                GameUI.CampfireScreen(gameData);
                                break;
                        }
                        break;
            };
            
            // No room left
            if (!gameData.Progress.Next())
            {
                HandleVictory(gameData);
                return;
            }
            gameData.Save();
        }
    }

    // return false if quit
    private static bool HanldePause(GameData gameData)
    {
        List<string> pauseOptions = ["RESUME", "SAVE & EXIT"];

        gameData.SetTime(false);
        GameUI.PausePopup(pauseOptions, gameData.GetElapsedTime());

        int? optionInd = InteractiveUI.PickString(CursorPos.PauseOptionLeft, CursorPos.PauseOptionTop, pauseOptions);
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
            equipInv = gameData.Player.EquipInventory;
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
            }
            else
            {
                int? equippedSkillInd = InteractiveUI.PickComponent(CursorPos.SubZoneTop + 1, gameData.Player.Skills);
                if (equippedSkillInd != null)
                    gameData.Player.ChangeSkill((int) equippedSkillInd, pickedSkill);
            }
            skillInv = gameData.Player.SkillInventory;
        }
    }

    // Return null if quit mid-fight, false if lose, true if win
    private static bool? HandleFightEvent(GameData gameData, FightEvent fightEvent)
    {
        List<string> fightActions = ["Attack", "Use Skill"];

        while (fightEvent.Monsters.Count > 0)
        {
            GameUI.FightScreen(gameData, fightEvent.Monsters, fightActions);

            switch (InteractiveUI.PickString(CursorPos.SubZoneTop + 1, fightActions))
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
                    else return null;
            }
            
            if (gameData.Player.HP <= 0)
                return false;

            fightEvent.Monsters.RemoveAll(monster => monster.HP == 0);
        }

        return HandleRewards(gameData, fightEvent.Rewards) ? true : null;
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
            
            switch (InteractiveUI.PickString(CursorPos.SubZoneTop, shopActions))
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

                    int? pickedInvInd = InteractiveUI.PickString(CursorPos.SubZoneTop, invActions);
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
            endInd = playerItems.Count > displayCount ? displayCount : playerItems.Count;
        }
    }

    private static void HandleVictory(GameData gameData)
    {
        gameData.SetTime(false);

        List<string> winOptions = ["POST SCORE", "RETURN TO TITLE"];

        GameUI.VictoryScreen(gameData.GetElapsedTime(), winOptions);

        switch(InteractiveUI.PickString(CursorPos.MainMenuLeft, CursorPos.EndMenuTop + 2, winOptions))
        {
            case 0:
                // Post score
                break;
            
            case 1: case null: default: return;
        }
    }
}