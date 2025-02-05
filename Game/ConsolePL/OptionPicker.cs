using BLL.Game.Components;
using BLL.Game.Components.Item;

using static System.Console;
using static ConsolePL.ConsoleUtilities;

namespace ConsolePL
{
    public static class OptionPicker
    {
        private enum Input
        {
            None, Up, Down, Confirm, Cancel
        }

        private static Input GetInput(ConsoleKey key)
        {
            return key switch
            {
                ConsoleKey.UpArrow or ConsoleKey.W
                    => Input.Up,
                ConsoleKey.DownArrow or ConsoleKey.S
                    => Input.Down,
                ConsoleKey.Enter or ConsoleKey.RightArrow or ConsoleKey.Spacebar or ConsoleKey.D
                    => Input.Confirm,
                ConsoleKey.Escape or ConsoleKey.LeftArrow or ConsoleKey.A
                    => Input.Cancel,
                _ => Input.None,
            };
        }

        private static int? Generic<T>(List<T> options, Action<T> printFunc, int startCursorTop = 0, int startCursorLeft = 0, int zoneHeight = -1, int padLength = 0)
        {
            if (options.Count == 0)
            {
                while (true)
                {
                    if (GetInput(ReadKey(true).Key) == Input.Cancel)
                        return null;
                }
            }

            if (zoneHeight == -1)
                zoneHeight = options.Count;

            int index = 0;
            int startIndex = 0;
            int stopIndex = Math.Min(options.Count, zoneHeight);
            int indexCursorTop = startCursorTop;
            bool refreshScreen = true;
            bool leavePointer = typeof(T) == typeof(string);

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
                                Write("▲".PadLeft(6));
                                DrawEmptyLine(padLength);
                                continue;
                            }
                            
                            if (stopIndex < options.Count && i == stopIndex - 1)
                            {
                                Write("▼".PadLeft(6));
                                DrawEmptyLine(padLength);
                                continue;
                            }

                            if (i == index)
                            {
                                indexCursorTop = CursorTop;
                                Write(" ►");
                            }
                            printFunc(options[i]);
                        }
                    }

                    refreshScreen = false;
                }

                switch (GetInput(ReadKey(true).Key))
                {
                    case Input.Up:
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

                    case Input.Down:
                        if (index < options.Count - 1)
                        {
                            index++;
                            if (index == stopIndex - 1 && stopIndex < options.Count)
                            {
                                startIndex++;
                                stopIndex++;
                            }
                            refreshScreen = true;
                            break;
                        }
                        continue;

                    case Input.Confirm:
                        SetCursorPosition(startCursorLeft, indexCursorTop);
                        if (leavePointer) Write(" ►");
                        printFunc(options[index]);
                        return index;

                    case Input.Cancel:
                        SetCursorPosition(startCursorLeft, indexCursorTop);
                        printFunc(options[index]);
                        return null;

                    default: continue;
                }
            }
        }

        public static int? Component<T>(List<T> components, int startCursorTop = 0, int startCursorLeft = 0, int zoneHeight = -1) where T : GameComponent
            => Generic(components, ComponentRenderer.Render, startCursorTop, startCursorLeft, zoneHeight);

        public static int? TradeItem<T>(List<T> items, bool buying = true, int startCursorTop = 0, int startCursorLeft = 0, int zoneHeight = -1) where T : GameItem
            => Generic(items, item => ComponentRenderer.RenderItemPrice(item, buying), startCursorTop, startCursorLeft, zoneHeight);

        public static int? String(List<string> options, int startCursorTop = 0, int startCursorLeft = 0, int zoneHeight = -1, int padLength = 0)
            => Generic(options, option =>
            {
                Write($" {option}");
                DrawEmptyLine(padLength);
            }, startCursorTop, startCursorLeft, zoneHeight, padLength);
    }
}