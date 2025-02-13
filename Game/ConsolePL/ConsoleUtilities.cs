using System.Text;
using BLL.Config;

using static System.Console;
using static ConsolePL.ConsoleUtilities.UIConstants;

namespace ConsolePL
{
    public static class ConsoleUtilities
    {
        private static ConfigManager Config => ConfigManager.Instance;

        public static object ConsoleLock { get; } = new();

        #region Constants
        public static class UIConstants
        {
            public const int
                UIWidth = 73, UIHeight = 24,
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
        #endregion

        #region Methods
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
        #endregion
    }
}