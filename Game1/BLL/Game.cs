using System.Security.Cryptography;

using static GameUIHelper;

class Game
{
    private static NetworkHandler? networkHandler;
    private static User? user;

    public static void Start()
    {
        GameUI.ConsoleSizeNotice();

        List<string> welcomeOptions = ["PLAY ONLINE", "PLAY OFFLINE", "EXIT"];
        GameUI.TitleScreenBorders();

        while(true)
        {
            GameUI.TitleScreenBorders(false, true);
            GameUI.StartTitleAnim();

            switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop, welcomeOptions))
            {
                case 0:
                    OnlineMode();
                    break;

                case 1:
                    OfflineMode();
                    break;
                
                default:
                    return;
            }
        }
    }

    // Return false if failed to connect to server or login
    private static void OnlineMode()
    {
        GameUI.ConnectingScreen(ref networkHandler);

        List<string> onlineOptions = ["REGISTER", "LOGIN", "RESET PASSWORD", "RETURN"];

        while (true)
        {
            if (networkHandler == null || !networkHandler.IsConnected)
                return;

            GameUI.TitleScreenBorders(false, true);
            GameUI.StartTitleAnim();
            
            switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop, onlineOptions))
            {
                case 0:
                    GameUI.RegisterScreen(networkHandler);
                    break;

                case 1:
                    GameUI.LoginScreen(networkHandler, out user);
                    if (user != null)
                        OnlinePlay();
                    break;

                case 2:
                    GameUI.ResetPasswordScreen(networkHandler);
                    break;

                default:
                    networkHandler.Close();
                    networkHandler = null;
                    return;
            }
        }
    }

    private static void OnlinePlay()
    {
        List<string> onlineOptions = ["NEW GAME", "LOAD GAME", "VIEW SCORE", "LOG OUT"];
        // , "UPDATE USER INFO"

        while (true)
        {
            if (networkHandler == null || !networkHandler.IsConnected || user == null)
                return;
                
            GameSave? cloudSave = networkHandler.DownloadSave(out _);
            cloudSave?.Save("CloudSave");

            GameUI.TitleScreenBorders(false, true);
            GameUI.StartTitleAnim();
            lock (ConsoleLock)
            {
                Console.CursorTop = CursorPos.BottomBorderTop - 1;
                WriteCenter($"Logged in as {user.Username}");
            }

            switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop, onlineOptions))
            {
                case 0:
                    NewGame();
                    break;

                case 1:
                    LoadGame();
                    break;

                case 2:
                    ViewScore();
                    break;

                // case 3:
                    // update user info
                    // break;

                default:
                    if (!networkHandler.Logout(out string? error))
                        GameUI.WarningPopup(error);
                    user = null;
                    return;
            }
        }
    }

    private static void ViewScore()
    {
        List<string> scoreOptions = ["PERSONAL", "TOP MONTHLY", "TOP ALL TIME", "RETURN"];

        while (true)
        {
            if (networkHandler == null || !networkHandler.IsConnected || user == null)
                return;

            if (!networkHandler.GetScores(out List<Score>? personal, out List<Score>? monthly, out List<Score>? alltime, out string errorMsg))
            {
                GameUI.WarningPopup(errorMsg);
                return;
            }

            if (personal == null || monthly == null || alltime == null)
            {
                GameUI.WarningPopup("Null scores");
                return;
            }
            
            GameUI.TitleScreenBorders(false, true);
            GameUI.StartTitleAnim();

            switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop, scoreOptions))
            {
                case 0:
                    GameUI.ViewScoresScreen(personal, "PERSONAL SCORES");
                    break;

                case 1:
                    GameUI.ViewScoresScreen(personal, "TOP MONTHLY");
                    break;

                case 2:
                    GameUI.ViewScoresScreen(personal, "TOP ALL TIME");
                    break;

                default:
                    return;
            }
        }
    }

    // Return false if return without starting a game
    private static void OfflineMode()
    {
        List<string> playOptions = ["NEW GAME", "LOAD GAME", "RETURN"];

        while (true)
        {
            GameUI.TitleScreenBorders(false, true);
            GameUI.StartTitleAnim();

            switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop, playOptions))
            {
                case 0: // New Game
                    NewGame();
                    break;

                case 1: // Load Game
                    LoadGame();
                    break;
                
                default:
                    GameUI.TitleScreenBorders(false, true);
                    return;
            }
        }
    }

    private static void NewGame()
    {
        List<string> newGameOptions = ["RANDOM SEED", "CUSTOM SEED", "RETURN"];

        while (true)
        {
            GameUI.TitleScreenBorders(false, true);
            GameUI.StartTitleAnim();

            switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop, newGameOptions))
            {
                case 0: // Random Seed
                    StartNewRun();
                    GameUI.TitleScreenBorders();
                    return;

                case 1: // Custom Seed
                    string? seed = GameUI.EnterSeed();
                    if (seed == null)
                        continue;

                    StartNewRun(seed);
                    GameUI.TitleScreenBorders();
                    return;
                
                default:
                    continue;
            }
        }
    }

    private static void LoadGame()
    {
        FileManager.LoadSaves(out List<GameSave> saves, out string? error);

        if (error != null)
        {
            GameUI.WarningPopup(error);
            return;
        }

        GameUI.TitleScreenBorders(false, true);

        if (saves.Count > 0)
        {
            lock (ConsoleLock)
            {
                Console.CursorTop = CursorPos.TitleScreenMenuTop;
                Console.WriteLine(" -- Choose Save:");
            }

            int? pickedSaveInd = InteractiveUI.PickComponent(CursorPos.TitleScreenMenuTop + 1, saves);
            if (pickedSaveInd == null)
                return;
            else
            {
                GameUI.StopTitleAnim();
                GameLoop(saves[(int) pickedSaveInd]);
                GameUI.TitleScreenBorders();
                return;
            }
        }
        else
        {
            GameUI.WarningPopup("No saves found!");
            return;
        }
    }

    private static void StartNewRun(string? seed = null)
    {
        GameUI.StopTitleAnim();

        GameData gameData = new(seed?.GetHashCode());
        GameSave gameSave = new(gameData);

        List<Equipment> startEquips =
        [
            new(AssetManager.Equipments[1]),
            new(AssetManager.Equipments[2]),
            new(AssetManager.Equipments[3])
        ];
        List<Skill> startSkills =
        [
            new(AssetManager.Skills[1]),
            new(AssetManager.Skills[2]),
            new(AssetManager.Skills[3])
        ];

        GameUI.GenericGameScreen(gameData);
        GameUI.PrintMainZone(startEquips, "Choose Starting Item:");
        
        int? pickedEquipInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, startEquips);
        if (pickedEquipInd == null)
            return;

        // Equipment pickedEquip = new();
        gameData.Player.AddItem(startEquips[(int) pickedEquipInd]);

        GameUI.GenericGameScreen(gameData);
        GameUI.PrintMainZone(startSkills, "Choose Starting Skill:");
        
        int? pickedSkillInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, startSkills);
        if (pickedSkillInd == null)
            return;

        // Skill pickedSkill = new();
        gameData.Player.AddItem(startSkills[(int) pickedEquipInd]);

        gameData.Progress.Next();
        GameLoop(gameSave);
    }

    private static void GameLoop(GameSave gameSave)
    {
        GameData gameData = gameSave.GameData;
        EventManager eventManager = new(gameData);
        List<string> invOptions = ["Change Equipment", "Change Skill"];
        List<string> loseOptions = ["RETRY", "RETURN TO TITLE"];
        bool pickFromEnd = false;
        gameData.SetTime(true);

        while (true)
        {
            gameSave.Save("AutoSave");
            List<Event> routes = eventManager.GetEvents();
            
            GameUI.GenericGameScreen(gameData);
            GameUI.PrintMainZone(routes, "Routes:");
            GameUI.PrintSubZone(invOptions, "Inventory:");

            int? pickedRouteInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, routes, false, true, pickFromEnd);
            pickFromEnd = false;
            if (pickedRouteInd == null)
            {
                if (HanldePause(gameData)) continue;
                else 
                {
                    gameSave.Save("LocalSave");
                    if (networkHandler != null && !networkHandler.UploadSave(gameSave, out _))
                    {
                        networkHandler.Close();
                        networkHandler = null;
                    }
                    return;
                }
            }

            if (pickedRouteInd == routes.Count) // Move down to inventory options
            {
                int? pickedInvInd = InteractiveUI.PickString(CursorPos.SubZoneTop + 1, invOptions, true);
                switch (pickedInvInd)
                {
                    case null:
                        if (HanldePause(gameData)) continue;
                        else 
                        {
                            gameSave.Save("LocalSave");
                            if (networkHandler != null && !networkHandler.UploadSave(gameSave, out _))
                            {
                                networkHandler.Close();
                                networkHandler = null;
                            }
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
                    else if (fightResult == false) // lose
                    {
                        GameUI.GameOverScreen(loseOptions);

                        switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.EndScreenMenuTop, loseOptions))
                        {
                            case 0: // retry
                                gameData.Player.HP = hpPreFight;
                                gameData.Player.MP = mpPreFight;
                                fightEvent.Monsters = monstersPreFight;
                                continue;
                            
                            default: return;
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
            
            // No room left - victory
            if (!gameData.Progress.Next())
            {
                HandleVictory(gameData);
                return;
            }
        }
    }

    // return false if quit
    private static bool HanldePause(GameData gameData)
    {
        List<string> pauseOptions = ["RESUME", "SAVE & EXIT"];

        gameData.SetTime(false);
        GameUI.PausePopup(pauseOptions, gameData.GetElapsedTime());

        int? optionInd = InteractiveUI.PickString(CursorPos.PauseMenuLeft, CursorPos.PauseMenuTop, pauseOptions);
        switch (optionInd)
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
            GameUI.GenericGameScreen(gameData);
            GameUI.PrintMainZone(equipInv[startInd..endInd], "Inventory:");
            GameUI.PrintSubZone(gameData.Player.GetEquipped(), "Currently Equipped:");

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
            gameData.Player.ChangeEquip(pickedEquip);
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
            GameUI.GenericGameScreen(gameData);
            GameUI.PrintMainZone(skillInv[startInd..endInd], "Inventory:");
            GameUI.PrintSubZone(gameData.Player.Skills, "Currently Equipped:");

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
                gameData.Player.AddItem(pickedSkill);
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
            GameUI.GenericGameScreen(gameData);
            GameUI.PrintMainZone(fightEvent.Monsters);

            switch (InteractiveUI.PickString(CursorPos.SubZoneTop, fightActions))
            {
                case 0:
                    int? pickedMonsterInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop, fightEvent.Monsters);
                    if (pickedMonsterInd == null)
                        continue;
                    
                    Monster pickedMonster = fightEvent.Monsters[(int) pickedMonsterInd];
                    
                    gameData.Player.Attack(pickedMonster);
                    break;

                case 1:
                    GameUI.GenericGameScreen(gameData);
                    GameUI.PrintMainZone(fightEvent.Monsters);

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
                            case SkillType.Single:
                                pickedMonsterInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop, fightEvent.Monsters);
                                if (pickedMonsterInd == null)
                                    continue;

                                pickedMonster = fightEvent.Monsters[(int) pickedMonsterInd];
                                gameData.Player.UseSkill(pickedSkill, [pickedMonster]);
                                break;

                            case SkillType.Random:
                                pickedMonster = fightEvent.Monsters[RandomNumberGenerator.GetInt32(fightEvent.Monsters.Count - 1)];
                                gameData.Player.UseSkill(pickedSkill, [pickedMonster]);
                                break;

                            case SkillType.All:
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
            GameUI.GenericGameScreen(gameData);
            GameUI.PrintMainZone(rewards, "Rewards:");

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
            GameUI.GenericGameScreen(gameData);
            GameUI.ShopBanner();
            
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
                    GameUI.GenericGameScreen(gameData);
                    GameUI.ShopBanner();

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

            int? pickedEquipInd = InteractiveUI.PickTradeItem(CursorPos.MainZoneTop + 1, shopItems[startInd..endInd], true, startInd > 0, endInd < shopItems.Count, pickFromEnd);

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

            int? pickedEquipInd = InteractiveUI.PickTradeItem(CursorPos.MainZoneTop + 1, playerItems[startInd..endInd], false, startInd > 0, endInd < playerItems.Count, pickFromEnd);

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

        if (networkHandler != null && networkHandler.IsConnected && user != null)
        {
            Score score = new(user.UserID, user.Nickname, gameData.GetElapsedTime());
            if (!networkHandler.UploadScore(score, out _))
            {
                networkHandler.Close();
                networkHandler = null;
            }
        }

        List<string> winOptions = ["RETURN TO TITLE"];

        GameUI.VictoryScreen(gameData.GetElapsedTime(), winOptions);

        switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.EndScreenMenuTop + 3, winOptions))
        {
            default:
                return;
        }
    }
}