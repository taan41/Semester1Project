using static System.Console;
using static UIHelper;
using static Utilities;

static class GameUI
{
    public const string GameTitle = "CONSOLE CONQUER";

    private static CancellationTokenSource? titleAnimTokenSource = null;

    private static readonly string[] background0 = [
        "₁     ₀₀  ₁ ₁     ₀  ₁ ₁₀     ₀₁  ₁ ₀₀₁  ₁   ₁₀  ₀₁₁     ₀    ₀₁    ₁₀",
        "₀₁₀₀   ₁  ₀₁   ₀₁    ₀₁₁  ₀₁       ₀₁₁   ₁      ₀₀₁   ₁₀₁₁   ₀₁ ₁  ₀₀ ",
        "  ₁ ₁ ₁₀    ₁₀₀  ₁    ₁₀      ₀₁   ₀₁  ₁ ₁    ₁    ₁₀       ₁₀   ₁ ₁₁ ",
        "₀₁   ₁₀  ₁₁₀  ₀     ₀₀   ₀₁₁  ₀      ₀  ₁  ₁   ₀₀ ₁  ₁   ₀ ₀  ₀₀   ₁₁₀",
        "₁     ₀₀    ₁   ₁₀  ₀₁₁    ₀₁ ₁     ₀  ₁ ₁₀     ₀₁  ₁ ₀ ₀₁    ₀₁    ₁₀",
        "₀   ₀ ₁  ₁₀₁  ₁₀   ₁     ₀₁₀   ₀₀₁₁  ₀₁   ₁₀₁  ₀ ₀₀   ₁₁  ₀₁  ₀₀₁   ₀₁",
        "₀₀   ₁  ₁    ₀₁     ₀₁₀  ₀  ₀₁   ₁  ₀  ₁ ₁₀₁     ₀₀  ₁   ₁₀  ₀₀ ₁   ₀₁",
        "₀₁₁    ₀₁₁₀₀   ₁  ₀₁   ₀  ₀₁    ₁   ₁₀₁₁     ₀₁₁   ₁      ₀₀ ₀₁ ₁   ₀₀",
        "₀  ₀     ₁   ₁₀₁  ₁₀    ₁ ₀₁  ₀   ₀ ₁₁   ₀₁ ₁₀₁₀ ₀₀₁   ₁₀₁  ₀ ₀  ₁  ₀₁"
    ];

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

    private static readonly string[] title = [
        "",
        "    ╔═╗ ╔═╗ ╦╗╦ ╔═╗ ╔═╗ ╦   ╔═╗    ",
        "    ║   ║ ║ ║╚╣ ╚═╗ ║ ║ ║   ╠╣     ",
        "    ╚═╝ ╚═╝ ╩ ╩ ╚═╝ ╚═╝ ╩═╝ ╚═╝    ",
        "",
        "    ╔═╗ ╔═╗ ╦╗╦ ╔═╗ ╦ ╦ ╔═╗ ╦═╗    ",
        "    ║   ║ ║ ║╚╣ ║╔╣ ║ ║ ╠╣  ╠╦╝    ",
        "    ╚═╝ ╚═╝ ╩ ╩ ╚╩╩ ╚═╝ ╩═╝ ╩╚═    ",
        ""
    ];

    private static async Task TitleAnimation(CancellationToken stopToken)
    {
        int lineIndex = 0, lineLength = background[0].Length;

        try
        {
            while(!stopToken.IsCancellationRequested)
            {
                SetCursorPosition(0, CursorPos.MainTitleTop);
                for (int i = 0; i < background.Length; i++)
                {
                    Write($"{background[i][lineIndex..]}{background[i][0..lineIndex]}");
                    UIMisc.WriteCenter(title[i]);
                }
                lineIndex += 1;
                lineIndex %= lineLength;
                
                await Task.Delay(300, stopToken);
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

    public static void WelcomeScreen(List<string> options, bool clear = false)
    {
        if (clear) Clear();
        else SetCursorPosition(0, 0);

        UIMisc.DrawLine('=');
        CursorTop += 9;
        UIMisc.DrawLine('=');
        WriteLine();

        foreach (var option in options)
        {
            CursorLeft = CursorPos.MainMenuLeft;
            WriteLine($" {option}        ");
        }

        WriteLine();
        UIMisc.DrawLine('-');
    }

    public static int? PlayOnlineScreen()
    {
        WriteLine("WIP");
        return null;
    }

    public static void StartScreen(List<string> options, bool clear = false)
    {
        if (clear) Clear();
        else SetCursorPosition(0, 0);

        UIMisc.DrawLine('=');
        CursorTop += 9;
        UIMisc.DrawLine('=');
        WriteLine();

        foreach (var option in options)
        {
            CursorLeft = CursorPos.MainMenuLeft;
            WriteLine($" {option}        ");
        }

        WriteLine();
        UIMisc.DrawLine('-');
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

    public static void TreasureOpening(GameData gameData)
    {
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        WriteLine();
        WriteLine();
        UIMisc.WriteCenter(@"╔╤═══════════════════╤╗");
        UIMisc.WriteCenter(@"║├─────   ─── ──  ───┤║");
        UIMisc.WriteCenter(@"║├─   ──╔════╗ ──── ─┤║");
        UIMisc.WriteCenter(@"╠╪══════╬╤══╤╬═══════╪╣");
        UIMisc.WriteCenter(@"║│───  ─╢└──┘║  ─ ── │║");
        UIMisc.WriteCenter(@"║├── ── ╚════╝ ─  ───┤║");
        UIMisc.WriteCenter(@"║└───────────────────┘║");
        UIMisc.WriteCenter(@"╚═════════════════════╝");
        WriteLine();
        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');
        ReadKey(true);

        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        UIMisc.WriteCenter(@"╲  ╲  ╲  ╲  ╲   ╱  ╱  ╱  ╱  ╱");
        UIMisc.WriteCenter(@"╲   ╔╤══════╦════╦═══════╤╗   ╱");
        UIMisc.WriteCenter(@"╠╧══════╩════╩═══════╧╣");
        UIMisc.WriteCenter(@"║   ╲ ╲ ╲ ╲ ╱ ╱ ╱ ╱   ║");
        UIMisc.WriteCenter(@"║╲ ╲ ╲ ╲ ╲ ╳ ╱ ╱ ╱ ╱ ╱║");
        UIMisc.WriteCenter(@"╠╤══════╦╤══╤╦═══════╤╣");
        UIMisc.WriteCenter(@"║│───  ─╢└──┘║  ─ ── │║");
        UIMisc.WriteCenter(@"║├── ── ╚════╝ ─  ───┤║");
        UIMisc.WriteCenter(@"║└───────────────────┘║");
        UIMisc.WriteCenter(@"╚═════════════════════╝");
        WriteLine();
        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');
        ReadKey(true);
    }

    public static void TreasureOpening2(GameData gameData)
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

    public static void TreasureOpening3(GameData gameData)
    {

        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        WriteLine();
        WriteLine();
        UIMisc.WriteCenter(@"    .:::::..                                ");
        UIMisc.WriteCenter(@"  :=: .:-=-:-=-:.:::.                       ");
        UIMisc.WriteCenter(@" -- .=:              .-=-..::.              ");
        UIMisc.WriteCenter(@" +  =.                   .-=:.::=+-.:.      ");
        UIMisc.WriteCenter(@" +  ==:...             :-: .=-.      :=:    ");
        UIMisc.WriteCenter(@".===-.   .::==:.      .:: =-           *:   ");
        UIMisc.WriteCenter(@":= ==:  .:-==:    .:-==: ::         :==*:   ");
        UIMisc.WriteCenter(@":=.=     :=+@*.  :-=-:.  ::  :-=-.  :: *:   ");
        UIMisc.WriteCenter(@":= +      .*@%- .:-=-:.  --  ::=-:  .: *:   ");
        UIMisc.WriteCenter(@":= =        :.        :: :: =-      .: *:   ");
        UIMisc.WriteCenter(@":= :-=-..             :: :: =:      .: *:   ");
        UIMisc.WriteCenter(@" ::..-=:.:..:=-.      :: :: =-    :=:. *:   ");
        UIMisc.WriteCenter(@"         :::.:=-.::.:-+- :: =+:.:.:=:.:.    ");
        UIMisc.WriteCenter(@"                .:::.:--.:: :-:::.          ");
        UIMisc.DrawLine('-');
        ReadKey(true);
        
        Clear();
        DrawHeader();
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        UIMisc.WriteCenter(@"      :::::-::.                             ");
        UIMisc.WriteCenter(@"    ::-.:::-:  ::::::::..                   ");
        UIMisc.WriteCenter(@"  .*:. .:=:::::..       :--::::..           ");
        UIMisc.WriteCenter(@"   :=-+=:.:::. .:=:.:::.      ::=+=::-:.    ");
        UIMisc.WriteCenter(@"       .=::.  :==:..:.  .-=---.::=:    :=:  ");
        UIMisc.WriteCenter(@"          .:=: ...    .:==:..--.         :: ");
        UIMisc.WriteCenter(@"    ...-=:.      ..:==:....   :=:.       :-.");
        UIMisc.WriteCenter(@".+-:.                     .::==:..:=:.  :-. ");
        UIMisc.WriteCenter(@":= ==:                             .:-*#:   ");
        UIMisc.WriteCenter(@":=.=     :=+@*.-==-.           .:==:   *:   ");
        UIMisc.WriteCenter(@":= +      .*@%-=-::  .:=-===-. .:-  =: *:   ");
        UIMisc.WriteCenter(@":= =        :.        :: :: =-      .: *:   ");
        UIMisc.WriteCenter(@":= :-=-..             :: :: =:      .: *:   ");
        UIMisc.WriteCenter(@" ::..-=:.:..:=-.      :: :: =-    :=:. *:   ");
        UIMisc.WriteCenter(@"         :::.:=-.::.:-+- :: =+:.:.:=:.:.    ");
        UIMisc.WriteCenter(@"                .:::.:--.:: :-:::.          ");
        UIMisc.DrawLine('-');
        ReadKey(true);
    }
}