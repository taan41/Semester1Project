using System.Security.Cryptography;

using static System.Console;
using static GameUIHelper;
using static Utilities;

static class GameUI
{
    public const string GameTitle = "CONSOLE CONQUER";

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
        DrawLine('-');
        for (int i = 1; i < CursorPos.BottomBorderTop; i++)
            WriteLine($"| {i}");
        DrawLine('-');

        CursorLeft = 5;
        CursorTop -= 3;
        Write("-- Please resize the console window to fit the game screen");
        CursorLeft = 5;
        CursorTop++;
        Write("-- Press any key to continue...");
        ReadKey(true);
    }

    public static void WarningPopup(string warning)
    {
        StopTitleAnim();
        CursorTop = CursorPos.PopupTop;
        WriteCenter($"╔{new string('═', UIConstants.PopupWidth)}╗");
        for (int i = 0; i < UIConstants.PopupHeight - 2; i++)
            WriteCenter($"║{new string(' ', UIConstants.PopupWidth)}║");
        WriteCenter($"╚{new string('═', UIConstants.PopupWidth)}╝");

        CursorTop = CursorPos.PopupTop + 2;
        WriteCenter(@"   .   ");
        WriteCenter(@"  ╱ ╲  ");
        WriteCenter(@" ╱ ┃ ╲ ");
        WriteCenter(@"╱  •  ╲");
        WriteCenter(@"‾‾‾‾‾‾‾");

        CursorTop++;
        WriteCenter(warning);
        ReadKey(true);
    }

    public static void SuccessPopup(string msg)
    {
        StopTitleAnim();
        CursorTop = CursorPos.PopupTop;
        WriteCenter($"╔{new string('═', UIConstants.PopupWidth)}╗");
        for (int i = 0; i < UIConstants.PopupHeight - 2; i++)
            WriteCenter($"║{new string(' ', UIConstants.PopupWidth)}║");
        WriteCenter($"╚{new string('═', UIConstants.PopupWidth)}╝");

        CursorTop = CursorPos.PopupTop + 2;
        WriteCenter(@"┌            ┐");
        WriteCenter(@"│        ╱   │");
        WriteCenter(@"│    ╲  ╱    │");
        WriteCenter(@"│     ╲╱     │");
        WriteCenter(@"└            ┘");

        CursorTop++;
        WriteCenter(msg);
        ReadKey(true);
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

    public static void TitleScreenBorders(bool clear = true, bool clearOptionsZone = false)
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

            DrawLine('-');
        }
    }

    public static void TitleScreenOptions(List<string> options)
    {
        CursorTop = CursorPos.TitleScreenMenuTop;
        foreach (var option in options)
        {
            CursorLeft = CursorPos.TitleScreenMenuLeft;
            WriteLine($" {option,-(UIConstants.UIWidth - CursorPos.TitleScreenMenuLeft - 1)}");
        }
    }

    public static void ConnectingScreen(ref NetworkHandler? networkHandler)
    {
        TitleScreenBorders(false, true);
        int connectResultCursorTop;
        lock (ConsoleLock)
        {
            SetCursorPosition(CursorPos.TitleScreenMenuLeft2, CursorPos.TitleScreenMenuTop);
            WriteLine(" Connecting to server...");
            connectResultCursorTop = CursorTop;
        }

        networkHandler = new(out string? error);
        int updateDataCursorTop;
        if (error != null)
        {
            networkHandler = null;
            lock (ConsoleLock)
            {
                SetCursorPosition(CursorPos.TitleScreenMenuLeft2, connectResultCursorTop);
                WriteLine($" Error: {error}");
                CursorLeft = CursorPos.TitleScreenMenuLeft2;
                CursorTop++;
                WriteLine(" Press any key to continue...");
            }
            ReadKey(true);
            TitleScreenBorders(false, true);
            return;
        }
        else
        {
            lock (ConsoleLock)
            {
                SetCursorPosition(CursorPos.TitleScreenMenuLeft2, connectResultCursorTop);
                WriteLine(" Connected successfully!");
                updateDataCursorTop = CursorTop + 1;
            }
        }

        lock (ConsoleLock)
        {
            SetCursorPosition(CursorPos.TitleScreenMenuLeft2, updateDataCursorTop);
            WriteLine(" Updating game data...");
            CursorLeft = CursorPos.TitleScreenMenuLeft2;
            WriteLine(" Updated successfully!");
            CursorLeft = CursorPos.TitleScreenMenuLeft2;
            CursorTop++;
            WriteLine(" Press any key to continue...");
        }

        ReadKey(true);
        TitleScreenBorders(false, true);
    }

    public static void RegisterScreen(NetworkHandler networkHandler)
    {
        if (networkHandler == null || !networkHandler.IsConnected)
        {
            WarningPopup("Can't connect to server!");
            goto return_label;
        }

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
            if (!networkHandler.IsConnected)
                goto return_label;

            TitleScreenBorders(false, true);
            StartTitleAnim();

            lock (ConsoleLock)
            {
                CursorTop = CursorPos.TitleScreenMenuTop;
                WriteLine(" 'ESC' to return");
                Write($" Username ({DataConstants.usernameMin} ~ {DataConstants.usernameMax} characters): ");
                (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                if (username != null)
                    WriteLine(username);
            }

            if (username == null)
            {
                username = ReadInput(tempCursorLeft, tempCursorTop, false, DataConstants.usernameMax);
                
                if (username == null)
                    goto return_label;
                else if (string.IsNullOrWhiteSpace(username) || username.Length < DataConstants.usernameMin)
                {
                    username = null;
                    continue;
                }

                if (!networkHandler.CheckUsername(username, out errorMsg))
                {
                    username = null;
                    WarningPopup(errorMsg);
                    continue;
                }
            }

            lock (ConsoleLock)
            {
                CursorTop = CursorPos.TitleScreenMenuTop + 2;
                Write($" Password ({DataConstants.passwordMin} ~ {DataConstants.passwordMax} characters): ");
                (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                if (password != null)
                    WriteLine(new string('*', password.Length));
            }

            if (password == null)
            {
                password = ReadInput(tempCursorLeft, tempCursorTop, true, DataConstants.passwordMax);
                
                if (password == null)
                    goto return_label;
                else if (string.IsNullOrWhiteSpace(password) || password.Length < DataConstants.passwordMin)
                {
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
                confirmPassword = ReadInput(tempCursorLeft, tempCursorTop, true, DataConstants.passwordMax);
                
                if (confirmPassword == null)
                    goto return_label;
                else if (string.IsNullOrWhiteSpace(confirmPassword) || confirmPassword.Length < DataConstants.passwordMin)
                {
                    confirmPassword = null;
                    continue;
                }
                else if (password != confirmPassword)
                {
                    password = null;
                    confirmPassword = null;
                    WarningPopup("Passwords do not match!");
                    continue;
                }
            }

            lock (ConsoleLock)
            {
                CursorTop = CursorPos.TitleScreenMenuTop + 4;
                Write($" Nickname ({DataConstants.nicknameMin} ~ {DataConstants.nicknameMax} characters): ");
                (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                if (nickname != null)
                    WriteLine(nickname);
            }

            if (nickname == null)
            {
                nickname = ReadInput(tempCursorLeft, tempCursorTop, false, DataConstants.nicknameMax);
                
                if (nickname == null)
                    goto return_label;
                else if (string.IsNullOrWhiteSpace(nickname) || nickname.Length < DataConstants.nicknameMin)
                {
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
                email = ReadInput(tempCursorLeft, tempCursorTop, false, DataConstants.emailLen);
                
                if (email == null)
                    goto return_label;
                else if (string.IsNullOrWhiteSpace(email))
                {
                    email = null;
                    continue;
                }
            }

            User newUser = new(username, nickname, password, email);
            if (!networkHandler.Register(newUser, out errorMsg))
                WarningPopup(errorMsg);
            else
                SuccessPopup("Registration Successful!");
            
            break;
        }

        return_label:
            TitleScreenBorders(false, true);
            StartTitleAnim();
            return;
    }

    public static void LoginScreen(NetworkHandler networkHandler, out User? loggedInUser)
    {
        loggedInUser = null;

        if (networkHandler == null || !networkHandler.IsConnected)
        {
            WarningPopup("Can't connect to server!");
            goto return_label;
        }

        PasswordSet? passwordSet;
        int tempCursorLeft, tempCursorTop;

        while (true)
        {
            if (!networkHandler.IsConnected)
                goto return_label;

            TitleScreenBorders(false, true);
            StartTitleAnim();
            
            lock (ConsoleLock)
            {
                CursorTop = CursorPos.TitleScreenMenuTop;
                WriteLine(" 'ESC' to return");
                Write(" Username: ");
                (tempCursorLeft, tempCursorTop) = GetCursorPosition();
            }

            string? username = ReadInput(tempCursorLeft, tempCursorTop, false, DataConstants.usernameMax);
            if (username == null)
                goto return_label;
            else if (string.IsNullOrWhiteSpace(username))
                continue;

            passwordSet = networkHandler.GetPassword(username, out string? errorMsg);
            if (passwordSet == null)
            {
                WarningPopup(errorMsg);
                continue;
            }

            lock (ConsoleLock)
            {
                CursorTop = CursorPos.TitleScreenMenuTop + 2;
                Write(" Password: ");
                (tempCursorLeft, tempCursorTop) = GetCursorPosition();
            }

            string? password = ReadInput(tempCursorLeft, tempCursorTop, true, DataConstants.passwordMax);
            if (password == null)
                goto return_label;
            else if (string.IsNullOrWhiteSpace(password))
                continue;

            if (!Security.VerifyPassword(password, passwordSet))
            {
                WarningPopup("Incorrect password!");
                continue;
            }

            loggedInUser = networkHandler.Login(out errorMsg);

            if (loggedInUser == null)
                WarningPopup(errorMsg);
            else
                SuccessPopup("Login Successful!");

            break;
        }

        return_label:
            TitleScreenBorders(false, true);
            StartTitleAnim();
            return;
    }

    public static void ResetPasswordScreen(NetworkHandler networkHandler)
    {
        if (networkHandler == null || !networkHandler.IsConnected)
        {
            WarningPopup("Can't connect to server!");
            return;
        }

        string?
            username = null,
            email = null,
            password = null,
            confirmPassword = null,
            errorMsg;
        int tempCursorLeft, tempCursorTop;
        bool emailCheck = false;

        while (true)
        {
            if (!networkHandler.IsConnected)
                return;

            TitleScreenBorders(false, true);
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
                username = ReadInput(tempCursorLeft, tempCursorTop, false, DataConstants.usernameMax);
                
                if (username == null)
                    return;
                else if (string.IsNullOrWhiteSpace(username))
                {
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
                email = ReadInput(tempCursorLeft, tempCursorTop, false, DataConstants.emailLen);
                
                if (email == null)
                    return;
                else if (string.IsNullOrWhiteSpace(email))
                {
                    email = null;
                    continue;
                }
            }

            User userToReset = new() { Username = username, Email = email };
            if (!emailCheck)
            {
                if (!networkHandler.ValidateEmail(userToReset, out errorMsg))
                {
                    username = null;
                    email = null;
                    WarningPopup(errorMsg);
                }
                else
                {
                    SuccessPopup("Valid user info");
                    emailCheck = true;
                }

                continue;
            }

            lock (ConsoleLock)
            {
                CursorTop = CursorPos.TitleScreenMenuTop + 3;
                Write($" Password ({DataConstants.passwordMin} ~ {DataConstants.passwordMax} characters): ");
                (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                if (password != null)
                    WriteLine(new string('*', password.Length));
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
                CursorTop = CursorPos.TitleScreenMenuTop + 4;
                Write(" Confirm Password: ");
                (tempCursorLeft, tempCursorTop) = GetCursorPosition();
                if (confirmPassword != null)
                    WriteLine(new string('*', confirmPassword.Length));
            }

            if (confirmPassword == null)
            {
                CursorTop = tempCursorTop;
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
                    password = null;
                    confirmPassword = null;
                    WarningPopup("Passwords do not match!");
                    continue;
                }
            }

            userToReset.PwdSet = new(password);

            if (!networkHandler.ResetPwd(userToReset, out errorMsg))
                WarningPopup(errorMsg);
            else
                SuccessPopup("Password reset successful!");

            break;
        }

        TitleScreenBorders(false, true);
        StartTitleAnim();
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

    public static void ViewScoresScreen(List<Score> scores, string listName)
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
        DrawLine('-');
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

    public static void GenericGameScreen(GameData gameData)
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        DrawLine('-');

        CursorTop += UIConstants.MainZoneHeight;
        DrawLine('-');

        CursorTop += UIConstants.SubZoneHeight;
        DrawLine('-');

        gameData.Player.Print();
        DrawLine('-');
    }

    public static void PrintMainZone(List<string> options, string? msg = null)
    {
        CursorTop = CursorPos.MainZoneTop;
        if (msg != null)
            WriteLine($" -- {msg}");
        options.ForEach(option => WriteLine($" {option}"));
    }

    public static void PrintMainZone<T>(List<T> components, string? msg = null) where T : Component
    {
        CursorTop = CursorPos.MainZoneTop;
        if (msg != null)
            WriteLine($" -- {msg}");
        components.ForEach(components => components.Print());
    }

    public static void PrintSubZone(List<string> options, string? msg = null)
    {
        CursorTop = CursorPos.SubZoneTop;
        if (msg != null)
            WriteLine($" -- {msg}");
        options.ForEach(option => WriteLine($" {option}"));
    }

    public static void PrintSubZone<T>(List<T> components, string? msg = null) where T : Component
    {
        CursorTop = CursorPos.SubZoneTop;
        if (msg != null)
            WriteLine($" -- {msg}");
        components.ForEach(components => components.Print());
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

    public static void ShopTradingScreen<T>(GameData gameData, List<T> items, bool buying) where T : Item
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        DrawLine('-');
        WriteLine($" -- {(buying ? "Buying:" : "Selling:")}");
        items.ForEach(item => item.PrintPrice(buying));
        CursorTop = CursorPos.PlayerZoneTop - 1;
        DrawLine('-');
        gameData.Player.Print();
        DrawLine('-');
    }

    public static void TreasureOpening(GameData gameData)
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        DrawLine('-');
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
        DrawLine('-');
        ReadKey(true);
        
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        DrawLine('-');
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
        DrawLine('-');
        ReadKey(true);
    }

    public static void GameOverScreen(List<string> options)
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
        DrawLine('-');
        CursorTop += 7;
        DrawLine('-');

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
        DrawLine('-');
        CursorTop += 7;
        DrawLine('-');

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

    public static void CampfireScreen(GameData gameData)
    {
        GenericGameScreen(gameData);
        CursorTop = CursorPos.SubZoneTop;
        DrawLine(' ');

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