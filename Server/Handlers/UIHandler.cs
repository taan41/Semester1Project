using System.Text;

using static System.Console;
using static UIHandler.Numbers;


static class UIHandler
{
    public static class Numbers
    {
        public const int UIWidth = 70;

        public const int
            NameLen = 25,
            PlayerNameLen = 25;

        public const int
            PlayerBarLen = 30,
            MonsterBarLen = 10,
            EliteBarLen = 15,
            BossBarLen = 20;

    }
    
    public static void WriteCenter(string str)
        => WriteLine(str.PadLeft((UIWidth + str.Length - 1) / 2));

    public static void DrawLine(char lineChar)
        => WriteLine(new string(lineChar, UIWidth));

    public static void DrawBar(int currentValue, int maxValue, bool includeValue, int barLength, ConsoleColor color)
    {
        StringBuilder sb = new("[");
        sb.Append(new string('■', (int) ((double) currentValue / maxValue * barLength)).PadRight(barLength, '-'));
        sb.Append(']');
        if (includeValue)
            sb.Append($" {currentValue}/{maxValue}");
        sb.Append("  ");

        ForegroundColor = color;
        WriteLine(sb.ToString());
        ResetColor();
    }
}