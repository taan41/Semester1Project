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
            PlayerNameLen = MagicNum.nicknameMax;

        public const int
            PlayerBarLen = 30,
            MonsterBarLen = 10,
            EliteBarLen = 15,
            BossBarLen = 20;

    }

    public class Misc
    {
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

        public static T? PickComponent<T>(int startCursorTop, List<T> components) where T : Component
        {
            int pointer = 0, oldPtr;
            ConsoleKeyInfo keyPressed;

            SetCursorPosition(0, startCursorTop);
            Write(" ►");
            components[pointer].Print();
            SetCursorPosition(0, startCursorTop);

            while (true)
            {
                keyPressed = ReadKey(true);
                switch(keyPressed.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (pointer > 0)
                        {
                            oldPtr = pointer--;
                            break;
                        }
                        continue;

                    case ConsoleKey.DownArrow:
                        if (pointer < components.Count - 1)
                        {
                            oldPtr = pointer++;
                            break;
                        }
                        continue;

                    case ConsoleKey.Enter:
                        Write(" ►");
                        components[pointer].Print();
                        return components[pointer];

                    case ConsoleKey.Escape:
                        components[pointer].Print();
                        return null;

                    default: continue;
                }

                components[oldPtr].Print();
                SetCursorPosition(0, pointer + startCursorTop);
                
                Write(" ►");
                components[pointer].Print();
                SetCursorPosition(0, CursorTop - 1);
            }
        }

        public static int? PickOption(int startCursorTop, List<string> options)
            => PickOption(0, startCursorTop, options);

        public static int? PickOption(int startCursorLeft, int startCursorTop, List<string> options)
        {
            int pointer = 0, oldPtr;
            ConsoleKeyInfo keyPressed;

            SetCursorPosition(startCursorLeft, startCursorTop);
            WriteLine($" ► {options[pointer]} ");
            SetCursorPosition(startCursorLeft, startCursorTop);

            while (true)
            {
                keyPressed = ReadKey(true);
                switch(keyPressed.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (pointer > 0)
                        {
                            oldPtr = pointer--;
                            break;
                        }
                        continue;

                    case ConsoleKey.DownArrow:
                        if (pointer < options.Count - 1)
                        {
                            oldPtr = pointer++;
                            break;
                        }
                        continue;

                    case ConsoleKey.Enter:
                        Write($" ► {options[pointer]} ");
                        return pointer;

                    case ConsoleKey.Escape:
                        Write($" {options[pointer]}  ");
                        return null;

                    default: continue;
                }

                Write($" {options[oldPtr]}  ");
                SetCursorPosition(startCursorLeft, pointer + startCursorTop);
                Write($" ► {options[pointer]}");
                CursorLeft = startCursorLeft;
            }
        }
    }

    public class Menu
    {
        public static int? Welcome()
        {
            Clear();
            ForegroundColor = ConsoleColor.Red;
            Misc.DrawLine('=');
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
            Misc.DrawLine('=');
            WriteLine();
            ResetColor();

            int startCursorLeft = UIWidth / 10 * 4;
            int startCursorTop = CursorTop;

            List<string> options = ["LOGIN", "PLAY OFFLINE", "EXIT"];
            foreach (var option in options)
            {
                CursorLeft = startCursorLeft;
                WriteLine($" {option}");
            }

            ForegroundColor = ConsoleColor.Blue;
            Misc.DrawLine('-');
            ResetColor();

            return Misc.PickOption(startCursorLeft, startCursorTop, options);
        }
    }
}