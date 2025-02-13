using System.Security.Cryptography;
using BLL.Game;
using BLL.Game.Components.Others;
using BLL.Game.Components.Item;
using BLL.Game.Components.Event;
using BLL.Game.Components.Entity;
using BLL.Server;

using static ConsolePL.ProgramUI;
using static ConsolePL.ConsoleUtilities;

namespace ConsolePL
{
    class Program
    {
        private static ServerHandler ServerHandler => ServerHandler.Instance;

        #region Main
        public static void Main()
        {
            Console.CursorVisible = false;
            Console.Clear();
            Console.SetWindowSize(UIConstants.UIWidth + 1, UIConstants.UIHeight + 1);

            do
            {
                ConsoleSizeNotice();
            }
            while (Console.WindowHeight < UIConstants.UIHeight + 1 && Console.WindowWidth < UIConstants.UIWidth + 1);

            while (true)
            {
                Console.Clear();
                Console.CursorVisible = false;

                try
                {
                    if (!ModeSelection())
                        return;
                }
                catch (Exception ex)
                {
                    StopTitleAnim();
                    
                    Console.Clear();
                    Console.WriteLine($"Game crashed: {ex.Message}");
                    Console.ReadKey(true);
                }
                finally
                {
                    if (ServerHandler.IsConnected)
                        ServerHandler.Close();
                }
            }
        }
        #endregion

        #region Menu Screens
        static bool ModeSelection()
        {
            List<string> options = ["GUIDE", "PLAY ONLINE", "PLAY OFFLINE", "EXIT"];

            while (true)
            {
                TitleScreenDrawBorders(false, true);
                StartTitleAnim();

                switch (OptionPicker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
                {
                    case 0:
                        Guide();
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

        static void Guide()
        {
            List<string> options = ["NAVIGATING UI", "GAMEPLAY", "ABOUT ONLINE MODE", "RETURN"];

            while (true)
            {
                TitleScreenDrawBorders(false, true);
                StartTitleAnim();

                switch (OptionPicker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
                {
                    case 0:
                        GuideForNavigatingUI();
                        break;

                    case 1:
                        GuideForGameplay();
                        break;

                    case 2:
                        GuideForOnlineMode();
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

                switch (OptionPicker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
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
                
                switch (OptionPicker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
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
            List<string> options = ["NEW GAME", "LOAD GAME", "VIEW SCORE", "MANAGE ACCOUNT", "LOG OUT"];

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
                    Console.Write($"Welcome, {ServerHandler.Nickname}!");
                }

                switch (OptionPicker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
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

                    case 3:
                        ManageAccount(out bool deletedAccount);
                        if (deletedAccount)
                            return;
                        break;

                    case 4: case null:
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

                if (!ServerHandler.GetAllScores(out List<string> personal, out List<string> monthly, out List<string> alltime, out string error))
                {
                    Popup(error);
                    return;
                }
                
                TitleScreenDrawBorders(false, true);
                StartTitleAnim();

                switch (OptionPicker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
                {
                    case 0:
                        ViewScoresScreen(personal, "PERSONAL SCORES");
                        break;

                    case 1:
                        ViewScoresScreen(monthly, "MONTHLY LEADERBOARD");
                        break;

                    case 2:
                        ViewScoresScreen(alltime, "ALL TIME LEADERBOARD");
                        break;

                    case 3: case null:
                        return;
                }
            }
        }

        static void ManageAccount(out bool deletedAccount)
        {
            List<string> options = ["VIEW ACCOUNT INFO", "CHANGE PASSWORD", "CHANGE NICKNAME", "CHANGE EMAIL", "DELETE ACCOUNT", "RETURN"];
            
            deletedAccount = false;

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
                    Console.Write($"Welcome, {ServerHandler.Nickname}!");
                }

                switch (OptionPicker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
                {
                    case 0:
                        ViewAccountInfoScreen();
                        break;

                    case 1:
                        ChangePasswordScreen();
                        break;

                    case 2:
                        ChangeNicknameScreen();
                        break;

                    case 3:
                        ChangeEmailScreen();
                        break;

                    case 4:
                        if (DeleteAccountScreen())
                        {
                            deletedAccount = true;
                            return;
                        }
                        break;

                    case 5: case null:
                        return;
                }
            }
        }

        static void LoadGame()
        {
            if (ServerHandler.IsLoggedIn)
                if (!ServerHandler.DownloadSave(out string downloadSaveError))
                    Popup(downloadSaveError);

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

                int? pickedSaveInd = OptionPicker.Component(saves, CursorPos.TitleScreenMenuTop + 1);
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

                switch (OptionPicker.String(options, CursorPos.TitleScreenMenuTop, CursorPos.TitleScreenMenuLeft))
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
        #endregion

        #region Game Screens
        static void StartNewRun(string? seed = null)
        {
            StopTitleAnim();

            GameHandler gameHandler = new(seed);

            if (ServerHandler.IsLoggedIn)
                gameHandler.Player.Name = ServerHandler.Nickname;

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
            
            int? pickedEquipInd = OptionPicker.Component(startEquips, CursorPos.MainZoneTop + 1);
            if (pickedEquipInd == null)
                return;

            gameHandler.Player.AddItem(new Equipment(startEquips[(int) pickedEquipInd]));

            GenericGameScreen(gameHandler.Progress, gameHandler.Player);
            PrintMainZone(startSkills, "Choose Starting Skill:");
            
            int? pickedSkillInd = OptionPicker.Component(startSkills, CursorPos.MainZoneTop + 1);
            if (pickedSkillInd == null)
                return;
                
            gameHandler.Player.AddItem(new Skill(startSkills[(int) pickedSkillInd]));
            gameHandler.Progress.Next();
            GameLoop(gameHandler);
        }

        static void GameLoop(GameHandler gameHandler)
        {
            List<string> actions = ["Pick A Route", "Inventory"];
            List<string> invOptions = ["Change Equipment", "Change Skill"];

            if (ServerHandler.IsLoggedIn)
                gameHandler.Player.Name = ServerHandler.Nickname;

            gameHandler.Timer(true);

            while (true)
            {
                gameHandler.SaveAs("AutoSave");

                List<GameEvent> routes = gameHandler.GetEvents();

                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                PrintMainZone(routes, "Routes:");
                PrintSubZone(actions, "Actions:");

                int? pickedActionInd = OptionPicker.String(actions, CursorPos.SubZoneTop + 1);
                switch (pickedActionInd)
                {
                    case 0:
                        int? pickedRouteInd = OptionPicker.Component(routes, CursorPos.MainZoneTop + 1);
                        if (pickedRouteInd == null)
                            break;

                        GameEvent pickedRoute = routes[(int) pickedRouteInd];
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

                        int? pickedInvInd = OptionPicker.String(invOptions, CursorPos.SubZoneTop + 1);
                        switch (pickedInvInd)
                        {
                            case 0:
                                HandleEquipInventory(gameHandler);
                                break;

                            case 1:
                                HandleSkillInventory(gameHandler);
                                break;

                            default:
                                continue;
                        }
                        goto case 1;

                    default:
                        if (HandlePause(gameHandler))
                            break;
                        else
                            return;
                }
            }
        }

        static bool HandlePause(GameHandler gameHandler)
        {
            List<string> pauseOptions = ["RESUME", "SAVE & EXIT"];

            gameHandler.Timer(false);
            PausePopup(pauseOptions, gameHandler.GetElapsedTime());

            int? pickedPauseInd = OptionPicker.String(pauseOptions, CursorPos.PauseMenuTop, CursorPos.PauseMenuLeft, pauseOptions.Count, 4);
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

        static void HandleEquipInventory(GameHandler gameHandler)
        {
            List<Equipment> equipInv = gameHandler.Player.EquipInventory;

            while (true)
            {
                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                PrintMainZone([], "Inventory:");
                PrintSubZone(gameHandler.Player.Equipped, "Currently Equipped:");

                int? pickedEquipInd = OptionPicker.Component(equipInv, CursorPos.MainZoneTop + 1, 0, UIConstants.MainZoneHeight - 1);
                if (pickedEquipInd == null)
                    return;

                Equipment pickedEquip = equipInv[(int) pickedEquipInd];
                gameHandler.Player.ChangeEquip(pickedEquip);
                equipInv = gameHandler.Player.EquipInventory;
            }
        }

        static void HandleSkillInventory(GameHandler gameHandler)
        {
            List<Skill> skillInv = gameHandler.Player.SkillInventory;
            int displayCount = UIConstants.MainZoneHeight - 1;

            while (true)
            {
                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                PrintMainZone([], "Inventory:");
                PrintSubZone(gameHandler.Player.Skills, "Currently Equipped:");

                int? pickedSkillInd = OptionPicker.Component(skillInv, CursorPos.MainZoneTop + 1, 0, displayCount);
                if (pickedSkillInd == null)
                    return;

                Skill pickedSkill = skillInv[(int) pickedSkillInd];

                if (gameHandler.Player.Skills.Count < 3)
                {
                    gameHandler.Player.AddItem(pickedSkill);
                }
                else
                {
                    int? equippedSkillInd = OptionPicker.Component(gameHandler.Player.Skills, CursorPos.SubZoneTop + 1);
                    if (equippedSkillInd != null)
                        gameHandler.Player.ChangeSkill((int) equippedSkillInd, pickedSkill);
                }

                skillInv = gameHandler.Player.SkillInventory;
            }
        }

        static bool HandleEvent(GameHandler gameHandler, GameEvent route)
        {
            return route switch
            {
                FightEvent fightEvent => HandleFightEvent(gameHandler, fightEvent),
                _ => HandleNonfightEvent(gameHandler, route),
            };
        }

        static bool HandleFightEvent(GameHandler gameHandler, FightEvent fightEvent)
        {
            List<string> fightActions = ["Attack", "Use Skill"];
            List<string> lostOptions = ["RETRY", "RETURN TO TITLE"];

            gameHandler.SavePrefightState(fightEvent);

            while (fightEvent.Monsters.Count > 0)
            {
                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                PrintMainZone(fightEvent.Monsters);
                PrintSubZone(fightActions, "Actions:");

                int? pickedActionInd = OptionPicker.String(fightActions, CursorPos.SubZoneTop + 1);
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
                        int? pickedMonsterInd = OptionPicker.Component(fightEvent.Monsters, CursorPos.MainZoneTop);
                        if (pickedMonsterInd == null)
                            continue;

                        gameHandler.Player.Attack(fightEvent.Monsters[(int) pickedMonsterInd]);
                        break;

                    case 1:
                        PrintSubZone(gameHandler.Player.Skills, "Skills:");

                        int? pickedSkillInd = OptionPicker.Component(gameHandler.Player.Skills, CursorPos.SubZoneTop + 1);
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
                                    pickedMonsterInd = OptionPicker.Component(fightEvent.Monsters, CursorPos.MainZoneTop);
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

                    int? pickLostOptInd = OptionPicker.String(lostOptions, CursorPos.EndScreenMenuTop, CursorPos.EndScreenMenuLeft);
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

        static bool HandleNonfightEvent(GameHandler gameHandler, GameEvent route)
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
                        case GameEvent.Type.Camp:
                            gameHandler.Player.Regenerate();
                            CampfireScreen(gameHandler.Progress, gameHandler.Player);
                            break;
                    }
                    break;
            }

            return true;
        }

        static bool HandleRewards(GameHandler gameHandler, List<GameItem> rewards)
        {
            while (rewards.Count > 0)
            {
                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                PrintMainZone(rewards, "Rewards:");

                int? pickedRewardInd = OptionPicker.Component(rewards, CursorPos.MainZoneTop + 1);
                if (pickedRewardInd == null)
                    if (HandlePause(gameHandler))
                        continue;
                    else
                        return false;

                GameItem pickedReward = rewards[(int) pickedRewardInd];

                gameHandler.Player.AddItem(pickedReward);
                rewards.Remove(pickedReward);
            }

            return true;
        }

        static void HandleShop(GameHandler gameHandler, ShopEvent shopEvent)
        {
            List<string> options = ["Shopping", "Inventory", "Continue"];
            List<string> shopActions = ["Buy", "Sell", $"Reroll Shop ({gameHandler.RerollCost}G)"];
            List<string> invActions = ["Change Equipment", "Change Skill"];

            while (true)
            {
                GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                ShopBanner();
                PrintSubZone(options, "Actions:");
                
                switch (OptionPicker.String(options, CursorPos.SubZoneTop + 1))
                {
                    case 0:
                        GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                        ShopBanner();
                        PrintSubZone(shopActions, "Shopping:");
                        
                        int? pickedShopInd = OptionPicker.String(shopActions, CursorPos.SubZoneTop + 1);
                        switch (pickedShopInd)
                        {
                            case 0:
                                HandleShopBuy(gameHandler, shopEvent);
                                break;

                            case 1:
                                HandleShopSell(gameHandler);
                                break;

                            case 2:
                                gameHandler.RerollShop(shopEvent);
                                break;

                            default:
                                continue;
                        }
                        goto case 0;

                    case 1:
                        GenericGameScreen(gameHandler.Progress, gameHandler.Player);
                        ShopBanner();
                        PrintSubZone(invActions, "Inventory:");

                        int? pickedInvInd = OptionPicker.String(invActions, CursorPos.SubZoneTop + 1);
                        switch (pickedInvInd)
                        {
                            case 0:
                                HandleEquipInventory(gameHandler);
                                break;

                            case 1:
                                HandleSkillInventory(gameHandler);
                                break;

                            default:
                                continue;
                        }
                        goto case 1;

                    default:
                        return;
                }
            }
        }

        static void HandleShopBuy(GameHandler gameHandler, ShopEvent shopEvent)
        {
            List<GameItem> shopItems = shopEvent.SellingItems;
            int displayCount = UIConstants.MainZoneHeight + UIConstants.SubZoneHeight;

            while (true)
            {
                ShopTradingScreen(gameHandler.Progress, gameHandler.Player, shopItems, true);

                int? pickedItemInd = OptionPicker.TradeItem(shopItems, true, CursorPos.MainZoneTop + 1, 0, displayCount);
                if (pickedItemInd == null)
                    return;

                GameItem pickedItem = shopItems[(int) pickedItemInd];
                if (pickedItem.Price <= gameHandler.Player.Gold.Quantity)
                {
                    gameHandler.Player.TradeItem(pickedItem, true);
                    shopItems.Remove(pickedItem);
                }
                else
                    Popup("Not enough gold!");
            }
        }

        static void HandleShopSell(GameHandler gameHandler)
        {
            List<GameItem> playerInv = [];
            playerInv.AddRange(gameHandler.Player.EquipInventory);
            playerInv.AddRange(gameHandler.Player.SkillInventory);

            int displayCount = UIConstants.MainZoneHeight + UIConstants.SubZoneHeight;

            while (true)
            {
                ShopTradingScreen(gameHandler.Progress, gameHandler.Player, playerInv, false);

                int? pickedItemInd = OptionPicker.TradeItem(playerInv, false, CursorPos.MainZoneTop + 1, 0, displayCount);
                if (pickedItemInd == null)
                    return;

                GameItem pickedItem = playerInv[(int) pickedItemInd];
                gameHandler.Player.TradeItem(pickedItem, false);
                playerInv.Remove(pickedItem);
            }
        }

        static void HandleVictory(GameHandler gameHandler)
        {
            gameHandler.RunWin();

            List<string> winOptions = ["RETURN TO TITLE"];

            VictoryScreen(gameHandler.GetElapsedTime(), winOptions, ServerHandler.IsLoggedIn);

            switch (OptionPicker.String(winOptions, CursorPos.EndScreenMenuTop + 3, CursorPos.EndScreenMenuLeft))
            {
                default:
                    return;
            }
        }
        #endregion
    }
}