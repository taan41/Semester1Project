using System.Security.Cryptography;
using static System.Console;
using static UIHelper;
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
                SetCursorPosition(0, CursorPos.MainTitleTop);
                for (int i = 0; i < background.Length; i++)
                {
                    Write($"{background[i][lineIndex..]}{background[i][0..lineIndex]}");
                    UIMisc.WriteCenter(titles[randomTitle][i]);
                }
                lineIndex += 1;
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

    public static void WarningPopup(string warning)
    {
        CursorTop = CursorPos.WarningTop;
        UIMisc.WriteCenter($"╔{new string('═', UIConstants.WarningWidth)}╗");
        for (int i = 0; i < UIConstants.WarningHeight - 2; i++)
            UIMisc.WriteCenter($"║{new string(' ', UIConstants.WarningWidth)}║");
        UIMisc.WriteCenter($"╚{new string('═', UIConstants.WarningWidth)}╝");

        CursorTop = CursorPos.WarningTop + 2;
        UIMisc.WriteCenter(@".");
        UIMisc.WriteCenter(@"/ \");
        UIMisc.WriteCenter(@"/ ┃ \");
        UIMisc.WriteCenter(@"/  •  \");
        UIMisc.WriteCenter(@"‾‾‾‾‾‾‾");

        CursorTop++;
        UIMisc.WriteCenter(warning);
    }

    public static void WelcomeScreen(List<string> options)
    {
        Clear();
        UIMisc.DrawLine('=');
        CursorTop += 9;
        UIMisc.DrawLine('=');
        int optionsCursorTop = CursorTop;
        CursorTop += 6;
        UIMisc.DrawLine('-');

        CursorTop = optionsCursorTop;
        WriteLine();
        foreach (var option in options)
        {
            CursorLeft = CursorPos.MainMenuLeft;
            WriteLine($" {option}        ");
        }
    }

    public static int? PlayOnlineScreen()
    {
        WriteLine("WIP");
        return null;
    }

    public static void StartScreen(List<string> options, bool clear = true)
    {
        if (clear) Clear();
        else SetCursorPosition(0, 0);
        UIMisc.DrawLine('=');
        CursorTop += 9;
        UIMisc.DrawLine('=');
        int optionsCursorTop = CursorTop;
        CursorTop += 6;
        UIMisc.DrawLine('-');

        CursorTop = optionsCursorTop;
        WriteLine();
        foreach (var option in options)
        {
            CursorLeft = CursorPos.MainMenuLeft;
            WriteLine($" {option}                 ");
        }
    }

    public static void PausePopup(List<string> pauseOptions, TimeSpan? elapsedTime = null)
    {
        CursorTop = CursorPos.PauseBorderTop;
        UIMisc.WriteCenter($"╔{new string('═', UIConstants.PauseWidth)}╗");
        for (int i = 0; i < UIConstants.PauseHeight - 2; i++)
            UIMisc.WriteCenter($"║{new string(' ', UIConstants.PauseWidth)}║");
        UIMisc.WriteCenter($"╚{new string('═', UIConstants.PauseWidth)}╝");

        CursorTop = CursorPos.PauseOptionTop;
        foreach(var option in pauseOptions)
        {
            CursorLeft = CursorPos.PauseOptionLeft;
            WriteLine(option);
        }

        if (elapsedTime != null)
        {
            CursorTop = CursorPos.PauseTimeTop;
            UIMisc.WriteCenter("Run Time:");
            UIMisc.WriteCenter($"{elapsedTime:hh\\:mm\\:ss\\.fff}");
        }
    }

    public static void PrintComponents<T>(List<T> components, int zoneHeight, string? msg = null, int? cursorTop = null) where T: Component
    {
        if (cursorTop != null)
            CursorTop = (int) cursorTop;

        if (msg != null)
            WriteLine(msg);
        
        components.ForEach(component => component.Print());

        if (zoneHeight > components.Count + (msg == null ? 0 : 1))
            Write(new string('\n', zoneHeight - components.Count - (msg == null ? 0 : 1)));
    }

    public static void PrintOptions(List<string> options, int zoneHeight, string? msg = null, int? cursorTop = null)
    {
        if (cursorTop != null)
            CursorTop = (int) cursorTop;

        if (msg != null)
            WriteLine(msg);
        
        options.ForEach(option => WriteLine($" {option}"));

        if (zoneHeight > options.Count + (msg == null ? 0 : 1))
            Write(new string('\n', zoneHeight - options.Count - (msg == null ? 0 : 1)));
    }

    public static void DrawHeader(bool setCursor = false)
    {
        if (setCursor)
            SetCursorPosition(0, 0);

        UIMisc.DrawLine('=');
        ForegroundColor = ConsoleColor.Cyan;
        UIMisc.WriteCenter(GameTitle);
        ResetColor();
        UIMisc.DrawLine('=');
    }

    public static void GenericGameScreen(GameData gameData)
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        Write(new string('\n', UIConstants.MainZoneHeight));
        UIMisc.DrawLine('-');
        Write(new string('\n', UIConstants.SubZoneHeight));
        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');
    }

    public static void RouteScreen(GameData gameData, List<Event> routes, List<string> invOptions)
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        PrintComponents(routes, UIConstants.MainZoneHeight, " -- Pick a Route:");
        UIMisc.DrawLine('-');
        PrintOptions(invOptions, UIConstants.SubZoneHeight, " -- Inventory:");
        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');
    }

    public static void InventoryScreen<T>(GameData gameData, List<T> inv, List<T> equipped) where T : Item
    {
        Clear();
        DrawHeader(false);
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        PrintComponents(inv, UIConstants.MainZoneHeight, " -- Inventory:");
        UIMisc.DrawLine('-');
        PrintComponents(equipped, UIConstants.SubZoneHeight, " -- Currently Equipped:");
        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');
    }

    public static void FightScreen(GameData gameData, List<Monster> monsters, List<string> actions)
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        PrintComponents(monsters, UIConstants.MainZoneHeight);
        UIMisc.DrawLine('-');
        PrintOptions(actions, UIConstants.SubZoneHeight, " -- Actions:");
        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');
    }

    public static void FightSkillScreen(GameData gameData, List<Monster> monsters, List<Skill> skills)
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        PrintComponents(monsters, UIConstants.MainZoneHeight);
        UIMisc.DrawLine('-');
        PrintComponents(skills, UIConstants.SubZoneHeight);
        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');
    }

    public static void RewardScreen(GameData gameData, List<Item> rewards)
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        PrintComponents(rewards, UIConstants.MainZoneHeight, " -- Rewards:");
        UIMisc.DrawLine('-');
        Write(new string('\n', UIConstants.SubZoneHeight));
        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');
    }

    public static void GameOverScreen(List<string> options)
    {
        Clear();
        DrawHeader();
        WriteLine();
        UIMisc.WriteCenter(@" _______ _______ _______ _______      _______ ___ ___ _______ ______ ");
        UIMisc.WriteCenter(@"|     __|   _   |   |   |    ___|    |       |   |   |    ___|   __ \");
        UIMisc.WriteCenter(@"|    |  |       |       |    ___|    |   -   |   |   |    ___|      <");
        UIMisc.WriteCenter(@"|_______|___|___|__|_|__|_______|    |_______|\_____/|_______|___|__|");
        WriteLine();
        WriteLine();
        UIMisc.DrawLine('-');
        CursorTop += 7;
        UIMisc.DrawLine('-');

        CursorTop = CursorPos.EndMenuTop;
        foreach (var option in options)
        {
            CursorLeft = CursorPos.MainMenuLeft;
            WriteLine($" {option}        ");
        }
    }

    public static void VictoryScreen(TimeSpan elapsedTime, List<string> options)
    {
        Clear();
        DrawHeader();
        WriteLine();
        UIMisc.WriteCenter(@" ___ ___ _______ ______ _______ _______ ______ ___ ___ __ ");
        UIMisc.WriteCenter(@"|   |   |_     _|      |_     _|       |   __ \   |   |  |");
        UIMisc.WriteCenter(@"|   |   |_|   |_|   ---| |   | |   -   |      <\     /|__|");
        UIMisc.WriteCenter(@" \_____/|_______|______| |___| |_______|___|__| |___| |__|");
        WriteLine();
        WriteLine();
        UIMisc.DrawLine('-');
        CursorTop += 7;
        UIMisc.DrawLine('-');

        CursorTop = CursorPos.EndMenuTop;
        UIMisc.WriteCenter($"Run duration: {elapsedTime:hh\\:mm\\:ss\\.fff}");
        WriteLine();
        foreach (var option in options)
        {
            CursorLeft = CursorPos.MainMenuLeft;
            WriteLine($" {option}        ");
        }
    }

    public static void ShopMainScreen(GameData gameData, List<string> actions)
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        UIMisc.WriteCenter(@"   _____________________________                            ");
        UIMisc.WriteCenter(@"  /    _____ _____ _____ _____  \            ┌─────────┐    ");
        UIMisc.WriteCenter(@" /   |   __|  |  |     |  _  |   \           │   ♦$♦   │    ");
        UIMisc.WriteCenter(@"|    |__   |     |  |  |   __|    |      ════╪═════════╪════");
        UIMisc.WriteCenter(@" \   |_____|__|__|_____|__|      /           │ █     █ │    ");
        UIMisc.WriteCenter(@"  \_____________________________/            ;  = ┴ =  ;    ");
        UIMisc.DrawLine('-');
        PrintOptions(actions, UIConstants.SubZoneHeight);
        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');
    }

    public static void ShopTradingScreen<T>(GameData gameData, List<T> items, bool buying) where T : Item
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        WriteLine($" -- {(buying ? "Buying:" : "Selling:")}");
        items.ForEach(item => item.PrintPrice(buying));
        CursorTop = CursorPos.PlayerZoneTop - 1;
        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');
    }

    public static void CampfireScreen(GameData gameData)
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        CursorTop += 11;
        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');

        CancellationTokenSource animTokenSource = new();
        _ = Task.Run(() => DrawCampfire(animTokenSource.Token));

        ReadKey(true);
        animTokenSource.Cancel();
    }

    public static void TreasureOpening(GameData gameData)
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        WriteLine();
        UIMisc.WriteCenter(@"       _ _                                  ");
        UIMisc.WriteCenter(@"    .' '.: '*:=. _                          ");
        UIMisc.WriteCenter(@"  .' .'            '* :=. _                 ");
        UIMisc.WriteCenter(@" /  /                      . *: -__         ");
        UIMisc.WriteCenter(@":  :                     .'  .:     ' .     ");
        UIMisc.WriteCenter(@":  '=._                 /  .'          \    ");
        UIMisc.WriteCenter(@":._     '*:=._         '  /             :   ");
        UIMisc.WriteCenter(@":  +'* =._     '* =._ :  :           _(#)   ");
        UIMisc.WriteCenter(@":O +     : '*:=._     '  ::     _ .=*+  :   ");
        UIMisc.WriteCenter(@":  +     | '@,-:   '* : -..=:*'      + O:   ");
        UIMisc.WriteCenter(@":O +      ' # ,|      +''::' +       +  :   ");
        UIMisc.WriteCenter(@":  +-._               + O::O +       + O:   ");
        UIMisc.WriteCenter(@" *=:._  '+-._         +  ::  +       +  :   ");
        UIMisc.WriteCenter(@"     ''*=:._  '+-._   + O::O +   _.=*'  :   ");
        UIMisc.WriteCenter(@"           ''*=:._  ''+  ::  +*' _.;=*'     ");
        UIMisc.DrawLine('-');
        ReadKey(true);
        
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        UIMisc.WriteCenter(@"  .* .'_       '*::=._                      ");
        UIMisc.WriteCenter(@" /. _    '*:=-._      ''*::=._              ");
        UIMisc.WriteCenter(@"' * _ '*=-._     '*:=._     :'*::=.__       ");
        UIMisc.WriteCenter(@"     ' =_    '*=-._     '-:'  .*      `.    ");
        UIMisc.WriteCenter(@"          *=_       '*=-._  /            \  ");
        UIMisc.WriteCenter(@"           _(#)_           ' ._           . ");
        UIMisc.WriteCenter(@"     _-+*'₀ ₁ ₀₀ '*=. _         *=_       : ");
        UIMisc.WriteCenter(@".+*' ₁₀ ₁ ₁ ₀₀₁  ₀   ₁ ₁'*=-._      *=_  /  ");
        UIMisc.WriteCenter(@": *+= _₁ ₁  ₀₁₁  ₁ ₀ ₁  ₀₁₁ ₁  ₀'*=-._(#)   ");
        UIMisc.WriteCenter(@":O +    '*+= _ ₁₁ ₀  ₀₁₀ ₁   ₀₀ ₁  _.+' :   ");
        UIMisc.WriteCenter(@":  +     | '@, '*+= _₀₁ ₀ ₀ ₁_.+:'   + O:   ");
        UIMisc.WriteCenter(@":O +      ' # ,|      +''::' +       +  :   ");
        UIMisc.WriteCenter(@":  +-._               + O::O +       + O:   ");
        UIMisc.WriteCenter(@" *=:._  '+-._         +  ::  +       +  :   ");
        UIMisc.WriteCenter(@"     ''*=:._  '+-._   + O::O +   _.=*'  :   ");
        UIMisc.WriteCenter(@"           ''*=:._  ''+  ::  +*' _.;=*'     ");
        UIMisc.DrawLine('-');
        ReadKey(true);
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
                UIMisc.WriteCenter(line);
            frameInd %= campfireAnim.Length;

            await Task.Delay(300, CancellationToken.None);
        }
    }
}