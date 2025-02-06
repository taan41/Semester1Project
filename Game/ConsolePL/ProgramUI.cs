using System.Security.Cryptography;
using BLL.Game.Components;
using BLL.Game.Components.Item;
using BLL.Game.Components.Entity;
using BLL.Game.Components.Others;
using BLL.Server;
using BLL.Config;

using static System.Console;
using static ConsolePL.ConsoleUtilities;
using static ConsolePL.ComponentRenderer;

namespace ConsolePL
{
    public static class ProgramUI
    {
        #region Constants
        public const string GameTitle = "CONSOLE CONQUER";

        private static ServerHandler ServerHandler => ServerHandler.Instance;
        private static DatabaseConfig DbConfig => ConfigManager.Instance.DatabaseConfig;
        private static CancellationTokenSource? titleAnimTokenSource = null;
        #endregion

        #region Misc
        public static void ConsoleSizeNotice()
        {
            Clear();
            DrawLine();

            int noticeTop = 0;
            for (int i = 1; i < CursorPos.BottomBorderTop; i++)
            {
                WriteLine($"| {i}");
                if (i == 18)
                    noticeTop = CursorTop;
            }
            DrawLine();

            CursorLeft = 5;
            CursorTop = noticeTop;
            Write("-- Please resize the console window to fit the game screen");
            CursorLeft = 5;
            CursorTop++;
            Write("-- All lines must be straight and fully visible");
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
        #endregion

        #region Popup
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
                WriteCenter(@"  / \  ");
                WriteCenter(@" / | \ ");
                WriteCenter(@"/  .  \");
                WriteCenter(@"-------");
            }
            else if (popupType == PopupType.Success)
            {
                WriteCenter(@"┌            ┐");
                WriteCenter(@"│        /   │");
                WriteCenter(@"│    \  /    │");
                WriteCenter(@"│     \/     │");
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
        #endregion

        #region Title Animation
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
                "",
                @"      ____ ___  _  __ ___ ___  __    ___    ",
                @"    / ___/ __ \/ |/ / __/ __ \/ /  / __/    ",
                @"   / /__/ /_/ /    /\ \/ /_/ / /__/ _/      ",
                @"   \___/\____/_/|_/___/\____/____/___/__    ",
                @"    / ___/ __ \/ |/ / __ \/ / / / __/ _ \   ",
                @"   / /__/ /_/ /    / /_/ / /_/ / _// , _/   ",
                @"   \___/\____/_/|_/\___\_\____/___/_/|_|    ",
                ""
            ],
            [
                @"  _____ _____ _____ _____ _____ __    _____  ",
                @" |     |     |   | |   __|     |  |  |   __| ",
                @" |   --|  |  | | | |__   |  |  |  |__|   __| ",
                @" |_____|_____|_|___|_____|_____|_____|_____| ",
                @"  _____ _____ _____ _____ _____ _____ _____  ",
                @" |     |     |   | |     |  |  |   __| __  | ",
                @" |   --|  |  | | | |  |  |  |  |   __|    -| ",
                @" |_____|_____|_|___|__  _|_____|_____|__|__| ",
                @"                      |__|                   "
            ],
            [
                "",
                @"   __    ___   _      __   ___   _     ____  ",
                @"  / /`  / / \ | |\ | ( (` / / \ | |   | |_   ",
                @"  \_\_, \_\_/ |_| \| _)_) \_\_/ |_|__ |_|__  ",
                "",
                @"  __    ___   _      ___    _     ____  ___  ",
                @" / /`  / / \ | |\ | / / \  | | | | |_  | |_) ",
                @" \_\_, \_\_/ |_| \| \_\_\\ \_\_/ |_|__ |_| \ ",
                ""
            ]
        ];

        private static async Task TitleAnimation(CancellationToken stopToken)
        {
            int lineIndex = 0, lineLength = background[0].Length;
            int randomTitle = RandomNumberGenerator.GetInt32(titles.Length);

            ConsoleColor[] titleColors = [ ConsoleColor.Red, ConsoleColor.Blue, ConsoleColor.Magenta, ConsoleColor.Yellow ];
            ConsoleColor[] backgroundColors = [ ConsoleColor.Green, ConsoleColor.Cyan ];
            int colorIndex = RandomNumberGenerator.GetInt32(10);

            try
            {
                while(!stopToken.IsCancellationRequested)
                {
                    lock (ConsoleLock)
                    {
                        SetCursorPosition(0, CursorPos.TitleAnimationTop);
                        for (int i = 0; i < background.Length; i++)
                        {
                            ForegroundColor = backgroundColors[colorIndex % backgroundColors.Length];
                            Write($"{background[i][lineIndex..]}{background[i][0..lineIndex]}");
                            ForegroundColor = titleColors[colorIndex % titleColors.Length];
                            WriteCenter(titles[randomTitle][i]);
                            ResetColor();
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
        #endregion

        #region Title Screen
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
        #endregion

        #region Guide Screens

        public static void GuideForNavigatingUI()
        {
            TitleScreenDrawBorders(false, true);

            lock (ConsoleLock)
            {
                CursorTop = CursorPos.TitleScreenMenuTop;
                WriteLine(" -- Navigating the UI:");
                WriteLine(" Use 'W' & 'S' or 'Up' & 'Down' to navigate.");
                WriteLine(" Press 'Enter', 'Space', 'D' or 'Right' to select.");
                WriteLine(" Press 'ESC', 'A' or 'Left' to return.");
                WriteLine(" (Press any key to continue...)");
            }

            ReadKey(true);
        }
        public static void GuideForGameplay()
        {
            TitleScreenDrawBorders(false, true);

            lock (ConsoleLock)
            {
                CursorTop = CursorPos.TitleScreenMenuTop;
                WriteLine(" -- Gameplay:");
                WriteLine(" Beat the game by progressing through all the rooms and floors.");
                WriteLine(" Each room offers various events with the last being the floor's boss.");
                WriteLine(" During battle, you can either attack directly or use skills:");
                WriteLine(" + When attacking, both parties will deal damage to each other.");
                WriteLine(" ++ The damage is based on the attacker's ATK and the defender's DEF.");
                WriteLine(" + When using skills, you won't take damage but will consume MP.");
                WriteLine(" ++ MP will be restored slowly after each direct attack.");
                WriteLine(" (Press any key to continue...)");
            }

            ReadKey(true);
        }

        public static void GuideForOnlineMode()
        {
            TitleScreenDrawBorders(false, true);

            lock (ConsoleLock)
            {
                CursorTop = CursorPos.TitleScreenMenuTop;
                WriteLine(" -- About Online Mode:");
                WriteLine(" Game data will be updated when connected to the server.");
                WriteLine(" You need an account to access other online features.");
                WriteLine(" You can reset your password using registered email.");
                WriteLine(" Online mode allows you to:");
                WriteLine(" + Upload your progress to access from anywhere.");
                WriteLine(" + Compete with other players in the leaderboard.");
                WriteLine(" (Press any key to continue...)");
            }

            ReadKey(true);
        }
        #endregion

        #region Server Connect
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
        #endregion

        #region Account Management
        public static void RegisterScreen()
        {
            string?
                username = null,
                nickname = null,
                password = null,
                confirmPassword = null,
                email = null,
                error;
            int tempLeft, tempTop;

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
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (username != null)
                        WriteLine(username);
                }
                
                if (username == null)
                {
                    username = ReadInput(tempLeft, tempTop, false, DbConfig.UsernameMax);

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
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (password != null)
                        WriteLine(new string('*', password.Length));
                }

                if (password == null)
                {
                    password = ReadInput(tempLeft, tempTop, true, DbConfig.PasswordMax);
                    
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
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (confirmPassword != null)
                        WriteLine(new string('*', confirmPassword.Length));
                }

                if (confirmPassword == null)
                {
                    CursorTop = tempTop;
                    confirmPassword = ReadInput(tempLeft, tempTop, true, DbConfig.PasswordMax);
                    
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
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (nickname != null)
                        WriteLine(nickname);
                }

                if (nickname == null)
                {
                    nickname = ReadInput(tempLeft, tempTop, false, DbConfig.NicknameMax);
                    
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
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (email != null)
                        WriteLine(email);
                }

                if (email == null)
                {
                    email = ReadInput(tempLeft, tempTop, false, DbConfig.EmailMax);
                    
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
            int tempLeft, tempTop;

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
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (username != null)
                        WriteLine(username);
                }

                if (username == null)
                {
                    username = ReadInput(tempLeft, tempTop, false, DbConfig.UsernameMax);

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
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (email != null)
                        WriteLine(email);
                }

                if (email == null)
                {
                    email = ReadInput(tempLeft, tempTop, false, DbConfig.EmailMax);

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
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (password != null)
                        WriteLine(new string('*', password.Length));
                }

                if (password == null)
                {
                    password = ReadInput(tempLeft, tempTop, true, DbConfig.PasswordMax);

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
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (confirmPassword != null)
                        WriteLine(new string('*', confirmPassword.Length));
                }

                if (confirmPassword == null)
                {
                    confirmPassword = ReadInput(tempLeft, tempTop, true, DbConfig.PasswordMax);

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
            int tempLeft, tempTop;

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
                    (tempLeft, tempTop) = GetCursorPosition();
                }

                string? username = ReadInput(tempLeft, tempTop, false, DbConfig.UsernameMax);

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
                    (tempLeft, tempTop) = GetCursorPosition();
                }

                string? password = ReadInput(tempLeft, tempTop, true, DbConfig.PasswordMax);

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

        public static void ViewAccountInfoScreen()
        {
            if (!ServerHandler.IsConnected)
                return;

            TitleScreenDrawBorders(false, true);
            StartTitleAnim();

            lock (ConsoleLock)
            {
                CursorTop = CursorPos.TitleScreenMenuTop;
                WriteLine($" Username: {ServerHandler.Username}");
                WriteLine($" Nickname: {ServerHandler.Nickname}");
                WriteLine($" Email: {ServerHandler.Email}");
                WriteLine();
                WriteLine(" Press any key to return...");
            }

            ReadKey(true);
        }

        public static void ChangePasswordScreen()
        {
            string?
                oldPassword = null,
                newPassword = null,
                confirmPassword = null;
            int tempLeft, tempTop;

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
                    Write(" Old Password: ");
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (oldPassword != null)
                        WriteLine(new string('*', oldPassword.Length));
                }

                if (oldPassword == null)
                {
                    oldPassword = ReadInput(tempLeft, tempTop, true, DbConfig.PasswordMax);

                    if (oldPassword == null)
                        return;

                    if (string.IsNullOrWhiteSpace(oldPassword) || oldPassword.Length < DbConfig.PasswordMin)
                    {
                        Popup($"Must be {DbConfig.PasswordMin} ~ {DbConfig.PasswordMax} characters");
                        oldPassword = null;
                        continue;
                    }

                    if (!ServerHandler.ValidatePassword(oldPassword))
                    {
                        Popup("Incorrect password!");
                        oldPassword = null;
                        continue;
                    }
                }

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop + 2;
                    Write(" New Password: ");
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (newPassword != null)
                        WriteLine(new string('*', newPassword.Length));
                }

                if (newPassword == null)
                {
                    newPassword = ReadInput(tempLeft, tempTop, true, DbConfig.PasswordMax);

                    if (newPassword == null)
                        return;

                    if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < DbConfig.PasswordMin)
                    {
                        Popup($"Must be {DbConfig.PasswordMin} ~ {DbConfig.PasswordMax} characters");
                        newPassword = null;
                        continue;
                    }
                }

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop + 3;
                    Write(" Confirm Password: ");
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (confirmPassword != null)
                        WriteLine(new string('*', confirmPassword.Length));
                }

                if (confirmPassword == null)
                {
                    confirmPassword = ReadInput(tempLeft, tempTop, true, DbConfig.PasswordMax);

                    if (confirmPassword == null)
                        return;

                    if (newPassword != confirmPassword)
                    {
                        Popup("Passwords do not match!");
                        newPassword = null;
                        confirmPassword = null;
                        continue;
                    }
                }

                if (!ServerHandler.UpdateMainUser(null, null, newPassword, out string error))
                {
                    Popup(error);
                    return;
                }
                else
                {
                    Popup("Changed password successfully!", PopupType.Success);
                    return;
                }
            }
        }

        public static void ChangeNicknameScreen()
        {
            string? nickname = null;
            int tempLeft, tempTop;

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
                    Write(" New Nickname: ");
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (nickname != null)
                        WriteLine(nickname);
                }

                if (nickname == null)
                {
                    nickname = ReadInput(tempLeft, tempTop, false, DbConfig.NicknameMax);

                    if (nickname == null)
                        return;

                    if (string.IsNullOrWhiteSpace(nickname) || nickname.Length < DbConfig.NicknameMin)
                    {
                        Popup($"Must be {DbConfig.NicknameMin} ~ {DbConfig.NicknameMax} characters");
                        nickname = null;
                        continue;
                    }

                    if (!ServerHandler.UpdateMainUser(nickname, null, null, out string error))
                    {
                        Popup(error);
                        nickname = null;
                        continue;
                    }
                    else
                    {
                        Popup("Changed nickname successfully!", PopupType.Success);
                        return;
                    }
                }
            }
        }

        public static void ChangeEmailScreen()
        {
            string? email = null;
            int tempLeft, tempTop;

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
                    Write(" New Email: ");
                    (tempLeft, tempTop) = GetCursorPosition();
                    if (email != null)
                        WriteLine(email);
                }

                if (email == null)
                {
                    email = ReadInput(tempLeft, tempTop, false, DbConfig.EmailMax);

                    if (email == null)
                        return;

                    if (string.IsNullOrWhiteSpace(email) || email.Length < DbConfig.EmailMin)
                    {
                        Popup($"Invalid format");
                        email = null;
                        continue;
                    }

                    if (!ServerHandler.UpdateMainUser(null, email, null, out string error))
                    {
                        Popup(error);
                        email = null;
                        continue;
                    }
                    else
                    {
                        Popup("Changed email successfully!", PopupType.Success);
                        return;
                    }
                }
            }
        }

        public static bool DeleteAccountScreen()
        {
            string? password = null;
            int tempLeft, tempTop;

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
                    Write(" Password: ");
                    (tempLeft, tempTop) = GetCursorPosition();
                }

                if (password == null)
                {
                    password = ReadInput(tempLeft, tempTop, true, DbConfig.PasswordMax);

                    if (password == null)
                        return false;

                    if (string.IsNullOrWhiteSpace(password) || password.Length < DbConfig.PasswordMin)
                    {
                        Popup($"Must be {DbConfig.PasswordMin} ~ {DbConfig.PasswordMax} characters");
                        password = null;
                        continue;
                    }

                    if (!ServerHandler.ValidatePassword(password))
                    {
                        Popup("Incorrect password!");
                        password = null;
                        continue;
                    }
                }

                lock (ConsoleLock)
                {
                    CursorTop = CursorPos.TitleScreenMenuTop + 3;
                    WriteLine(" This action is irreversible!");
                    Write(" Type 'DELETE' to confirm: ");
                    (tempLeft, tempTop) = GetCursorPosition();
                }

                string? confirm = ReadInput(tempLeft, tempTop, false, 6);

                if (confirm == null)
                    return false;

                if (confirm != "DELETE")
                {
                    Popup("Invalid confirmation!");
                    continue;
                }

                if (!ServerHandler.DeleteAccount(out string error))
                {
                    Popup(error);
                    return false;
                }
                else
                {
                    Popup("Account deleted successfully!", PopupType.Success);
                    return true;
                }
            }
        }
        #endregion

        #region Game UI Helpers

        public static void PrintMainZone(List<string> options, string? msg = null, int optionCursorLeft = 1)
        {
            CursorTop = CursorPos.MainZoneTop;
            for (int i = 0; i < UIConstants.MainZoneHeight; i++)
                DrawEmptyLine();

            CursorTop = CursorPos.MainZoneTop;
            if (msg != null)
                WriteLine($" -- {msg}");
            options.Take(UIConstants.MainZoneHeight - (msg != null ? 1 : 0)).ToList().ForEach(option =>
            {
                CursorLeft = optionCursorLeft;
                WriteLine(option);
            });
        }

        public static void PrintMainZone<T>(List<T> components, string? msg = null) where T : GameComponent
        {
            CursorTop = CursorPos.MainZoneTop;
            for (int i = 0; i < UIConstants.MainZoneHeight; i++)
                DrawEmptyLine();

            CursorTop = CursorPos.MainZoneTop;
            if (msg != null)
                WriteLine($" -- {msg}");
            components.Take(UIConstants.MainZoneHeight - (msg != null ? 1 : 0)).ToList().ForEach(Render);
        }

        public static void PrintSubZone(List<string> options, string? msg = null, int optionCursorLeft = 1)
        {
            CursorTop = CursorPos.SubZoneTop;
            for (int i = 0; i < UIConstants.SubZoneHeight; i++)
                DrawEmptyLine();

            CursorTop = CursorPos.SubZoneTop;
            if (msg != null)
                WriteLine($" -- {msg}");
            options.Take(UIConstants.SubZoneHeight - (msg != null ? 1 : 0)).ToList().ForEach(option =>
            {
                CursorLeft = optionCursorLeft;
                WriteLine(option);
            });
        }

        public static void PrintSubZone<T>(List<T> components, string? msg = null) where T : GameComponent
        {
            CursorTop = CursorPos.SubZoneTop;
            for (int i = 0; i < UIConstants.SubZoneHeight; i++)
                DrawEmptyLine();
                
            CursorTop = CursorPos.SubZoneTop;
            if (msg != null)
                WriteLine($" -- {msg}");
            components.Take(UIConstants.SubZoneHeight - (msg != null ? 1 : 0)).ToList().ForEach(Render);
        }
        #endregion

        #region Game UI
        public static string? EnterSeed()
        {
            int tempTop;

            lock (ConsoleLock)
            {
                SetCursorPosition(CursorPos.TitleScreenMenuLeft, CursorPos.TitleScreenMenuTop + 4);
                WriteLine(" ENTER SEED: ");
                tempTop = CursorTop;
            }

            return ReadInput(15, tempTop, false, 40);
        }

        public static void ViewScoresScreen(List<string> scores, string listName)
        {
            StopTitleAnim();
            Clear();
            DrawHeader();
            CursorTop++;

            WriteLine($" -- {listName}:");
            WriteLine("     Nickname                  - Run Duration - Uploaded Time");
            for (int i = 1; i < 16; i++)
            {
                if (i <= scores.Count)
                    WriteLine($" {i,2}. {scores[i - 1]}");
                else
                    WriteLine($" {i,2}.");
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
            RenderComponent(player);
            DrawLine();
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

        public static void ShopTradingScreen<T>(RunProgress progress, Player player, List<T> items, bool buying) where T : GameItem
        {
            Clear();
            DrawHeader();
            progress.Print();
            DrawLine();
            WriteLine($" -- {(buying ? "Buying:" : "Selling:")}");
            items.ForEach(item => RenderItemPrice(item, buying));
            CursorTop = CursorPos.PlayerZoneTop - 1;
            DrawLine();
            RenderComponent(player);
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
                WriteLine(option);
                DrawEmptyLine();
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
            RenderComponent(player);
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
        #endregion
    }
}