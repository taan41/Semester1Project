using System.Text;

using static System.Console;
using static UIHelper.UIConstants;

static class UIHelper
{
    public static class UIConstants
    {
        public const int UIWidth = 70;

        public const int
            NameLen = 25,
            PlayerNameLen = Utilities.DataConstants.nicknameMax;

        public const int
            PlayerBarLen = 30,
            MonsterBarLen = 10,
            EliteBarLen = 15,
            BossBarLen = 20;

    }

    public static class UIMisc
    {
        public static void WriteCenter(string str)
        {
            CursorLeft = (UIWidth - str.Length) / 2;
            WriteLine(str);
        }

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

    public static class InteractiveUI
    {
        public static T? PickComponent<T>(int startCursorTop, List<T> components) where T : Component
            => PickComponent(0, startCursorTop, components);

        public static T? PickComponent<T>(int startCursorLeft, int startCursorTop, List<T> components) where T : Component
        {
            int pointer = 0, oldPtr;
            ConsoleKeyInfo keyPressed;

            SetCursorPosition(startCursorLeft, startCursorTop);
            Write(" ►");
            components[pointer].Print();
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
                SetCursorPosition(startCursorLeft, pointer + startCursorTop);
                
                Write(" ►");
                components[pointer].Print();
                SetCursorPosition(startCursorLeft, CursorTop - 1);
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
                        CursorLeft = startCursorLeft;
                        Write($" ► {options[pointer]} ◄");
                        return pointer;

                    case ConsoleKey.Escape:
                        CursorLeft = startCursorLeft;
                        Write($" {options[pointer]}  ");
                        return null;

                    default: continue;
                }

                SetCursorPosition(startCursorLeft, startCursorTop + oldPtr);
                Write($" {options[oldPtr]}  ");
                SetCursorPosition(startCursorLeft, startCursorTop + pointer);
                Write($" ► {options[pointer]}");
            }
        }
    }
}