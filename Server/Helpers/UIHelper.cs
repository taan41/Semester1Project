using static System.Console;
using static UIHelper.Numbers;


static class UIHelper
{
    public static class Numbers
    {
        public const int UIWidth = 70;
    }
    
    public static void WriteCenter(string str)
        => WriteLine(str.PadLeft((UIWidth + str.Length - 1) / 2));

    public static void DrawLine(char lineChar)
        => WriteLine(new string(lineChar, UIWidth));

    public static void DrawHeader(string header)
    {
        DrawLine('=');
        WriteCenter(header);
        DrawLine('=');
    }
}