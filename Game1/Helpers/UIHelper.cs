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
        public static int? PickComponent<T>(int startCursorTop, List<T> components, bool exitUpwards = false, bool exitDownwards = false, bool startUpwards = false) where T : Component
            => PickComponent(0, startCursorTop, components, exitUpwards, exitDownwards, startUpwards);

        public static int? PickComponent<T>(int startCursorLeft, int startCursorTop, List<T> components, bool exitUpwards = false, bool exitDownwards = false, bool startUpwards = false) where T : Component
        {
            int curIndex = startUpwards ? components.Count - 1 : 0, oldIndex;

            if (components.Count == 0)
            {
                while (true)
                    if (ReadKey(true).Key == ConsoleKey.Escape)
                        return null;
            }

            ConsoleKeyInfo keyPressed;

            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
            Write(" ►");
            components[curIndex].Print();
            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);

            while (true)
            {
                keyPressed = ReadKey(true);
                switch(keyPressed.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (curIndex > 0)
                        {
                            oldIndex = curIndex--;
                            break;
                        }
                        else if (exitUpwards)
                        {
                            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                            components[curIndex].Print();
                            return -1;
                        }
                        continue;

                    case ConsoleKey.DownArrow:
                        if (curIndex < components.Count - 1)
                        {
                            oldIndex = curIndex++;
                            break;
                        }
                        else if (exitDownwards)
                        {
                            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                            components[curIndex].Print();
                            return components.Count;
                        }
                        continue;

                    case ConsoleKey.Enter:
                        SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                        Write(" ►");
                        components[curIndex].Print();
                        return curIndex;

                    case ConsoleKey.Escape:
                        SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                        components[curIndex].Print();
                        return null;

                    default: continue;
                }

                SetCursorPosition(startCursorLeft, startCursorTop + oldIndex);
                components[oldIndex].Print();
                
                SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                Write(" ►");
                components[curIndex].Print();
            }
        }

        public static int? PickOption(int startCursorTop, List<string> options, bool exitUpwards = false, bool exitDownwards = false, bool startUpwards = false)
            => PickOption(0, startCursorTop, options, exitUpwards, exitDownwards, startUpwards);

        public static int? PickOption(int startCursorLeft, int startCursorTop, List<string> options, bool exitUpwards = false, bool exitDownwards = false, bool startUpwards = false)
        {
            int curIndex = startUpwards ? options.Count - 1 : 0, oldIndex;
            ConsoleKeyInfo keyPressed;

            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
            WriteLine($" ► {options[curIndex]} ");
            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);

            while (true)
            {
                keyPressed = ReadKey(true);
                switch(keyPressed.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (curIndex > 0)
                        {
                            oldIndex = curIndex--;
                            break;
                        }
                        else if (exitUpwards)
                        {
                            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                            Write($" {options[curIndex]}  ");
                            return -1;
                        }
                        continue;

                    case ConsoleKey.DownArrow:
                        if (curIndex < options.Count - 1)
                        {
                            oldIndex = curIndex++;
                            break;
                        }
                        else if (exitDownwards)
                        {
                            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                            Write($" {options[curIndex]}  ");
                            return options.Count;
                        }
                        continue;

                    case ConsoleKey.Enter:
                        SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                        Write($" ► {options[curIndex]} ◄");
                        return curIndex;

                    case ConsoleKey.Escape:
                        SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                        Write($" {options[curIndex]}  ");
                        return null;

                    default: continue;
                }

                SetCursorPosition(startCursorLeft, startCursorTop + oldIndex);
                Write($" {options[oldIndex]}  ");

                SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                Write($" ► {options[curIndex]}");
            }
        }
    }
}