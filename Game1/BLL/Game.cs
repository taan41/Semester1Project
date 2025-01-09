using System.Security.Cryptography;

using static GameUIHelper;
using static Utilities;

class Game
{
    private static NetworkHandler? networkHandler;

    public static void Start()
    {
        List<string> welcomeOptions = ["PLAY ONLINE", "PLAY OFFLINE", "EXIT"];
        GameUI.TitleScreenBorders();

        while(true)
        {
            GameUI.StartTitleAnim();

            switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop, welcomeOptions))
            {
                case 0:
                    if (OnlineGame())
                        goto case 1;
                    break;

                case 1:
                    StartRun();
                    break;
                
                case 2: case null:
                    return;
            }
        }
    }

    // Return false if failed to connect to server or login
    private static bool OnlineGame()
    {
        GameUI.ConnectingScreen(ref networkHandler);

        if (networkHandler == null)
            return false;

        List<string> onlineOptions = ["REGISTER", "LOGIN", "RESET PASSWORD", "RETURN"];

        while (true)
        {
            GameUI.TitleScreenBorders(false, true);
            switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop, onlineOptions))
            {
                case 0:
                    Register();
                    break;

                case 1:
                    // Login();
                    return true;

                case 2:
                    // ResetPassword();
                    break;

                default:
                    networkHandler.Close();
                    networkHandler = null;
                    GameUI.TitleScreenBorders(false, true);
                    return false;
            }
        }
    }

    private static void Register()
    {
        if (networkHandler == null)
            return;

        if (!networkHandler.IsConnected)
        {
            GameUI.WarningPopup("Can't connect to server!");
            GameUI.TitleScreenBorders(true);
            GameUI.StartTitleAnim();
            return;
        }

        GameUI.TitleScreenBorders(false, true);
        string?
            username = null,
            nickname = null,
            password = null,
            confirmPassword = null,
            email = null,
            errorMsg;
        int tempCursorLeft, tempCursorTop;

        while (true)
        {
            lock (ConsoleLock)
            {
                Console.CursorTop = CursorPos.TitleScreenMenuTop;
                Console.WriteLine(" 'ESC' to return");
                Console.Write($" Username ({DataConstants.usernameMin} ~ {DataConstants.usernameMax} characters): ");
                (tempCursorLeft, tempCursorTop) = Console.GetCursorPosition();
                if (username != null)
                    Console.WriteLine(username);
            }

            if (username == null)
            {
                username = ReadInput(tempCursorLeft, tempCursorTop, false, DataConstants.usernameMax);
                
                if (username == null)
                    return;
                else if (string.IsNullOrWhiteSpace(username) || username.Length < DataConstants.usernameMin)
                {
                    username = null;
                    continue;
                }

                if (!networkHandler.CheckUsername(username, out errorMsg))
                {
                    username = null;
                    GameUI.WarningPopup(errorMsg);
                    GameUI.TitleScreenBorders(true);
                    GameUI.StartTitleAnim();
                    continue;
                }
            }

            lock (ConsoleLock)
            {
                Console.CursorTop = CursorPos.TitleScreenMenuTop + 2;
                Console.Write($" Password ({DataConstants.passwordMin} ~ {DataConstants.passwordMax} characters): ");
                (tempCursorLeft, tempCursorTop) = Console.GetCursorPosition();
                if (password != null)
                    Console.WriteLine(new string('*', password.Length));
            }

            if (password == null)
            {
                password = ReadInput(tempCursorLeft, tempCursorTop, true, DataConstants.passwordMax);
                
                if (password == null)
                    return;
                else if (string.IsNullOrWhiteSpace(password) || password.Length < DataConstants.passwordMin)
                {
                    password = null;
                    continue;
                }
            }

            lock (ConsoleLock)
            {
                Console.CursorTop = CursorPos.TitleScreenMenuTop + 3;
                Console.Write(" Confirm Password: ");
                (tempCursorLeft, tempCursorTop) = Console.GetCursorPosition();
                if (confirmPassword != null)
                    Console.WriteLine(new string('*', confirmPassword.Length));
            }

            if (confirmPassword == null)
            {
                Console.CursorTop = tempCursorTop;
                confirmPassword = ReadInput(tempCursorLeft, tempCursorTop, true, DataConstants.passwordMax);
                
                if (confirmPassword == null)
                    return;
                else if (string.IsNullOrWhiteSpace(confirmPassword) || confirmPassword.Length < DataConstants.passwordMin)
                {
                    confirmPassword = null;
                    continue;
                }
                else if (password != confirmPassword)
                {
                    confirmPassword = null;
                    GameUI.WarningPopup("Passwords do not match!");
                    GameUI.TitleScreenBorders(true);
                    GameUI.StartTitleAnim();
                    continue;
                }
            }

            lock (ConsoleLock)
            {
                Console.CursorTop = CursorPos.TitleScreenMenuTop + 4;
                Console.Write($" Nickname ({DataConstants.nicknameMin} ~ {DataConstants.nicknameMax} characters): ");
                (tempCursorLeft, tempCursorTop) = Console.GetCursorPosition();
                if (nickname != null)
                    Console.WriteLine(nickname);
            }

            if (nickname == null)
            {
                nickname = ReadInput(tempCursorLeft, tempCursorTop, false, DataConstants.nicknameMax);
                
                if (nickname == null)
                    return;
                else if (string.IsNullOrWhiteSpace(nickname) || nickname.Length < DataConstants.nicknameMin)
                {
                    nickname = null;
                    continue;
                }
            }

            lock (ConsoleLock)
            {
                Console.CursorTop = CursorPos.TitleScreenMenuTop + 5;
                Console.Write(" Email: ");
                (tempCursorLeft, tempCursorTop) = Console.GetCursorPosition();
                if (email != null)
                    Console.WriteLine(email);
            }

            if (email == null)
            {
                email = ReadInput(tempCursorLeft, tempCursorTop, false, DataConstants.emailLen);
                
                if (email == null)
                    return;
                else if (string.IsNullOrWhiteSpace(email))
                {
                    email = null;
                    continue;
                }
            }

            User newUser = new(username, nickname, password, email);
            if (!networkHandler.Register(newUser, out errorMsg))
            {
                GameUI.WarningPopup(errorMsg);
                GameUI.TitleScreenBorders(true);
                GameUI.StartTitleAnim();
                return;
            }
            else
            {
                GameUI.SuccessPopup("Registration Successful!");
                GameUI.TitleScreenBorders(true);
                GameUI.StartTitleAnim();
                return;
            }
        }
    }

    // Return false if return without starting a game
    private static bool StartRun()
    {
        List<string> playOptions = ["NEW GAME", "LOAD GAME", "RETURN"];
        int tempCursorTop;

        while (true)
        {
            GameUI.StartTitleAnim();
            GameUI.TitleScreenBorders(false, true);

            switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop, playOptions))
            {
                case 0: new_game:// New Game
                    List<string> newGameOptions = ["RANDOM SEED", "CUSTOM SEED", "RETURN"];

                    switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop, newGameOptions))
                    {
                        case 0: // Random Seed
                            NewGame();
                            GameUI.TitleScreenBorders(true);
                            return true;

                        case 1: // Custom Seed
                            lock (ConsoleLock)
                            {
                                Console.SetCursorPosition(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop + 4);
                                Console.WriteLine(" ENTER SEED: ");
                                tempCursorTop = Console.CursorTop;
                            }

                            string? input = ReadInput(15, tempCursorTop, false, 40);
                            if (string.IsNullOrWhiteSpace(input))
                            {
                                GameUI.TitleScreenBorders(false, true);
                                goto new_game;
                            }
                            
                            NewGame(input);
                            GameUI.TitleScreenBorders(true);
                            return true;
                        
                        default:
                            continue;
                    }

                case 1: // Load Game
                    if (!LoadGame())
                    {
                        continue;
                    }
                    GameUI.TitleScreenBorders(true);
                    return true;
                
                default:
                    GameUI.TitleScreenBorders(false, true);
                    return false;
            }
        }
    }

    private static void NewGame(string? seed = null)
    {
        GameUI.StopTitleAnim();

        GameData gameData = new(seed?.GetHashCode());
        GameSave gameSave = new(gameData);

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

        GameUI.GenericGameScreen(gameData);
        GameUI.PrintComponents(startEquips, UIConstants.MainZoneHeight, " -- Choose Starting Item:", CursorPos.MainZoneTop);
        
        int? pickedEquipInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, startEquips);
        if (pickedEquipInd == null)
            return;

        Equipment pickedEquip = new(startEquips[(int) pickedEquipInd]);
        gameData.Player.AddItem(pickedEquip);

        GameUI.GenericGameScreen(gameData);
        GameUI.PrintComponents(startSkills, UIConstants.MainZoneHeight, " -- Choose Starting Skill:", CursorPos.MainZoneTop);
        
        int? pickedSkillInd = InteractiveUI.PickComponent(CursorPos.MainZoneTop + 1, startSkills);
        if (pickedSkillInd == null)
            return;

        Skill pickedSkill = new(startSkills[(int) pickedEquipInd]);
        gameData.Player.AddItem(pickedSkill);

        gameData.Progress.Next();
        GameLoop(gameSave);
    }

    private static bool LoadGame()
    {
        FileManager.LoadSaves(out List<GameSave> saves, out string? error);

        if (error != null)
        {
            GameUI.WarningPopup(error);
            return false;
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
                return false;
            else
            {
                GameUI.StopTitleAnim();
                GameLoop(saves[(int) pickedSaveInd]);
                return true;
            }
        }
        else
        {
            lock (ConsoleLock)
            {
                Console.CursorTop = CursorPos.TitleScreenMenuTop;
                WriteCenter("No Save Found!");
            }
            return false;
        }
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
            
            // GameUI.RouteScreen(gameData, routes, invOptions);
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
                    else if (fightResult == false)
                    {
                        GameUI.GameOverScreen(loseOptions);

                        switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.EndScreenMenuTop, loseOptions))
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
            // GameUI.InventoryScreen(gameData, equipInv[startInd..endInd], gameData.Player.GetEquipped());
            GameUI.GenericGameScreen(gameData);
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
            // GameUI.InventoryScreen(gameData, skillInv[startInd..endInd], gameData.Player.Skills);
            GameUI.GenericGameScreen(gameData);
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
            // GameUI.FightScreen(gameData, fightEvent.Monsters, fightActions);
            GameUI.GenericGameScreen(gameData);
            GameUI.PrintMainZone(fightEvent.Monsters);

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

        List<string> winOptions = ["POST SCORE", "RETURN TO TITLE"];

        GameUI.VictoryScreen(gameData.GetElapsedTime(), winOptions);

        switch (InteractiveUI.PickString(CursorPos.TitleScreenMenuLeft, CursorPos.EndScreenMenuTop + 2, winOptions))
        {
            case 0:
                // Post score
                break;
            
            case 1: case null: default: return;
        }
    }
}