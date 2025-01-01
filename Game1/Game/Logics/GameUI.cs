using static System.Console;
using static UIHelper;
using static Utilities;

static class GameUI
{
    public const string GameTitle = "CONSOLE CONQUER";

    public static (int cursorLeft, int cursorTop) WelcomeScreen(List<string> options)
    {
        Clear();
        ForegroundColor = ConsoleColor.Red;
        UIMisc.DrawLine('=');
        ForegroundColor = ConsoleColor.Cyan;
        //₀₁₁₀     ----------------------------------------------------------------------
        WriteLine("   ₁₀₁₀   ₁₀         ╔═╗ ╔═╗ ╦╗╦ ╔═╗ ╔═╗ ╦   ╔═╗     ₁ ₁₀       ₁₀₁  ");
        WriteLine("    ₁ ₁     ₀        ║   ║ ║ ║╚╣ ╚═╗ ║ ║ ║   ╠╣            ₀   ₁₁    ");
        WriteLine("                     ╚═╝ ╚═╝ ╩ ╩ ╚═╝ ╚═╝ ╩═╝ ╚═╝                 ₁₀₀ ");
        WriteLine("     ₀₀₁  ₁₁₀ ₁₀                                     ₁₀₁₀  ₀ ₁ ");
        WriteLine(" ₀₁                  ╔═╗ ╔═╗ ╦╗╦ ╔═╗ ╦ ╦ ╔═╗ ╦═╗          ₀₀  ₁₀     ");
        WriteLine("₁₁ ₀₀ ₁ ₀₁           ║   ║ ║ ║╚╣ ║╔╣ ║ ║ ╠╣  ╠╦╝                  ₀₁ ");
        WriteLine("   ₁         ₀₁      ╚═╝ ╚═╝ ╩ ╩ ╚╩╩ ╚═╝ ╩═╝ ╩╚═         ₁₁₀ ₀₁      ");
        ForegroundColor = ConsoleColor.Green;
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
        ForegroundColor = ConsoleColor.Blue;
        UIMisc.DrawLine('-');
        ResetColor();

        return (optionsCursorLeft, optionsCursorTop);
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