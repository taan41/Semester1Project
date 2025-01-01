using static System.Console;
using static UIHelper;
using static Utilities;

static class GameUI
{
    public const string GameTitle = "CONSOLE CONQUER";

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

    public static async Task TitleAnimation(int cursorTop, CancellationToken stopToken)
    {
        int lineIndex = 0, lineLength = background[0].Length;

        try
        {
            while(!stopToken.IsCancellationRequested)
            {
                SetCursorPosition(0, cursorTop);
                // foreach(var line in background)
                //     WriteLine($"{line[lineIndex..]}{line[0..lineIndex]}");
                // SetCursorPosition(0, cursorTop + 1);
                // foreach(var line in title)
                //     UIMisc.WriteCenter(line);
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

    public static (int cursorLeft, int cursorTop, CancellationTokenSource animToken) WelcomeScreen(List<string> options)
    {
        CancellationTokenSource animTokenSource = new();

        Clear();
        SetCursorPosition(0, 0);
        UIMisc.DrawLine('=');
        int titleCursorTop = CursorTop;
        CursorTop += 9;
        UIMisc.DrawLine('=');
        WriteLine();
        ResetColor();

        int optionsCursorLeft = UIConstants.UIWidth / 10 * 4;
        int optionsCursorTop = CursorTop;

        foreach (var option in options)
        {
            CursorLeft = optionsCursorLeft;
            WriteLine($" {option}");
        }

        WriteLine();
        UIMisc.DrawLine('-');

        _ = Task.Run(() => TitleAnimation(titleCursorTop, animTokenSource.Token));

        return (optionsCursorLeft, optionsCursorTop, animTokenSource);
    }

    public static int? PlayOnline()
    {
        WriteLine("WIP");
        return null;
    }

    public static (int cursorLeft, int cursorTop) StartGameScreen(List<string> options)
    {
        Clear();
        int optionsCursorLeft = 0;
        int optionsCursorTop = CursorTop;

        foreach (var option in options)
        {
            CursorLeft = optionsCursorLeft;
            WriteLine($" {option}");
        }

        return (optionsCursorLeft, optionsCursorTop);
    }

    public static (int cursorLeft, int cursorTop) PickComponentScreen<T>(GameData gameData, List<T> components, string? pickMsg = null) where T: Component
    {
        Clear();
        UIMisc.DrawLine('=');
        ForegroundColor = ConsoleColor.Cyan;
        UIMisc.WriteCenter(GameTitle);
        ResetColor();
        UIMisc.DrawLine('=');
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        if (pickMsg != null)
            WriteLine(pickMsg);
        
        int optionsCursorLeft = 0;
        int optionsCursorTop = CursorTop;
        
        foreach (var component in components)
        {
            CursorLeft = optionsCursorLeft;
            component.Print();
        }

        UIMisc.DrawLine('-');
        gameData.Player.Print();
        UIMisc.DrawLine('-');

        return (optionsCursorLeft, optionsCursorTop);
    }

    public static (int routeCurTop, int invCurTop) PickRouteScreen(GameData gameData, List<Event> routes, List<string> invOptions)
    {
        Clear();
        UIMisc.DrawLine('=');
        ForegroundColor = ConsoleColor.Cyan;
        UIMisc.WriteCenter(GameTitle);
        ResetColor();
        UIMisc.DrawLine('=');
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        
        WriteLine(" -- Pick a route:");
        int routeCurTop = CursorTop;
        routes.ForEach(route => route.Print());
        UIMisc.DrawLine('-');

        int invCurTop = CursorTop;
        invOptions.ForEach(option => WriteLine($" {option}"));
        UIMisc.DrawLine('-');

        gameData.Player.Print();
        UIMisc.DrawLine('-');

        return (routeCurTop, invCurTop);
    }

    public static (int invCurTop, int equippedCurTop) PickInventoryScreen<T>(GameData gameData, List<T> inv, List<T> equipped) where T : Item
    {
        Clear();
        UIMisc.DrawLine('=');
        ForegroundColor = ConsoleColor.Cyan;
        UIMisc.WriteCenter(GameTitle);
        ResetColor();
        UIMisc.DrawLine('=');
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        
        WriteLine(" -- Inventory:");
        int invCurTop = CursorTop;
        inv.ForEach(item => item.Print());
        UIMisc.DrawLine('-');

        WriteLine(" -- Currently equipped:");
        int equippedCurTop = CursorTop;
        equipped.ForEach(item => item.Print());
        UIMisc.DrawLine('-');

        gameData.Player.Print();
        UIMisc.DrawLine('-');

        return (invCurTop, equippedCurTop);
    }

    public static (int monsterCurTop, int actionCurTop) FightScreen(GameData gameData, FightEvent fightEvent, List<string> actions)
    {
        Clear();
        UIMisc.DrawLine('=');
        ForegroundColor = ConsoleColor.Cyan;
        UIMisc.WriteCenter(GameTitle);
        ResetColor();
        UIMisc.DrawLine('=');
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        
        int monsterCurTop = CursorTop;
        fightEvent.Monsters.ForEach(monster => monster.Print());
        UIMisc.DrawLine('-');

        int actionCurTop = CursorTop;
        actions.ForEach(action => WriteLine($" {action}"));
        UIMisc.DrawLine('-');

        gameData.Player.Print();
        UIMisc.DrawLine('-');

        return (monsterCurTop, actionCurTop);
    }

    public static (int skillCurTop, int noticeCurTop) SkillFightScreen(GameData gameData, FightEvent fightEvent)
    {
        Clear();
        UIMisc.DrawLine('=');
        ForegroundColor = ConsoleColor.Cyan;
        UIMisc.WriteCenter(GameTitle);
        ResetColor();
        UIMisc.DrawLine('=');
        gameData.Progress.Print();
        UIMisc.DrawLine('-');
        
        fightEvent.Monsters.ForEach(monster => monster.Print());
        UIMisc.DrawLine('-');

        int skillCurTop = CursorTop;
        gameData.Player.Skills.ForEach(skill => skill.Print());
        int noticeCurTop = CursorTop;
        WriteLine();
        UIMisc.DrawLine('-');

        gameData.Player.Print();
        UIMisc.DrawLine('-');

        return (skillCurTop, noticeCurTop);
    }
}