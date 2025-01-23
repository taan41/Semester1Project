using System.Security.Cryptography;
using BLL.GameComponents;
using BLL.GameComponents.ItemComponents;
using BLL.GameComponents.EntityComponents;
using BLL.GameComponents.Others;
using BLL.GameHandlers;
using DAL;
using DAL.ConfigClasses;

using static System.Console;
using static ConsolePL.ConsoleHelper;
using static ConsolePL.ComponentPrinter;

namespace ConsolePL
{
    public static class GameScreens
    {
        public const string GameTitle = "CONSOLE CONQUER";

        private static DatabaseConfig DbConfig => ConfigManager.Instance.DatabaseConfig;
        private static CancellationTokenSource? titleAnimTokenSource = null;

        private static readonly string[] background = [
            "₁       ₀₀₁₁     ₁₁ ₁₁         ₀₁  ₁ ₀₀₁  ₁   ₁₀₀₁₁    ₀₀₁          ₁₀",
            "₀₁₀₀      ₀₁   ₀₁     ₁    ₀₁    ₁   ₀₁₁₁         ₀₀₁   ₁₀ ₁₁       ₀₀",
            "  ₁ ₁ ₁₀     ₁₁      ₁₀      ₀₁       ₁  ₁₁₁₁      ₁₀       ₁₀   ₁ ₁₁ ",
            "₀₁    ₁₀₁₁₀        ₀₀   ₀₁₁₀      ₀₁₁     ₁     ₀₀₁  ₁        ₀₀   ₁₁₀",
            "₁      ₁₁₀₀    ₁   ₁₀₀ ₁₁    ₀₁ ₁     ₀  ₁₁₀      ₀₁₁₀  ₀₁    ₀₁    ₁₀",
            "₀    ₁  ₁₀₁₁₀       ₁   ₀₁₀   ₀₀₁₁        ₁₀₁  ₀₀₀₁₁      ₀₁  ₀₀₁   ₀₁",
            "₀₀       ₁₁    ₀₁     ₀₁₀₀₁   ₁₀     ₁  ₁₁₀₁     ₀₀  ₁   ₁₀₀₁       ₀₁",
            "₀₁₁    ₀₁₁₀₀   ₁  ₀₁₀ ₀₁       ₁   ₁₀₁₁     ₀₁₁   ₁      ₀₀₀₁₁      ₀₀",
            "₀₀   ₁     ₁₀₁  ₁₀   ₁ ₀₁  ₀   ₀₁₁   ₀₁₁₀₁  ₀₀₀₁   ₁₀₁           ₁  ₀₁"
        ];

        private static readonly string[][] titles = [
            [
                "",
                "    ╔═╗ ╔═╗ ╦╗╦ ╔═╗ ╔═╗ ╦   ╔═╗    ",
                "    ║   ║ ║ ║║║ ╚═╗ ║ ║ ║   ╠╣     ",
                "    ╚═╝ ╚═╝ ╩╚╝ ╚═╝ ╚═╝ ╩═╝ ╚═╝    ",
                "",
                "    ╔═╗ ╔═╗ ╦╗╦ ╔═╗ ╦ ╦ ╔═╗ ╦═╗    ",
                "    ║   ║ ║ ║║║ ║╔╣ ║ ║ ╠╣  ╠╦╝    ",
                "    ╚═╝ ╚═╝ ╩╚╝ ╚╩╩ ╚═╝ ╩═╝ ╩╚═    ",
                ""
            ],
            [
                @"      ____ ___  _  __ ___ ___  __   ____    ",
                @"    / ___/ __ \/ |/ / __/ __ \/ /  / __/    ",
                @"   / /__/ /_/ /    /\ \/ /_/ / /__/ _/      ",
                @"   \___/\____/_/|_/___/\____/____/___/      ",
                "",
                @"      ____ ___  _  __ ___  __  __ ___ __    ",
                @"    / ___/ __ \/ |/ / __ \/ / / / __/ _ \   ",
                @"   / /__/ /_/ /    / /_/ / /_/ / _// , _/   ",
                @"   \___/\____/_/|_/\___\_\____/___/_/|_|    ",
            ]
        ];

        private static async Task TitleAnimation(CancellationToken stopToken)
        {
            int lineIndex = 0, lineLength = background[0].Length;
            int randomTitle = RandomNumberGenerator.GetInt32(titles.Length);

            try
            {
                while(!stopToken.IsCancellationRequested)
                {
                    lock (ConsoleLock)
                    {
                        SetCursorPosition(0, CursorPos.TitleAnimationTop);
                        for (int i = 0; i < background.Length; i++)
                        {
                            Write($"{background[i][lineIndex..]}{background[i][0..lineIndex]}");
                            WriteCenter(titles[randomTitle][i]);
                        }
                    }

                    lineIndex++;
                    lineIndex %= lineLength;
                    
                    await Task.Delay(400, stopToken);
                }
            }
            catch (OperationCanceledException) {}
        }

        public static void StartTitleAnim()
        {
            if (titleAnimTokenSource != null)
                return;

            titleAnimTokenSource = new();
            _ = Task.Run(() => TitleAnimation(titleAnimTokenSource.Token));
        }

        public static void StopTitleAnim()
        {
            if (titleAnimTokenSource == null)
                return;

            titleAnimTokenSource.Cancel();
            titleAnimTokenSource = null;
        }

        public static void ConsoleSizeNotice()
        {
            Clear();
            DrawLine();
            for (int i = 1; i < CursorPos.BottomBorderTop; i++)
                WriteLine($"| {i}");
            DrawLine();

            CursorLeft = 5;
            CursorTop -= 3;
            Write("-- Please resize the console window to fit the game screen");
            CursorLeft = 5;
            CursorTop++;
            Write("-- Press any key to continue...");
            ReadKey(true);
        }

        public static void DrawHeader(bool setCursor = false)
        {
            if (setCursor)
                SetCursorPosition(0, 0);

            DrawLine('=');
            ForegroundColor = ConsoleColor.Cyan;
            WriteCenter(GameTitle);
            ResetColor();
            DrawLine('=');
        }

        public enum PopupType
        {
            Warning,
            Success
        }

        public static void Popup(string msg, PopupType popupType = PopupType.Warning)
        {
            string[] msgLines = msg.Split('\n');

            StopTitleAnim();
            CursorTop = CursorPos.PopupTop;
            WriteCenter($"╔{new string('═', UIConstants.PopupWidth)}╗");
            for (int i = 0; i < UIConstants.PopupHeight + msgLines.Length - 3; i++)
            {
                WriteCenter($"║{new string(' ', UIConstants.PopupWidth)}║");
            }
            WriteCenter($"╚{new string('═', UIConstants.PopupWidth)}╝");

            CursorTop = CursorPos.PopupTop + 2;
            if (popupType == PopupType.Warning)
            {
                WriteCenter(@"   .   ");
                WriteCenter(@"  ╱ ╲  ");
                WriteCenter(@" ╱ ┃ ╲ ");
                WriteCenter(@"╱  •  ╲");
                WriteCenter(@"‾‾‾‾‾‾‾");
            }
            else if (popupType == PopupType.Success)
            {
                WriteCenter(@"┌            ┐");
                WriteCenter(@"│        ╱   │");
                WriteCenter(@"│    ╲  ╱    │");
                WriteCenter(@"│     ╲╱     │");
                WriteCenter(@"└            ┘");
            }

            CursorTop++;
            foreach (var line in msgLines)
                WriteCenter(line);
            ReadKey(true);
            Clear();
        }

        public static void PausePopup(List<string> pauseOptions, TimeSpan? elapsedTime = null)
        {
            CursorTop = CursorPos.PausePopupTop;
            WriteCenter($"╔{new string('═', UIConstants.PausePopupWidth)}╗");
            for (int i = 0; i < UIConstants.PausePopupHeight - 2; i++)
                WriteCenter($"║{new string(' ', UIConstants.PausePopupWidth)}║");
            WriteCenter($"╚{new string('═', UIConstants.PausePopupWidth)}╝");

            CursorTop = CursorPos.PauseMenuTop;
            foreach(var option in pauseOptions)
            {
                CursorLeft = CursorPos.PauseMenuLeft;
                WriteLine(option);
            }

            if (elapsedTime != null)
            {
                CursorTop = CursorPos.PauseElapsedTimeTop;
                WriteCenter("Run Time:");
                WriteCenter($"{elapsedTime:hh\\:mm\\:ss\\.fff}");
            }
        }

        public static void TitleScreenDrawBorders(bool clear = true, bool clearOptionsZone = false)
        {
            lock (ConsoleLock)
            {
                if (clear)
                    Clear();
                else
                    SetCursorPosition(0, 0);

                DrawLine('=');
                CursorTop += 9;
                DrawLine('=');

                if (clearOptionsZone)
                {
                    do
                        WriteLine(new string(' ', UIConstants.UIWidth));
                    while (CursorTop < CursorPos.BottomBorderTop);
                }
                else
                    CursorTop = CursorPos.BottomBorderTop;

                DrawLine();
            }
        }

        public static void TitleScreenDrawOptions(List<string> options)
        {
            CursorTop = CursorPos.TitleScreenMenuTop;
            foreach (var option in options)
            {
                CursorLeft = CursorPos.TitleScreenMenuLeft;
                WriteLine($" {option,-(UIConstants.UIWidth - CursorPos.TitleScreenMenuLeft - 1)}");
            }
        }

        public static void ServerConnectScreen()
        {
            TitleScreenDrawBorders(false, true);
            int lastCursorTop;

            lock (ConsoleLock)
            {
                SetCursorPosition(CursorPos.TitleScreenMenuLeft2, CursorPos.TitleScreenMenuTop);
                WriteLine(" Connecting to server...");
                lastCursorTop = CursorTop;
            }

            if (!ServerHandler.Connect(out string error))
            {
                Popup(error);
                return;
            }

            lock (ConsoleLock)
            {
                SetCursorPosition(CursorPos.TitleScreenMenuLeft2, lastCursorTop++);
                WriteLine(" Updating game configs...");
                lastCursorTop = CursorTop;
            }

            if (!ServerHandler.UpdateConfig(out error))
            {
                Popup(error);
                return;
            }

            lock (ConsoleLock)
            {
                SetCursorPosition(CursorPos.TitleScreenMenuLeft2, lastCursorTop++);
                WriteLine(" Updating game assets...");
                lastCursorTop = CursorTop;
            }

            if (!ServerHandler.UpdateAssets(out error))
            {
                Popup(error);
                return;
            }

            lock (ConsoleLock)
            {
                SetCursorPosition(CursorPos.TitleScreenMenuLeft2, lastCursorTop++);
                WriteLine(" Updated game data successfully!");
                SetCursorPosition(CursorPos.TitleScreenMenuLeft2, CursorTop + 2);
                WriteLine(" Press any key to continue...");
            }

            ReadKey(true);
        }

        public static void RegisterScreen()
        {
            string?
                username = null,
                nickname = null,
                password = null,
                confirmPassword = null,
                email = null,
                error;
            int tempCursorLeft, tempCursorTop;

            while (true)
            {
                if (!ServerHandler.IsConnected)
                    return;

                TitleScreenDrawBorders(false, true);
                StartTitleAnim();

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop;
                    WriteLine(" 'ESC' to return");
                    Write(" Username: ");
                    (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                    if (username != null)
                        WriteLine(username);
                }
                
                if (username == null)
                {
                    username = ReadInput(tempCursorLeft, tempCursorTop, false, DbConfig.UsernameMax);

                    if (username == null)
                        return;
                    
                    if (string.IsNullOrWhiteSpace(username) || username.Length < DbConfig.UsernameMin)
                    {
                        Popup($"Must be {DbConfig.UsernameMin} ~ {DbConfig.UsernameMax} characters");
                        username = null;
                        continue;
                    }

                    if (!ServerHandler.CheckUsername(username, out error))
                    {
                        Popup(error);
                        username = null;
                        continue;
                    }
                }

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop + 2;
                    Write(" Password: ");
                    (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                    if (password != null)
                        WriteLine(new string('*', password.Length));
                }

                if (password == null)
                {
                    password = ReadInput(tempCursorLeft, tempCursorTop, true, DbConfig.PasswordMax);
                    
                    if (password == null)
                        return;

                    if (string.IsNullOrWhiteSpace(password) || password.Length < DbConfig.PasswordMin)
                    {
                        Popup($"Must be {DbConfig.PasswordMin} ~ {DbConfig.PasswordMax} characters");
                        password = null;
                        continue;
                    }
                }

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop + 3;
                    Write(" Confirm Password: ");
                    (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                    if (confirmPassword != null)
                        WriteLine(new string('*', confirmPassword.Length));
                }

                if (confirmPassword == null)
                {
                    CursorTop = tempCursorTop;
                    confirmPassword = ReadInput(tempCursorLeft, tempCursorTop, true, DbConfig.PasswordMax);
                    
                    if (confirmPassword == null)
                        return;

                    if (password != confirmPassword)
                    {
                        Popup("Passwords do not match!");
                        password = null;
                        confirmPassword = null;
                        continue;
                    }
                }

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop + 4;
                    Write(" Nickname: ");
                    (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                    if (nickname != null)
                        WriteLine(nickname);
                }

                if (nickname == null)
                {
                    nickname = ReadInput(tempCursorLeft, tempCursorTop, false, DbConfig.NicknameMax);
                    
                    if (nickname == null)
                        return;

                    if (string.IsNullOrWhiteSpace(nickname) || nickname.Length < DbConfig.NicknameMin)
                    {
                        Popup($"Must be {DbConfig.NicknameMin} ~ {DbConfig.NicknameMax} characters");
                        nickname = null;
                        continue;
                    }
                }

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop + 5;
                    Write(" Email: ");
                    (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                    if (email != null)
                        WriteLine(email);
                }

                if (email == null)
                {
                    email = ReadInput(tempCursorLeft, tempCursorTop, false, DbConfig.EmailMax);
                    
                    if (email == null)
                        return;

                    if (string.IsNullOrWhiteSpace(email) || email.Length < DbConfig.EmailMin)
                    {
                        Popup($"Invalid format");
                        email = null;
                        continue;
                    }
                }

                if (!ServerHandler.Register(username, nickname, password, email, out error))
                {
                    Popup(error);
                    continue;
                }
                else
                {
                    Popup("Registration Successful!", PopupType.Success);
                    return;
                }

            }
        }

        public static void ResetPasswordScreen()
        {
            string?
                username = null,
                email = null,
                password = null,
                confirmPassword = null;
            int tempCursorLeft, tempCursorTop;

            while (true)
            {
                if (!ServerHandler.IsConnected)
                    return;

                TitleScreenDrawBorders(false, true);
                StartTitleAnim();

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop;
                    WriteLine(" 'ESC' to return");
                    Write(" Username: ");
                    (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                    if (username != null)
                        WriteLine(username);
                }

                if (username == null)
                {
                    username = ReadInput(tempCursorLeft, tempCursorTop, false, DbConfig.UsernameMax);

                    if (username == null)
                        return;

                    if (string.IsNullOrWhiteSpace(username) || username.Length < DbConfig.UsernameMin)
                    {
                        Popup($"Must be {DbConfig.UsernameMin} ~ {DbConfig.UsernameMax} characters");
                        username = null;
                        continue;
                    }
                }

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop + 2;
                    Write(" Email: ");
                    (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                    if (email != null)
                        WriteLine(email);
                }

                if (email == null)
                {
                    email = ReadInput(tempCursorLeft, tempCursorTop, false, DbConfig.EmailMax);

                    if (email == null)
                        return;

                    if (string.IsNullOrWhiteSpace(email) || email.Length < DbConfig.EmailMin)
                    {
                        Popup($"Invalid format");
                        email = null;
                        continue;
                    }
                }

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop + 3;
                    Write(" Password: ");
                    (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                    if (password != null)
                        WriteLine(new string('*', password.Length));
                }

                if (password == null)
                {
                    password = ReadInput(tempCursorLeft, tempCursorTop, true, DbConfig.PasswordMax);

                    if (password == null)
                        return;

                    if (string.IsNullOrWhiteSpace(password) || password.Length < DbConfig.PasswordMin)
                    {
                        Popup($"Must be {DbConfig.PasswordMin} ~ {DbConfig.PasswordMax} characters");
                        password = null;
                        continue;
                    }
                }

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop + 4;
                    Write(" Confirm Password: ");
                    (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                    if (confirmPassword != null)
                        WriteLine(new string('*', confirmPassword.Length));
                }

                if (confirmPassword == null)
                {
                    confirmPassword = ReadInput(tempCursorLeft, tempCursorTop, true, DbConfig.PasswordMax);

                    if (confirmPassword == null)
                        return;

                    if (password != confirmPassword)
                    {
                        Popup("Passwords do not match!");
                        password = null;
                        confirmPassword = null;
                        continue;
                    }
                }

                if (!ServerHandler.ResetPassword(username, email, password, out string? error))
                {
                    Popup(error);
                    continue;
                }
                else
                {
                    Popup("Password Reset Successful!", PopupType.Success);
                    return;
                }
            }
        }

        public static bool LoginScreen()
        {
            int tempCursorLeft, tempCursorTop;

            while (true)
            {
                if (!ServerHandler.IsConnected)
                    return false;

                TitleScreenDrawBorders(false, true);
                StartTitleAnim();
                
                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop;
                    WriteLine(" 'ESC' to return");
                    Write(" Username: ");
                    (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                }

                string? username = ReadInput(tempCursorLeft, tempCursorTop, false, DbConfig.UsernameMax);

                if (username == null)
                    return false;

                if (string.IsNullOrWhiteSpace(username) || username.Length < DbConfig.UsernameMin)
                {
                    Popup($"Must be {DbConfig.UsernameMin} ~ {DbConfig.UsernameMax} characters");
                    continue;
                }

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop + 2;
                    Write(" Password: ");
                    (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                }

                string? password = ReadInput(tempCursorLeft, tempCursorTop, true, DbConfig.PasswordMax);

                if (password == null)
                    return false;
                
                if (string.IsNullOrWhiteSpace(password) || password.Length < DbConfig.PasswordMin)
                {
                    Popup($"Must be {DbConfig.PasswordMin} ~ {DbConfig.PasswordMax} characters");
                    continue;
                }

                if (!ServerHandler.Login(username, password, out string error))
                {
                    Popup(error);
                    continue;
                }
                else
                {
                    Popup("Login Successful!", PopupType.Success);
                    return true;
                }
            }
        }

        public static string? EnterSeed()
        {
            int tempCursorTop;

            lock (ConsoleLock)
            {
                SetCursorPosition(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop + 4);
                WriteLine(" ENTER SEED: ");
                tempCursorTop = CursorTop;
            }

            return ReadInput(15, tempCursorTop, false, 40);
        }

        public static void ViewScoresScreen(List<string> scores, string listName)
        {
            StopTitleAnim();
            Clear();
            DrawHeader();
            CursorTop++;

            WriteLine($" -- {listName}:");
            for (int i = 1; i < 16; i++)
            {
                if (i <= scores.Count)
                    WriteLine($" {i}. {scores[i - 1]}");
                else
                    WriteLine($" {i}.");
            }

            CursorTop = CursorPos.BottomBorderTop;
            DrawLine();
            ReadKey(true);
        }

        public static void GenericGameScreen(RunProgress progress, Player player)
        {
            Clear();
            DrawHeader();
            progress.Print();
            DrawLine();
            CursorTop = CursorPos.SubZoneTop - 1;
            DrawLine();
            CursorTop = CursorPos.PlayerZoneTop - 1;
            DrawLine();
            PrintComponent(player);
            DrawLine();
        }

        public static void PrintMainZone(List<string> options, string? msg = null)
        {
            CursorTop = CursorPos.MainZoneTop;
            for (int i = 0; i < UIConstants.MainZoneHeight; i++)
                DrawEmptyLine();

            CursorTop = CursorPos.MainZoneTop;
            if (msg != null)
                WriteLine($" -- {msg}");
            options.ForEach(option => WriteLine($" {option}"));
        }

        public static void PrintMainZone<T>(List<T> components, string? msg = null) where T : ComponentAbstract
        {
            CursorTop = CursorPos.MainZoneTop;
            for (int i = 0; i < UIConstants.MainZoneHeight; i++)
                DrawEmptyLine();

            CursorTop = CursorPos.MainZoneTop;
            if (msg != null)
                WriteLine($" -- {msg}");
            components.ForEach(PrintComponent);
        }

        public static void PrintSubZone(List<string> options, string? msg = null)
        {
            CursorTop = CursorPos.SubZoneTop;
            for (int i = 0; i < UIConstants.SubZoneHeight; i++)
                DrawEmptyLine();

            CursorTop = CursorPos.SubZoneTop;
            if (msg != null)
                WriteLine($" -- {msg}");
            options.ForEach(option => WriteLine($" {option}"));
        }

        public static void PrintSubZone<T>(List<T> components, string? msg = null) where T : ComponentAbstract
        {
            CursorTop = CursorPos.SubZoneTop;
            for (int i = 0; i < UIConstants.SubZoneHeight; i++)
                DrawEmptyLine();
                
            CursorTop = CursorPos.SubZoneTop;
            if (msg != null)
                WriteLine($" -- {msg}");
            components.ForEach(PrintComponent);
        }

        public static void ShopBanner()
        {
            CursorTop = CursorPos.MainZoneTop;
            WriteCenter(@"   _____________________________                            ");
            WriteCenter(@"  /    _____ _____ _____ _____  \            ┌─────────┐    ");
            WriteCenter(@" /   |   __|  |  |     |  _  |   \           │   ♦$♦   │    ");
            WriteCenter(@"|    |__   |     |  |  |   __|    |      ════╪═════════╪════");
            WriteCenter(@" \   |_____|__|__|_____|__|      /           │ █     █ │    ");
            WriteCenter(@"  \_____________________________/            ;  = ┴ =  ;    ");
        }

        public static void ShopTradingScreen<T>(RunProgress progress, Player player, List<T> items, bool buying) where T : Item
        {
            Clear();
            DrawHeader();
            progress.Print();
            DrawLine();
            WriteLine($" -- {(buying ? "Buying:" : "Selling:")}");
            items.ForEach(item => PrintPrice(item, buying));
            CursorTop = CursorPos.PlayerZoneTop - 1;
            DrawLine();
            PrintComponent(player);
            DrawLine();
        }

        public static void TreasureOpening(RunProgress progress)
        {
            Clear();
            DrawHeader();
            progress.Print();
            DrawLine();
            WriteLine();
            WriteCenter(@"       _ _                                  ");
            WriteCenter(@"    .' '.: '*:=. _                          ");
            WriteCenter(@"  .' .'            '* :=. _                 ");
            WriteCenter(@" /  /                      . *: -__         ");
            WriteCenter(@":  :                     .'  .:     ' .     ");
            WriteCenter(@":  '=._                 /  .'          \    ");
            WriteCenter(@":._     '*:=._         '  /             :   ");
            WriteCenter(@":  +'* =._     '* =._ :  :           _(#)   ");
            WriteCenter(@":O +     : '*:=._     '  ::     _ .=*+  :   ");
            WriteCenter(@":  +     | '@,-:   '* : -..=:*'      + O:   ");
            WriteCenter(@":O +      ' # ,|      +''::' +       +  :   ");
            WriteCenter(@":  +-._               + O::O +       + O:   ");
            WriteCenter(@" *=:._  '+-._         +  ::  +       +  :   ");
            WriteCenter(@"     ''*=:._  '+-._   + O::O +   _.=*'  :   ");
            WriteCenter(@"           ''*=:._  ''+  ::  +*' _.;=*'     ");
            DrawLine();
            ReadKey(true);
            
            Clear();
            DrawHeader();
            progress.Print();
            DrawLine();
            WriteCenter(@"  .* .'_       '*::=._                      ");
            WriteCenter(@" /. _    '*:=-._      ''*::=._              ");
            WriteCenter(@"' * _ '*=-._     '*:=._     :'*::=.__       ");
            WriteCenter(@"     ' =_    '*=-._     '-:'  .*      `.    ");
            WriteCenter(@"          *=_       '*=-._  /            \  ");
            WriteCenter(@"           _(#)_           ' ._           . ");
            WriteCenter(@"     _-+*'₀ ₁ ₀₀ '*=. _         *=_       : ");
            WriteCenter(@".+*' ₁₀ ₁ ₁ ₀₀₁  ₀   ₁ ₁'*=-._      *=_  /  ");
            WriteCenter(@": *+= _₁ ₁  ₀₁₁  ₁ ₀ ₁  ₀₁₁ ₁  ₀'*=-._(#)   ");
            WriteCenter(@":O +    '*+= _ ₁₁ ₀  ₀₁₀ ₁   ₀₀ ₁  _.+' :   ");
            WriteCenter(@":  +     | '@, '*+= _₀₁ ₀ ₀ ₁_.+:'   + O:   ");
            WriteCenter(@":O +      ' # ,|      +''::' +       +  :   ");
            WriteCenter(@":  +-._               + O::O +       + O:   ");
            WriteCenter(@" *=:._  '+-._         +  ::  +       +  :   ");
            WriteCenter(@"     ''*=:._  '+-._   + O::O +   _.=*'  :   ");
            WriteCenter(@"           ''*=:._  ''+  ::  +*' _.;=*'     ");
            DrawLine();
            ReadKey(true);
        }

        public static void LostScreen(List<string> options)
        {
            Clear();
            DrawHeader();
            WriteLine();
            WriteCenter(@" _______ _______ _______ _______      _______ ___ ___ _______ ______ ");
            WriteCenter(@"|     __|   _   |   |   |    ___|    |       |   |   |    ___|   __ \");
            WriteCenter(@"|    |  |       |       |    ___|    |   -   |   |   |    ___|      <");
            WriteCenter(@"|_______|___|___|__|_|__|_______|    |_______|\_____/|_______|___|__|");
            WriteLine();
            WriteLine();
            DrawLine();
            CursorTop += 7;
            DrawLine();

            CursorTop = CursorPos.EndScreenMenuTop;
            foreach (var option in options)
            {
                CursorLeft = CursorPos.EndScreenMenuLeft;
                WriteLine($" {option}        ");
            }
        }

        public static void VictoryScreen(TimeSpan elapsedTime, List<string> options)
        {
            Clear();
            DrawHeader();
            WriteLine();
            WriteCenter(@" ___ ___ _______ ______ _______ _______ ______ ___ ___ __ ");
            WriteCenter(@"|   |   |_     _|      |_     _|       |   __ \   |   |  |");
            WriteCenter(@"|   |   |_|   |_|   ---| |   | |   -   |      <\     /|__|");
            WriteCenter(@" \_____/|_______|______| |___| |_______|___|__| |___| |__|");
            WriteLine();
            WriteLine();
            DrawLine();
            CursorTop += 7;
            DrawLine();

            CursorLeft = CursorPos.EndScreenMenuLeft;
            CursorTop = CursorPos.EndScreenMenuTop;
            WriteLine("RUN DURATION:");
            CursorLeft = CursorPos.EndScreenMenuLeft;
            WriteLine($"{elapsedTime:hh\\:mm\\:ss\\.fff}");
            CursorTop++;
            foreach (var option in options)
            {
                CursorLeft = CursorPos.EndScreenMenuLeft;
                WriteLine($" {option}        ");
            }
        }

        public static void CampfireScreen(RunProgress progress, Player player)
        {
            Clear();
            DrawHeader();
            progress.Print();
            DrawLine();
            CursorTop = CursorPos.PlayerZoneTop - 1;
            DrawLine();
            PrintComponent(player);
            DrawLine();

            CancellationTokenSource animTokenSource = new();
            _ = Task.Run(() => DrawCampfire(animTokenSource.Token));

            ReadKey(true);
            animTokenSource.Cancel();
        }

        private static readonly string[][] campfireAnim =
        [
            [
                @"           (            ,&&&.  ",
                @"            )           .,.&&  ",
                @"       )                \=__/  ",
                @"      (                 ,'-'.  ",
                @"  (        ),,      _.__|/ /|  ",
                @"   )   /\ -((------((_|___/ |  ",
                @"  (   // |  `)      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"            )           ,&&&.  ",
                @"                        .,.&&  ",
                @"   (   )                \=__/  ",
                @"     (                  ,'-'.  ",
                @"  (       ) ,,      _.__|/ /|  ",
                @"    )  /\ -((------((_|___/ |  ",
                @"  (   // |  `)      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"                        ,&&&.  ",
                @"   (                    .,.&&  ",
                @"      )                 \=__/  ",
                @"     (      (           ,'-'.  ",
                @"   (       ),,      _.__|/ /|  ",
                @"    )  /\ -((------((_|___/ |  ",
                @"  (   // |  )'      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"    (                   ,&&&.  ",
                @"                        .,.&&  ",
                @"      )     (           \=__/  ",
                @"      (                 ,'-'.  ",
                @"   (      ) ,,      _.__|/ /|  ",
                @"   )   /\ -((------((_|___/ |  ",
                @"   (  // |  )'      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"                        ,&&&.  ",
                @"            (           .,.&&  ",
                @"       )                \=__/  ",
                @"   )  (                 ,'-'.  ",
                @"  (        ),,      _.__|/ /|  ",
                @"   )   /\ -((------((_|___/ |  ",
                @"   (  // |  `)      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"            (           ,&&&.  ",
                @"                        .,.&&  ",
                @"   )   )                \=__/  ",
                @"     (                  ,'-'.  ",
                @"  (       ) ,,      _.__|/ /|  ",
                @"    )  /\ -((------((_|___/ |  ",
                @"   (  // |  `)      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"                        ,&&&.  ",
                @"   )                    .,.&&  ",
                @"      )                 \=__/  ",
                @"     (                  ,'-'.  ",
                @"   (       ),,      _.__|/ /|  ",
                @"    )  /\ -((------((_|___/ |  ",
                @"  (   // |  )'      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"   )                    ,&&&.  ",
                @"                        .,.&&  ",
                @"      )                 \=__/  ",
                @"      (                 ,'-'.  ",
                @"   (      ) ,,      _.__|/ /|  ",
                @"   )   /\ -((------((_|___/ |  ",
                @"  (   // |  )'      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"                        ,&&&.  ",
                @"                        .,.&&  ",
                @"       )                \=__/  ",
                @"      (                 ,'-'.  ",
                @"  (        ),,      _.__|/ /|  ",
                @"   )   /\ -((------((_|___/ |  ",
                @"  (   // |  `)      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"                        ,&&&.  ",
                @"                        .,.&&  ",
                @"       )                \=__/  ",
                @"     (                  ,'-'.  ",
                @"  (       ) ,,      _.__|/ /|  ",
                @"    )  /\ -((------((_|___/ |  ",
                @"   (  // |  `)      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"                        ,&&&.  ",
                @"                       .,.&&   ",
                @"      )                \=__/   ",
                @"     (   )  ,,          ,'-'.  ",
                @"   (      -((-._    _.__|/ /|  ",
                @"    )  /\   `'   `-((_|___/ |  ",
                @"   (  // |  )       ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"                       ,&&&.   ",
                @"               _,      .,.&&   ",
                @"      )  )    ',─'     \=__/   ",
                @"      (          \      ,'-'.  ",
                @"   (      )       \ _.__|/ /|  ",
                @"   )   /\  (       ((_|___/ |  ",
                @"   (  // |  )       ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"                       ,&&&.   ",
                @"         )     _,      .,.&&   ",
                @"       )      ',─'     \=__/   ",
                @"      (          \      ,'-'.  ",
                @"  (        )      \ _.__|/ /|  ",
                @"   )   /\  (       ((_|___/ |  ",
                @"  (   // |   )      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"         )             ,&&&.   ",
                @"               _,      .,.&&   ",
                @"       )      ',─'     \=__/   ",
                @"     (           \      ,'-'.  ",
                @"  (       )       \ _.__|/ /|  ",
                @"    )  /\   (      ((_|___/ |  ",
                @"  (   // |   )      ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"                       ,&&&.   ",
                @"               _,      .,.&&   ",
                @"      )    (  ',─'     \=__/   ",
                @"     (      )    \      ,'-'.  ",
                @"   (       )      \ _.__|/ /|  ",
                @"    )  /\   (      ((_|___/ |  ",
                @"  (   // |  )       ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
            [
                @"                       ,&&&.   ",
                @"           (            .,.&&  ",
                @"      )     )           \=__/  ",
                @"      (     ,,          ,'-'.  ",
                @"   (      -((-._    _.__|/ /|  ",
                @"   )   /\  ('    `-((_|___/ |  ",
                @"   (  // |  )       ((  `'--|  ",
                @" _ -.;_/ \\--._      \\ \-._/. ",
                @"(_;-// | \ \-'.\    <_,\_\`--'|",
                @"( `.__ _  ___,')      <_,-'__,'",
                @" `'(_ )_)(_)_)'                "
            ],
        ];

        private static async Task DrawCampfire(CancellationToken stopToken)
        {
            int frameInd = 0;
            while (!stopToken.IsCancellationRequested)
            {
                CursorTop = CursorPos.MainZoneTop;
                foreach(var line in campfireAnim[frameInd++])
                    WriteCenter(line);
                frameInd %= campfireAnim.Length;

                await Task.Delay(300, CancellationToken.None);
            }
        }
    }
}