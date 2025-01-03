using System.Text;

using static System.Console;
using static UIHelper.UIConstants;

static class UIHelper
{
    public static class UIConstants
    {
        public const int
            UIWidth = 70,
            WarningWidth = 40, WarningHeight = 11,
            PauseWidth = 34, PauseHeight = 15,
            MainZoneHeight = 6, SubZoneHeight = 4;

        public const int
            NameLen = 25,
            PlayerNameLen = Utilities.DataConstants.nicknameMax;

        public const int
            PlayerBarLen = 30,
            MonsterBarLen = 10,
            EliteBarLen = 15,
            BossBarLen = 20;
    }

    public static class CursorPos
    {
        public const int
            MainMenuLeft = UIWidth / 10 * 4, MainMenuTop = 12,
            MainTitleTop = 1,
            WarningTop = 4,
            ProgressTop = 3,
            MainZoneTop = 6, SubZoneTop = 13, PlayerZoneTop = 18,
            PauseBorderTop = 4,
            PauseOptionLeft = UIWidth / 10 * 4, PauseOptionTop = 7,
            PauseTimeTop = 13,
            EndMenuTop = 13;
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
            sb.Append(new string('■', currentValue * barLength / maxValue).PadRight(barLength, '-'));
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
            if (components.Count == 0)
            {
                while (true)
                    if (ReadKey(true).Key == ConsoleKey.Escape)
                        return null;
            }

            int curIndex = startUpwards ? components.Count - 1 : 0, oldIndex;
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
        
        public static int? TradeItem<T>(int startCursorTop, List<T> items, bool buying, bool exitUpwards = false, bool exitDownwards = false, bool startUpwards = false) where T : Item
            => TradeItem(0, startCursorTop, items, buying, exitUpwards, exitDownwards, startUpwards);

        public static int? TradeItem<T>(int startCursorLeft, int startCursorTop, List<T> items, bool buying, bool exitUpwards = false, bool exitDownwards = false, bool startUpwards = false) where T : Item
        {
            if (items.Count == 0)
            {
                while (true)
                    if (ReadKey(true).Key == ConsoleKey.Escape)
                        return null;
            }

            int curIndex = startUpwards ? items.Count - 1 : 0, oldIndex;
            ConsoleKeyInfo keyPressed;

            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
            Write(" ►");
            items[curIndex].PrintPrice(buying);
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
                            items[curIndex].PrintPrice(buying);
                            return -1;
                        }
                        continue;

                    case ConsoleKey.DownArrow:
                        if (curIndex < items.Count - 1)
                        {
                            oldIndex = curIndex++;
                            break;
                        }
                        else if (exitDownwards)
                        {
                            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                            items[curIndex].PrintPrice(buying);
                            return items.Count;
                        }
                        continue;

                    case ConsoleKey.Enter:
                        SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                        Write(" ►");
                        items[curIndex].PrintPrice(buying);
                        return curIndex;

                    case ConsoleKey.Escape:
                        SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                        items[curIndex].PrintPrice(buying);
                        return null;

                    default: continue;
                }

                SetCursorPosition(startCursorLeft, startCursorTop + oldIndex);
                items[oldIndex].PrintPrice(buying);
                
                SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                Write(" ►");
                items[curIndex].PrintPrice(buying);
            }
        }

        public static int? PickString(int startCursorTop, List<string> actions, bool exitUpwards = false, bool exitDownwards = false, bool startUpwards = false)
            => PickString(0, startCursorTop, actions, exitUpwards, exitDownwards, startUpwards);

        public static int? PickString(int startCursorLeft, int startCursorTop, List<string> actions, bool exitUpwards = false, bool exitDownwards = false, bool startUpwards = false)
        {
            if (actions.Count == 0)
            {
                while (true)
                    if (ReadKey(true).Key == ConsoleKey.Escape)
                        return null;
            }

            int curIndex = startUpwards ? actions.Count - 1 : 0, oldIndex;
            ConsoleKeyInfo keyPressed;

            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
            WriteLine($" ► {actions[curIndex]} ");
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
                            Write($" {actions[curIndex]}  ");
                            return -1;
                        }
                        continue;

                    case ConsoleKey.DownArrow:
                        if (curIndex < actions.Count - 1)
                        {
                            oldIndex = curIndex++;
                            break;
                        }
                        else if (exitDownwards)
                        {
                            SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                            Write($" {actions[curIndex]}  ");
                            return actions.Count;
                        }
                        continue;

                    case ConsoleKey.Enter:
                        SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                        Write($" ► {actions[curIndex]} ◄");
                        return curIndex;

                    case ConsoleKey.Escape:
                        SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                        Write($" {actions[curIndex]}  ");
                        return null;

                    default: continue;
                }

                SetCursorPosition(startCursorLeft, startCursorTop + oldIndex);
                Write($" {actions[oldIndex]}  ");

                SetCursorPosition(startCursorLeft, startCursorTop + curIndex);
                Write($" ► {actions[curIndex]}");
            }
        }
    }
}