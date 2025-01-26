using System.Security.Cryptography;
using BLL.GameComponents.Others;
using BLL.GameComponents.ItemComponents;
using BLL.GameComponents.EventComponents;
using BLL.GameComponents.EntityComponents;
using BLL.GameHandlers;
using BLL.GameHelpers;

using static ConsolePL.GameScreens;
using static ConsolePL.ConsoleHelper;

namespace ConsolePL
{
    class Program
    {
        public static void Main()
        {
            Console.CursorVisible = false;
            ConsoleSizeNotice();

            while (true)
            {
                Console.Clear();

                try
                {
                    if (!ModeSelection())
                        return;
                }
                catch (Exception ex)
                {
                    Console.Clear();
                    Console.WriteLine($"Game crashed: {ex}");
                    Console.ReadKey(true);
                }
                finally
                {
                    if (ServerHandler.IsConnected)
                        ServerHandler.Close();
                }
            }
        }
        
        static bool ModeSelection()
        {
            List<string> options = ["MANUAL", "PLAY ONLINE", "PLAY OFFLINE", "EXIT"];

            while (true)
            {
                TitleScreenDrawBorders(false, true);
                StartTitleAnim();

                switch (Picker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
                {
                    case 0:
                        Manual();
                        break;
                    
                    case 1:
                        OnlineMode();
                        break;

                    case 2:
                        OfflineMode();
                        break;

                    case 3: case null:
                        return false;
                }
            }
        }

        static void Manual()
        {
            List<string> options = ["NAVIGATING UI", "GAMEPLAY", "ABOUT ONLINE MODE", "RETURN"];

            while (true)
            {
                TitleScreenDrawBorders(false, true);
                StartTitleAnim();

                switch (Picker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
                {
                    case 0:
                        ManualNavigatingUI();
                        break;

                    case 1:
                        ManualGameplay();
                        break;

                    case 2:
                        ManualAboutOnlineMode();
                        break;

                    case 3: case null:
                        return;
                }
            }
        }

        static void OfflineMode()
        {
            List<string> options = ["NEW GAME", "LOAD GAME", "RETURN"];

            while (true)
            {
                TitleScreenDrawBorders(false, true);
                StartTitleAnim();

                switch (Picker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
                {
                    case 0:
                        NewGame();
                        break;

                    case 1:
                        LoadGame();
                        break;
                    
                    case 2: case null:
                        return;
                }
            }
        }

        static void OnlineMode()
        {
            ServerConnectScreen();

            List<string> options = ["REGISTER", "LOGIN", "RESET PASSWORD", "RETURN"];

            while (true)
            {
                if (!ServerHandler.IsConnected)
                    return;

                TitleScreenDrawBorders(false, true);
                StartTitleAnim();
                
                switch (Picker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
                {
                    case 0:
                        RegisterScreen();
                        break;

                    case 1:
                        if (LoginScreen())
                            OnlinePlay();
                        break;

                    case 2:
                        ResetPasswordScreen();
                        break;

                    case 3: case null:
                        ServerHandler.Close();
                        return;
                }
            }
        }

        static void OnlinePlay()
        {
            List<string> options = ["NEW GAME", "LOAD GAME", "VIEW SCORE", "LOG OUT"];

            while (true)
            {
                if (!ServerHandler.IsConnected || !ServerHandler.IsLoggedIn)
                    return;

                TitleScreenDrawBorders(false, true);
                StartTitleAnim();

                lock (ConsoleLock)
                {
                    Console.CursorLeft = 1;
                    Console.CursorTop = CursorPos.BottomBorderTop - 1;
                    Console.Write($"Welcome, {ServerHandler.Username}");
                }

                switch (Picker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
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

                    case 3: case null:
                        if (!ServerHandler.Logout(out string? error))
                            Popup(error);
                        return;
                }
            }
        }

        static void ViewScore()
        {
            List<string> options = ["PERSONAL", "TOP MONTHLY", "TOP ALL TIME", "RETURN"];

            while (true)
            {
                if (!ServerHandler.IsConnected || !ServerHandler.IsLoggedIn)
                    return;

                if (!ServerHandler.GetScores(out List<string> personal, out List<string> monthly, out List<string> alltime, out string error))
                {
                    Popup(error);
                    return;
                }
                
                TitleScreenDrawBorders(false, true);
                StartTitleAnim();

                switch (Picker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
                {
                    case 0:
                        ViewScoresScreen(personal, "PERSONAL SCORES");
                        break;

                    case 1:
                        ViewScoresScreen(monthly, "TOP MONTHLY SCORES");
                        break;

                    case 2:
                        ViewScoresScreen(alltime, "TOP OF ALL TIME");
                        break;

                    case 3: case null:
                        return;
                }
            }
        }

        static void LoadGame()
        {
            if (ServerHandler.IsLoggedIn)
                ServerHandler.DownloadSave();

            List<GameSave> saves = GameSave.LoadGameSaves(out string? error);

            if (error != null)
            {
                Popup(error);
                return;
            }

            TitleScreenDrawBorders(false, true);

            if (saves.Count > 0)
            {
                lock (ConsoleLock)
                {
                    Console.CursorTop = CursorPos.TitleScreenMenuTop;
                    Console.WriteLine(" -- Choose Save:");
                }

                int? pickedSaveInd = Picker.Component(saves, CursorPos.TitleScreenMenuTop + 1);
                if (pickedSaveInd != null)
                {
                    StopTitleAnim();
                    GameLoop(new(saves[(int) pickedSaveInd]));
                    return;
                }
                else
                    return;
            }
            else
            {
                Popup("No saves found!");
                return;
            }
        }

        static void NewGame()
        {
            List<string> options = ["RANDOM SEED", "CUSTOM SEED", "RETURN"];

            while (true)
            {
                TitleScreenDrawBorders(false, true);
                StartTitleAnim();

                switch (Picker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
                {
                    case 0:
                        StartNewRun();
                        break;

                    case 1:
                        string? seed = EnterSeed();
                        if (seed != null)
                            StartNewRun(seed);
                        break;
                    
                    case 2: case null:
                        return;
                }
            }
        }

        static void StartNewRun(string? seed = null)
        {
            StopTitleAnim();

            GameLoopHandler gameHandler = new(seed);

            List<Equipment> startEquips =
            [
                AssetLoader.GetEquip(1),
                AssetLoader.GetEquip(2),
                AssetLoader.GetEquip(3)
            ];
            List<Skill> startSkills =
            [
                AssetLoader.GetSkill(1),
                AssetLoader.GetSkill(2),
                AssetLoader.GetSkill(3)
            ];

            GenericGameScreen(gameHandler.Progress, gameHandler.Player);
            PrintMainZone(startEquips, "Choose Starting Item:");
            
            int? pickedEquipInd = Picker.Component(startEquips, CursorPos.MainZoneTop + 1);
            if (pickedEquipInd == null)
                return;

            gameHandler.Player.AddItem(new Equipment(startEquips[(int) pickedEquipInd]));

            GenericGameScreen(gameHandler.Progress, gameHandler.Player);
            PrintMainZone(startSkills, "Choose Starting Skill:");
            
            int? pickedSkillInd = Picker.Component(startSkills, CursorPos.MainZoneTop + 1);
            if (pickedSkillInd == null)
                return;
                
            gameHandler.Player.AddItem(new Skill(startSkills[(int) pickedSkillInd]));
            gameHandler.Progress.Next();
            GameLoop(gameHandler);
        }

        static void GameLoop(GameLoopHandler gameHandler)
        {
            List<string> actions = ["Pick A Route", "Inventory"];
            List<string> invOptions = ["Change Equipment", "Change Skill"];

            gameHandler.Timer(true);

            while (true)
            {
                gameHandler.SaveAs("AutoSave");

                List<Event> routes = gameHandler.Events.GetEvents();

                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                PrintMainZone(routes, "Routes:");
                PrintSubZone(actions, "Actions:");

                int? pickedActionInd = Picker.String(actions, CursorPos.SubZoneTop + 1);
                switch (pickedActionInd)
                {
                    case 0:
                        int? pickedRouteInd = Picker.Component(routes, CursorPos.MainZoneTop + 1);
                        if (pickedRouteInd == null)
                            break;

                        Event pickedRoute = routes[(int) pickedRouteInd];
                        if (!HandleEvent(gameHandler, pickedRoute))
                            return;

                        if (!gameHandler.Progress.Next())
                        {
                            HandleVictory(gameHandler);
                            return;
                        }
                        break;

                    case 1:
                        PrintSubZone(invOptions, "Inventory:");

                        int? pickedInvInd = Picker.String(invOptions, CursorPos.SubZoneTop + 1);
                        switch (pickedInvInd)
                        {
                            case 0:
                                HandleEquipInventory(gameHandler);
                                break;

                            case 1:
                                HandleSkillInventory(gameHandler);
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        if (HandlePause(gameHandler))
                            break;
                        else
                            return;
                }
            }
        }

        static bool HandlePause(GameLoopHandler gameHandler)
        {
            List<string> pauseOptions = ["RESUME", "SAVE & EXIT"];

            gameHandler.Timer(false);
            PausePopup(pauseOptions, gameHandler.GetElapsedTime());

            int? pickedPauseInd = Picker.String(pauseOptions, CursorPos.PauseMenuTop, CursorPos.PauseMenuLeft, pauseOptions.Count, 4);
            switch (pickedPauseInd)
            {
                case 1:
                    gameHandler.SaveAs("LocalSave", true);
                    return false;

                case 0: default:
                    gameHandler.Timer(true);
                    return true;
            }
        }

        static void HandleEquipInventory(GameLoopHandler gameHandler)
        {
            List<Equipment> equipInv = gameHandler.Player.EquipInventory;
            int displayCount = UIConstants.MainZoneHeight - 1;

            while (true)
            {
                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                PrintMainZone(equipInv, "Inventory:");
                PrintSubZone(gameHandler.Player.Equipped, "Currently Equipped:");

                int? pickedEquipInd = Picker.Component(equipInv, CursorPos.MainZoneTop + 1, 0, displayCount);
                if (pickedEquipInd == null)
                    return;

                Equipment pickedEquip = equipInv[(int) pickedEquipInd];
                gameHandler.Player.ChangeEquip(pickedEquip);
                equipInv = gameHandler.Player.EquipInventory;
            }
        }

        static void HandleSkillInventory(GameLoopHandler gameHandler)
        {
            List<Skill> skillInv = gameHandler.Player.SkillInventory;
            int displayCount = UIConstants.MainZoneHeight - 1;

            while (true)
            {
                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                PrintMainZone(skillInv, "Inventory:");
                PrintSubZone(gameHandler.Player.Skills, "Currently Equipped:");

                int? pickedSkillInd = Picker.Component(skillInv, CursorPos.MainZoneTop + 1, 0, displayCount);
                if (pickedSkillInd == null)
                    return;

                Skill pickedSkill = skillInv[(int) pickedSkillInd];

                if (gameHandler.Player.Skills.Count < 3)
                {
                    gameHandler.Player.AddItem(pickedSkill);
                }
                else
                {
                    int? equippedSkillInd = Picker.Component(gameHandler.Player.Skills, CursorPos.SubZoneTop + 1);
                    if (equippedSkillInd != null)
                        gameHandler.Player.ChangeSkill((int) equippedSkillInd, pickedSkill);
                }

                skillInv = gameHandler.Player.SkillInventory;
            }
        }

        static bool HandleEvent(GameLoopHandler gameHandler, Event route)
        {
            return route switch
            {
                FightEvent fightEvent => HandleFightEvent(gameHandler, fightEvent),
                _ => HandleNonfightEvent(gameHandler, route),
            };
        }

        static bool HandleFightEvent(GameLoopHandler gameHandler, FightEvent fightEvent)
        {
            List<string> fightActions = ["Attack", "Use Skill"];
            List<string> lostOptions = ["RETRY", "RETURN TO TITLE"];

            gameHandler.SavePrefightState(fightEvent);

            while (fightEvent.Monsters.Count > 0)
            {
                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                PrintMainZone(fightEvent.Monsters);
                PrintSubZone(fightActions, "Actions:");

                int? pickedActionInd = Picker.String(fightActions, CursorPos.SubZoneTop + 1);
                if (pickedActionInd == null)
                {
                    if (HandlePause(gameHandler))
                        continue;
                    else
                        return false;
                }

                switch (pickedActionInd)
                {
                    case 0:
                        int? pickedMonsterInd = Picker.Component(fightEvent.Monsters, CursorPos.MainZoneTop);
                        if (pickedMonsterInd == null)
                            continue;

                        gameHandler.Player.Attack(fightEvent.Monsters[(int) pickedMonsterInd]);
                        break;

                    case 1:
                        PrintSubZone(gameHandler.Player.Skills, "Skills:");

                        int? pickedSkillInd = Picker.Component(gameHandler.Player.Skills, CursorPos.SubZoneTop + 1);
                        if (pickedSkillInd == null)
                            continue;

                        Skill pickedSkill = gameHandler.Player.Skills[(int) pickedSkillInd];
                        if (pickedSkill.MPCost > gameHandler.Player.MP)
                        {
                            Popup("Not enough MP!");
                            continue;
                        }

                        if (pickedSkill.DamagePoint > 0)
                        {
                            switch (pickedSkill.SkillType)
                            {
                                case Skill.Type.Single:
                                    pickedMonsterInd = Picker.Component(fightEvent.Monsters, CursorPos.MainZoneTop + 1);
                                    if (pickedMonsterInd == null)
                                        continue;

                                    gameHandler.Player.UseSkill(pickedSkill, [fightEvent.Monsters[(int) pickedMonsterInd]]);
                                    break;

                                case Skill.Type.Random:
                                    gameHandler.Player.UseSkill(pickedSkill, [fightEvent.Monsters[RandomNumberGenerator.GetInt32(fightEvent.Monsters.Count - 1)]]);
                                    break;

                                case Skill.Type.All:
                                    gameHandler.Player.UseSkill(pickedSkill, fightEvent.Monsters);
                                    break;
                            }
                        }
                        else
                            gameHandler.Player.UseSkill<Monster>(pickedSkill, []);
                        break;

                    default:
                        continue;
                }

                fightEvent.Monsters.RemoveAll(monster => monster.HP == 0);

                if (gameHandler.Player.HP <= 0)
                {
                    LostScreen(lostOptions);

                    int? pickLostOptInd = Picker.String(lostOptions, CursorPos.EndScreenMenuTop, CursorPos.EndScreenMenuLeft);
                    switch (pickLostOptInd)
                    {
                        case 0:
                            gameHandler.LoadPrefightState(fightEvent);
                            continue;

                        default:
                            return false;
                    }
                }
            }

            return HandleRewards(gameHandler, fightEvent.Rewards);
        }

        static bool HandleNonfightEvent(GameLoopHandler gameHandler, Event route)
        {
            switch (route)
            {
                case ShopEvent shopEvent:
                    HandleShop(gameHandler, shopEvent);
                    break;

                case TreasureEvent treasureEvent:
                    TreasureOpening(gameHandler.Progress);
                    if (!HandleRewards(gameHandler, treasureEvent.Treasures))
                        return false;
                    break;

                case RandomEvent randomEvent:
                    if (!HandleEvent(gameHandler, randomEvent.ChildEvent))
                        return false;
                    break;

                default:
                    switch (route.EventType)
                    {
                        case Event.Type.Camp:
                            gameHandler.Player.Regenerate();
                            CampfireScreen(gameHandler.Progress, gameHandler.Player);
                            break;
                    }
                    break;
            }

            return true;
        }

        static bool HandleRewards(GameLoopHandler gameHandler, List<Item> rewards)
        {
            while (rewards.Count > 0)
            {
                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                PrintMainZone(rewards, "Rewards:");

                int? pickedRewardInd = Picker.Component(rewards, CursorPos.MainZoneTop + 1);
                if (pickedRewardInd == null)
                    if (HandlePause(gameHandler))
                        continue;
                    else
                        return false;

                Item pickedReward = rewards[(int) pickedRewardInd];

                gameHandler.Player.AddItem(pickedReward);
                rewards.Remove(pickedReward);
            }

            return true;
        }

        static void HandleShop(GameLoopHandler gameHandler, ShopEvent shopEvent)
        {
            List<string> shopActions = ["Buy", "Sell", "Inventory"];
            List<string> invActions = ["Change Equipment", "Change Skill"];

            while (true)
            {
                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                ShopBanner();
                PrintSubZone(shopActions, "Actions:");
                
                switch (Picker.String(shopActions, CursorPos.SubZoneTop + 1))
                {
                    case 0:
                        HandleShopBuy(gameHandler, shopEvent);
                        break;

                    case 1:
                        HandleShopSell(gameHandler);
                        break;

                    case 2:
                        GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                        ShopBanner();
                        PrintSubZone(invActions, "Inventory:");

                        int? pickedInvInd = Picker.String(invActions, CursorPos.SubZoneTop + 1);
                        switch (pickedInvInd)
                        {
                            case 0:
                                HandleEquipInventory(gameHandler);
                                break;

                            case 1:
                                HandleSkillInventory(gameHandler);
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        return;
                }
            }
        }

        static void HandleShopBuy(GameLoopHandler gameHandler, ShopEvent shopEvent)
        {
            List<Item> shopItems = shopEvent.SellingItems;
            int displayCount = UIConstants.MainZoneHeight + UIConstants.SubZoneHeight;

            while (true)
            {
                ShopTradingScreen(gameHandler.Progress, gameHandler.Player, shopItems, true);

                int? pickedItemInd = Picker.TradeItem(shopItems, true, CursorPos.MainZoneTop + 1, 0, displayCount);
                if (pickedItemInd == null)
                    return;

                Item pickedItem = shopItems[(int) pickedItemInd];
                if (pickedItem.Price <= gameHandler.Player.PlayerGold.Quantity)
                {
                    gameHandler.Player.TradeItem(pickedItem, true);
                    shopItems.Remove(pickedItem);
                }
                else
                    Popup("Not enough gold!");
            }
        }

        static void HandleShopSell(GameLoopHandler gameHandler)
        {
            List<Item> playerInv = [];
            playerInv.AddRange(gameHandler.Player.EquipInventory);
            playerInv.AddRange(gameHandler.Player.SkillInventory);

            int displayCount = UIConstants.MainZoneHeight + UIConstants.SubZoneHeight;

            while (true)
            {
                ShopTradingScreen(gameHandler.Progress, gameHandler.Player, playerInv, false);

                int? pickedItemInd = Picker.TradeItem(playerInv, false, CursorPos.MainZoneTop + 1, 0, displayCount);
                if (pickedItemInd == null)
                    return;

                Item pickedItem = playerInv[(int) pickedItemInd];
                gameHandler.Player.TradeItem(pickedItem, false);
                playerInv.Remove(pickedItem);
            }
        }

        static void HandleVictory(GameLoopHandler gameHandler)
        {
            gameHandler.RunWin();

            List<string> winOptions = ["RETURN TO TITLE"];

            VictoryScreen(gameHandler.GetElapsedTime(), winOptions);

            switch (Picker.String(winOptions, CursorPos.TitleScreenMenuLeft, CursorPos.EndScreenMenuTop))
            {
                default:
                    return;
            }
        }
    }
}