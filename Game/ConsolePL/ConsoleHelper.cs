using System.Text;
using BLL.GameComponents;
using BLL.GameComponents.ItemComponents;
using DAL;

using static System.Console;
using static ConsolePL.ConsoleHelper.UIConstants;
using static ConsolePL.ComponentPrinter;

namespace ConsolePL
{
    public static class ConsoleHelper
    {
        private static ConfigManager Config => ConfigManager.Instance;

        public static object ConsoleLock { get; } = new();

        public static class UIConstants
        {
            public const int
                UIWidth = 70, UIHeight = 24,
                PopupWidth = 40, PopupHeight = 11,
                PausePopupWidth = 34, PausePopupHeight = 15,
                MainZoneHeight = 6, SubZoneHeight = 4;

            public const int
                PlayerBarLen = 30,
                MonsterBarLen = 10,
                EliteBarLen = 15,
                BossBarLen = 20;

            public const int
                InputLimit = 500;

            public static int
                NameLen => Config.DatabaseConfig.NicknameMax;
        }

        public static class CursorPos
        {
            public const int
                TitleScreenMenuTop = 12, TitleScreenMenuLeft = UIWidth * 4 / 10,
                TitleScreenMenuLeft2 = UIWidth * 3 / 10,
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

        public static void DrawEmptyLine(int length = 0)
            => DrawLine(' ', length);

        public static void DrawLine(char lineChar = '-', int length = 0)
            => WriteLine(new string(lineChar, length != 0 ? length : Math.Min(UIWidth, WindowWidth - CursorLeft - 1)));

        public static void DrawBar(int currentValue, int maxValue, bool includeValue, int barLength, ConsoleColor color)
        {
            StringBuilder sb = new("[");
            sb.Append(new string('■', currentValue * barLength / maxValue).PadRight(barLength, '-'));
            sb.Append(']');
            if (includeValue)
                sb.Append($" {currentValue}/{maxValue}");
            sb.Append(new string(' ', Math.Max(0, WindowWidth - CursorLeft - sb.Length - 1)));

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

        public static class Picker
        {
            private enum ControlKey
            {
                None, Up, Down, Confirm, Cancel
            }

            private static ControlKey GetCtrlKey(ConsoleKey key)
            {
                return key switch
                {
                    ConsoleKey.UpArrow or ConsoleKey.W
                        => ControlKey.Up,
                    ConsoleKey.DownArrow or ConsoleKey.S
                        => ControlKey.Down,
                    ConsoleKey.Enter or ConsoleKey.RightArrow or ConsoleKey.Spacebar or ConsoleKey.D
                        => ControlKey.Confirm,
                    ConsoleKey.Escape or ConsoleKey.LeftArrow or ConsoleKey.A
                        => ControlKey.Cancel,
                    _ => ControlKey.None,
                };
            }

            public static int? Component<T>(List<T> components, int startCursorTop = 0, int startCursorLeft = 0, int zoneHeight = -1) where T : ComponentAbstract
            {
                if (components.Count == 0)
                {
                    while (true)
                    {
                        if (GetCtrlKey(ReadKey(true).Key) == ControlKey.Cancel)
                            return null;
                    }
                }

                if (zoneHeight == -1)
                    zoneHeight = components.Count;

                int index = 0;
                int startIndex = 0;
                int stopIndex = Math.Min(components.Count - 1, zoneHeight - 1);
                int indexCursorTop = startCursorTop;
                int? resultIndex;
                bool refreshScreen = true;

                while (true)
                {
                    if (refreshScreen)
                    {
                        lock (ConsoleLock)
                        {
                            CursorTop = startCursorTop;

                            for (int i = startIndex; i <= stopIndex; i++)
                            {
                                CursorLeft = startCursorLeft;

                                if (i == startIndex && startIndex > 0)
                                {
                                    Write("▲".PadLeft(NameLen + 2));
                                    DrawEmptyLine();
                                    continue;
                                }
                                
                                if (stopIndex < components.Count - 1 && i == stopIndex)
                                {
                                    Write("▼".PadLeft(NameLen + 2));
                                    DrawEmptyLine();
                                    continue;
                                }

                                if (i == index)
                                {
                                    indexCursorTop = CursorTop;
                                    Write(" ►");
                                }
                                Print(components[i]);
                            }
                        }

                        refreshScreen = false;
                    }

                    switch (GetCtrlKey(ReadKey(true).Key))
                    {
                        case ControlKey.Up:
                            if (index > 0)
                            {
                                index--;
                                if (index == startIndex && startIndex > 0)
                                {
                                    startIndex--;
                                    stopIndex--;
                                }
                                refreshScreen = true;
                            }
                            continue;

                        case ControlKey.Down:
                            if (index < components.Count - 1)
                            {
                                index++;
                                if (index == stopIndex && stopIndex < components.Count - 1)
                                {
                                    startIndex++;
                                    stopIndex++;
                                }
                                refreshScreen = true;
                            }
                            continue;

                        case ControlKey.Confirm:
                            return index;

                        case ControlKey.Cancel:
                            resultIndex = null;
                            SetCursorPosition(startCursorLeft, indexCursorTop);
                            Print(components[index]);
                            return resultIndex;

                        default: continue;
                    }
                }
            }

            public static int? TradeItem<T>(List<T> items, bool buying = true, int startCursorTop = 0, int startCursorLeft = 0, int zoneHeight = -1) where T : Item
            {
                if (items.Count == 0)
                {
                    while (true)
                    {
                        if (GetCtrlKey(ReadKey(true).Key) == ControlKey.Cancel)
                            return null;
                    }
                }

                if (zoneHeight == -1)
                    zoneHeight = items.Count;

                int index = 0;
                int startIndex = 0;
                int stopIndex = Math.Min(items.Count, zoneHeight);
                int indexCursorTop = startCursorTop;
                bool refreshScreen = true;

                while (true)
                {
                    if (refreshScreen)
                    {
                        lock (ConsoleLock)
                        {
                            CursorTop = startCursorTop;
                            for (int i = startIndex; i < stopIndex; i++)
                            {
                                CursorLeft = startCursorLeft;

                                if (startIndex > 0 && i == startIndex)
                                {
                                    Write("▲".PadLeft(NameLen + 2));
                                    DrawEmptyLine();
                                    continue;
                                }
                                
                                if (stopIndex < items.Count && i == stopIndex - 1)
                                {
                                    Write("▼".PadLeft(NameLen + 2));
                                    DrawEmptyLine();
                                    continue;
                                }
                                
                                if (i == index)
                                {
                                    indexCursorTop = CursorTop;
                                    Write(" ►");
                                }
                                PrintPrice(items[i], buying);
                            }
                        }

                        refreshScreen = false;
                    }

                    switch (GetCtrlKey(ReadKey(true).Key))
                    {
                        case ControlKey.Up:
                            if (index > 0)
                            {
                                index--;
                                if (index == startIndex + 1 && startIndex > 0)
                                {
                                    startIndex--;
                                    stopIndex--;
                                }
                                refreshScreen = true;
                                break;
                            }
                            continue;

                        case ControlKey.Down:
                            if (index < items.Count - 1)
                            {
                                index++;
                                if (index == stopIndex - 1 && stopIndex < items.Count)
                                {
                                    startIndex++;
                                    stopIndex++;
                                }
                                refreshScreen = true;
                                break;
                            }
                            continue;

                        case ControlKey.Confirm:
                            return index;

                        case ControlKey.Cancel:
                            SetCursorPosition(startCursorLeft, indexCursorTop);
                            PrintPrice(items[index], buying);
                            return null;

                        default: continue;
                    }
                }
            }

            public static int? String(List<string> actions, int startCursorTop = 0, int startCursorLeft = 0, int zoneHeight = -1, int padLength = 0)
            {
                if (actions.Count == 0)
                {
                    while (true)
                    {
                        if (GetCtrlKey(ReadKey(true).Key) == ControlKey.Cancel)
                            return null;
                    }
                }

                if (zoneHeight == -1)
                    zoneHeight = actions.Count;

                int index = 0;
                int startIndex = 0;
                int stopIndex = Math.Min(actions.Count, zoneHeight);
                int indexCursorTop = startCursorTop;
                bool refreshScreen = true;

                while (true)
                {
                    if (refreshScreen)
                    {
                        lock (ConsoleLock)
                        {
                            CursorTop = startCursorTop;

                            for (int i = startIndex; i < stopIndex; i++)
                            {
                                CursorLeft = startCursorLeft;

                                if (startIndex > 0 && i == startIndex)
                                    Write("▲".PadLeft(6));
                                else if (stopIndex < actions.Count && i == stopIndex - 1)
                                    Write("▼".PadLeft(6));
                                else
                                {
                                    if (i == index)
                                    {
                                        indexCursorTop = CursorTop;
                                        Write("► ");
                                    }
                                    Write(actions[i]);
                                }
                                
                                DrawEmptyLine(padLength);
                            }
                        }

                        refreshScreen = false;
                    }
                    
                    switch (GetCtrlKey(ReadKey(true).Key))
                    {
                        case ControlKey.Up:
                            if (index > 0)
                            {
                                index--;
                                if (index == startIndex + 1 && startIndex > 0)
                                {
                                    startIndex--;
                                    stopIndex--;
                                }
                                refreshScreen = true;
                                break;
                            }
                            continue;

                        case ControlKey.Down:
                            if (index < actions.Count - 1)
                            {
                                index++;
                                if (index == stopIndex - 1 && stopIndex < actions.Count)
                                {
                                    startIndex++;
                                    stopIndex++;
                                }
                                refreshScreen = true;
                                break;
                            }
                            continue;

                        case ControlKey.Confirm:
                            SetCursorPosition(startCursorLeft, indexCursorTop);
                            Write($"► {actions[index]} ◄");
                            DrawEmptyLine();
                            return index;

                        case ControlKey.Cancel:
                            SetCursorPosition(startCursorLeft, indexCursorTop);
                            Write(actions[index]);
                            DrawEmptyLine();
                            return null;

                        default: continue;
                    }
                }
            }
        }
    }
}