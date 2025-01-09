using System.Text;

using static System.Console;
using static GameUIHelper.UIConstants;

static class GameUIHelper
{
    public static object ConsoleLock { get; } = new();

    public static class UIConstants
    {
        public const int
            UIWidth = 70,
            PopupWidth = 40, PopupHeight = 11,
            PausePopupWidth = 34, PausePopupHeight = 15,
            MainZoneHeight = 6, SubZoneHeight = 4;

        public const int
            NameLen = 25;

        public const int
            PlayerBarLen = 30,
            MonsterBarLen = 10,
            EliteBarLen = 15,
            BossBarLen = 20;

        public const int
            InputLimit = 500;
    }

    public static class CursorPos
    {
        public const int
            TitleScreenMenuTop = 12, TitleScreenMenuLeft = UIWidth * 4 / 10,
            TitleScreenMenuLeft2 = UIWidth * 3 / 10,
            // StartScreenMainMenuTop = 4,
            // StartScreenSubMenuTop = 10,
            EndScreenMenuTop = 13, EndScreenMenuLeft = UIWidth * 4 / 10,
            TitleAnimationTop = 1,
            PopupTop = 4,
            ProgressBarTop = 3,
            MainZoneTop = 6, SubZoneTop = 13, PlayerZoneTop = 18,
            PausePopupTop = 4,
            PauseMenuTop = 7, PauseMenuLeft = UIWidth * 4 / 10, 
            PauseElapsedTimeTop = 13,
            BottomBorderTop = 22;
    }

    public static void WriteCenter(string str)
    {
        CursorLeft = (Math.Min(UIWidth, WindowWidth) - str.Length) / 2;
        WriteLine(str);
    }

    public static void DrawLine(char lineChar)
        => WriteLine(new string(lineChar, Math.Min(UIWidth, WindowWidth)));

    public static void DrawBar(int currentValue, int maxValue, bool includeValue, int barLength, ConsoleColor color)
    {
        StringBuilder sb = new("[");
        sb.Append(new string('■', currentValue * barLength / maxValue).PadRight(barLength, '-'));
        sb.Append(']');
        if (includeValue)
            sb.Append($" {currentValue}/{maxValue}");
        sb.Append(new string(' ', UIWidth - CursorLeft - sb.Length - 1));

        ForegroundColor = color;
        WriteLine(sb.ToString());
        ResetColor();
    }
    
    public static string? ReadInput(int? startCursorLeft = null, int? startCursorTop = null, bool intercept = false, int? characterLimit = null, bool clearAfterwards = false, StringBuilder? sb = null)
    {
        sb ??= new();
        characterLimit ??= InputLimit;

        bool done = false;
        startCursorLeft ??= CursorLeft;
        startCursorTop ??= CursorTop;

        // Set up the clipboard copy event
        // Override the default behavior of Ctrl+C
        CancelKeyPress += (sender, eventArgs) => {
            TextCopy.ClipboardService.SetText(sb.ToString());
            eventArgs.Cancel = true;
        };

        while(!done)
        {
            ConsoleKeyInfo key = ReadKey(true);

            switch(key.Key)
            {
                case ConsoleKey.Enter:
                    done = true;
                    continue;

                case ConsoleKey.Escape:
                    WriteLine();
                    return null;

                case ConsoleKey.Backspace:
                    lock (ConsoleLock)
                    {
                        SetCursorPosition((int) startCursorLeft, (int) startCursorTop);
                        Write(new string(' ', sb.Length));
                    }
                    
                    if((key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        sb.Clear();
                        continue;
                    }
                    else if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        break;
                    }
                    continue;
                
                case ConsoleKey.W:
                    // Ctrl+W is the same as Ctrl+Backspace on Windows
                    if((key.Modifiers & ConsoleModifiers.Control) != 0)
                        goto case ConsoleKey.Backspace;
                    else
                        goto default;

                case ConsoleKey.V:
                    if((key.Modifiers & ConsoleModifiers.Control) != 0)
                    {
                        string? copiedText = TextCopy.ClipboardService.GetText();
                        if (copiedText == null)
                            continue;
                        
                        int copiedLength = Math.Min(copiedText.Length, (int) characterLimit - sb.Length);
                        sb.Append(copiedText[..copiedLength]);
                        break;
                    }
                    else goto default;

                default:
                    if(sb.Length < characterLimit && !char.IsControl(key.KeyChar))
                    {
                        sb.Append(key.KeyChar);
                        break;
                    }
                    continue;
            }

            lock (ConsoleLock)
            {
                SetCursorPosition((int) startCursorLeft, (int) startCursorTop);
                WriteLine(intercept ? new string('*', sb.Length) : sb.ToString());
            }
        }

        string result = sb.ToString();
        sb.Clear();
        
        if (clearAfterwards)
        {
            lock (ConsoleLock)
            {
                SetCursorPosition((int) startCursorLeft, (int) startCursorTop);
                WriteLine(new string(' ', result.Length));
            }
        }

        return result;
    }

    public static class InteractiveUI
    {
        public static int? PickComponent<T>(int startCursorTop, List<T> components, bool exitUpwards = false, bool exitDownwards = false, bool startFromLast = false) where T : Component
            => PickComponent(0, startCursorTop, components, exitUpwards, exitDownwards, startFromLast);

        public static int? PickComponent<T>(int startCursorLeft, int startCursorTop, List<T> components, bool exitUpwards = false, bool exitDownwards = false, bool startFromLast = false) where T : Component
        {
            if (components.Count == 0)
            {
                while (true)
                    if (ReadKey(true).Key == ConsoleKey.Escape)
                        return null;
            }

            int index = startFromLast ? components.Count - 1 : 0;
            int indexCursorTop = startCursorTop;
            int? resultIndex;

            lock (ConsoleLock)
            {
                CursorTop = startCursorTop;
                for (int i = 0; i < components.Count; i++)
                {
                    CursorLeft = startCursorLeft;
                    if (i == index)
                    {
                        indexCursorTop = CursorTop;
                        Write(" ►");
                    }
                    components[i].Print();
                }
            }

            while (true)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow:
                        if (index > 0)
                        {
                            index--;
                            break;
                        }
                        else if (exitUpwards)
                        {
                            resultIndex = -1;
                            goto cancel_label;
                        }
                        continue;

                    case ConsoleKey.DownArrow:
                        if (index < components.Count - 1)
                        {
                            index++;
                            break;
                        }
                        else if (exitDownwards)
                        {
                            resultIndex = components.Count;
                            goto cancel_label;
                        }
                        continue;

                    case ConsoleKey.Enter:
                        return index;

                    case ConsoleKey.Escape:
                        resultIndex = null;
                        goto cancel_label;

                    cancel_label:
                        SetCursorPosition(startCursorLeft, indexCursorTop);
                        components[index].Print();
                        return resultIndex;

                    default: continue;
                }
                
                lock (ConsoleLock)
                {
                    CursorTop = startCursorTop;
                    for (int i = 0; i < components.Count; i++)
                    {
                        CursorLeft = startCursorLeft;
                        if (i == index)
                        {
                            indexCursorTop = CursorTop;
                            Write(" ►");
                        }
                        components[i].Print();
                    }
                }
            }
        }
        
        public static int? PickTradeItem<T>(int startCursorTop, List<T> items, bool buying, bool exitUpwards = false, bool exitDownwards = false, bool startFromLast = false) where T : Item
            => PickTradeItem(0, startCursorTop, items, buying, exitUpwards, exitDownwards, startFromLast);

        public static int? PickTradeItem<T>(int startCursorLeft, int startCursorTop, List<T> items, bool buying, bool exitUpwards = false, bool exitDownwards = false, bool startFromLast = false) where T : Item
        {
            if (items.Count == 0)
            {
                while (true)
                    if (ReadKey(true).Key == ConsoleKey.Escape)
                        return null;
            }

            int index = startFromLast ? items.Count - 1 : 0;
            int indexCursorTop = startCursorTop;
            int? resultIndex;

            lock (ConsoleLock)
            {
                CursorTop = startCursorTop;
                for (int i = 0; i < items.Count; i++)
                {
                    CursorLeft = startCursorLeft;
                    if (i == index)
                    {
                        indexCursorTop = CursorTop;
                        Write(" ►");
                    }
                    items[i].PrintPrice(buying);
                }
            }

            while (true)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow:
                        if (index > 0)
                        {
                            index--;
                            break;
                        }
                        else if (exitUpwards)
                        {
                            resultIndex = -1;
                            goto cancel_label;
                        }
                        continue;

                    case ConsoleKey.DownArrow:
                        if (index < items.Count - 1)
                        {
                            index++;
                            break;
                        }
                        else if (exitDownwards)
                        {
                            resultIndex = items.Count;
                            goto cancel_label;
                        }
                        continue;

                    case ConsoleKey.Enter:
                        return index;

                    case ConsoleKey.Escape:
                        return null;

                    cancel_label:
                        SetCursorPosition(startCursorLeft, indexCursorTop);
                        items[index].PrintPrice(buying);
                        return resultIndex;

                    default: continue;
                }
                
                lock (ConsoleLock)
                {
                    CursorTop = startCursorTop;
                    for (int i = 0; i < items.Count; i++)
                    {
                        CursorLeft = startCursorLeft;
                        if (i == index)
                        {
                            indexCursorTop = CursorTop;
                            Write(" ►");
                        }
                        items[i].PrintPrice(buying);
                    }
                }
            }
        }

        public static int? PickString(int startCursorTop, List<string> actions, bool exitUpwards = false, bool exitDownwards = false, bool startFromLast = false)
            => PickString(0, startCursorTop, actions, exitUpwards, exitDownwards, startFromLast);

        public static int? PickString(int startCursorLeft, int startCursorTop, List<string> actions, bool exitUpwards = false, bool exitDownwards = false, bool startFromLast = false)
        {
            if (actions.Count == 0)
            {
                while (true)
                    if (ReadKey(true).Key == ConsoleKey.Escape)
                        return null;
            }

            int index = startFromLast ? actions.Count - 1 : 0;
            int? resultIndex;
            int indexCursorTop = startCursorTop;

            lock (ConsoleLock)
            {
                CursorTop = startCursorTop;
                for (int i = 0; i < actions.Count; i++)
                {
                    CursorLeft = startCursorLeft;
                    if (i == index)
                    {
                        indexCursorTop = CursorTop;
                        WriteLine($" ► {actions[i]}");
                    }
                    else
                        WriteLine($" {actions[i]} ");
                }
            }

            while (true)
            {
                switch (ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow:
                        if (index > 0)
                        {
                            index--;
                            break;
                        }
                        else if (exitUpwards)
                        {
                            resultIndex = -1;
                            goto cancel_label;
                        }
                        continue;

                    case ConsoleKey.DownArrow:
                        if (index < actions.Count - 1)
                        {
                            index++;
                            break;
                        }
                        else if (exitDownwards)
                        {
                            resultIndex = actions.Count;
                            goto cancel_label;
                        }
                        continue;

                    case ConsoleKey.Enter:
                        SetCursorPosition(startCursorLeft, indexCursorTop);
                        Write($" ► {actions[index]} ◄");
                        return index;

                    case ConsoleKey.Escape:
                        resultIndex = null;
                        goto cancel_label;

                    cancel_label:
                        SetCursorPosition(startCursorLeft, indexCursorTop);
                        WriteLine($" {actions[index]}  ");
                        return resultIndex;

                    default: continue;
                }

                lock (ConsoleLock)
                {
                    CursorTop = startCursorTop;
                    for (int i = 0; i < actions.Count; i++)
                    {
                        CursorLeft = startCursorLeft;
                        if (i == index)
                        {
                            indexCursorTop = CursorTop;
                            WriteLine($" ► {actions[i]}");
                        }
                        else
                            WriteLine($" {actions[i]}  ");
                    }
                }
            }
        }
    }
}