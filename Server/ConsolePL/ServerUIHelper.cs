using System.Text;

using static System.Console;
using static ServerUIHelper.UIConstants;

static class ServerUIHelper
{
    public static class UIConstants
    {
        public const int
            UIWidth = 70,
            NameLen = 25,
            InputLimit = 500;
    }
    
    public static void WriteCenter(string str)
    {
        CursorLeft = (Math.Min(UIWidth, WindowWidth - 1) - str.Length) / 2;
        WriteLine(str);
    }

    public static void DrawLine(char lineChar)
        => WriteLine(new string(lineChar, Math.Min(UIWidth, WindowWidth - 1)));

    public static void DrawHeader(string header)
    {
        DrawLine('=');
        WriteCenter(header);
        DrawLine('=');
    }
    
    public static string? ReadInput(bool intercept = false, int? characterLimit = null, bool clearAfterwards = false, StringBuilder? sb = null)
    {
        sb ??= new();
        bool done = false;
        int index = 0, startCursorLeft = CursorLeft;
        CancelKeyPress += (sender, eventArgs) => {
            TextCopy.ClipboardService.SetText(sb.ToString());
            eventArgs.Cancel = true;
        };
        characterLimit ??= InputLimit;

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
                    HandleBackspace(sb, ref index, key.Modifiers);
                    break;
                
                case ConsoleKey.W:
                    if((key.Modifiers & ConsoleModifiers.Control) != 0)
                        goto case ConsoleKey.Backspace;
                    else
                        goto default;

                case ConsoleKey.V:
                    if((key.Modifiers & ConsoleModifiers.Control) != 0)
                        HandlePaste(sb, ref index, (int) characterLimit, intercept);
                    else goto default;
                    break;

                case ConsoleKey.LeftArrow:
                    if(index > 0)
                    {
                        MoveCursor(-1);
                        index--;
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if(index < sb.Length)
                    {
                        MoveCursor(1);
                        index++;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if(index > WindowWidth)
                    {
                        MoveCursor(-WindowWidth);
                        index -= WindowWidth;
                    }
                    else
                    {
                        MoveCursor(-index);
                        index = 0;
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if(sb.Length - index > WindowWidth)
                    {
                        MoveCursor(WindowWidth);
                        index += WindowWidth;
                    }
                    else
                    {
                        MoveCursor(sb.Length - index);
                        index = sb.Length;
                    }
                    break;

                default:
                    if((startCursorLeft + index + 1) / WindowWidth < WindowHeight && sb.Length < characterLimit)
                        HandleDefaultKey(sb, ref index, key.KeyChar, startCursorLeft, intercept);
                    break;
            }
        }

        string result = sb.ToString();
        sb.Clear();
        
        MoveCursor(-index);
        if (clearAfterwards)
        {
            Write(new string(' ', result.Length));
            MoveCursor(-result.Length);
        }
        else
            WriteLine(intercept ? new string('*', result.Length) : result);

        return result;
    }

    private static void HandleBackspace(StringBuilder sb, ref int index, ConsoleModifiers modifiers)
    {
        if(index < 1) return;

        int removedLength = 1;

        if((modifiers & ConsoleModifiers.Control) != 0)
        {
            sb.Remove(0, index);
            removedLength = index;
            MoveCursor(-index);
            index = 0;
        }
        else
        {
            sb.Remove(--index, 1);
            MoveCursor(-1);
        }

        (int oldLeft, int oldTop) = GetCursorPosition();
        Write(sb.ToString()[index ..]);
        Write(new string(' ', removedLength));
        SetCursorPosition(oldLeft, oldTop);
    }

    private static void HandlePaste(StringBuilder sb, ref int index, int limit, bool intercept)
    {
        string? copiedText = TextCopy.ClipboardService.GetText();
        if (copiedText == null)
            return;
        
        int copiedLength = Math.Min(copiedText.Length, limit - sb.Length);
        sb.Insert(index, copiedText[..copiedLength]);

        if (intercept)
        {
            Write(new string('*', sb.ToString()[index..].Length));
        }
        else
        {
            Write(sb.ToString()[index..]);
        }

        index += copiedLength;
        MoveCursor(index - sb.Length);
    }

    private static void HandleDefaultKey(StringBuilder sb, ref int index, char keyChar, int startCursorLeft, bool intercept)
    {
        sb.Insert(index++, keyChar);
        
        char displayChar = intercept ? '*' : keyChar;

        if(index < sb.Length)
        {
            Write(displayChar);
            Write(intercept ? new string('*', sb.Length - index) : sb.ToString()[index ..]);
            MoveCursor(index - sb.Length);

            if((sb.Length + startCursorLeft) % WindowWidth == 0)
                MoveCursor(1);
        }
        else if(CursorLeft + 1 == BufferWidth)
            WriteLine(displayChar);
        else
            Write(displayChar);
    }
    
    public static void MoveCursor(int charAmount)
    {
        int targetLeft = CursorLeft + charAmount;
        int targetTop = CursorTop;

        if(targetLeft < 0)
        {
            targetTop += (int) Math.Floor((double) targetLeft / WindowWidth);
            targetLeft = (targetLeft % WindowWidth + WindowWidth) % WindowWidth; // Wrap to valid range
        }
        else if(targetLeft >= WindowWidth)
        {
            targetTop += targetLeft / WindowWidth;
            targetLeft %= WindowWidth;
        }

        if(targetTop < 0) targetTop = 0;
        if(targetTop > WindowHeight)
        {
            targetTop = WindowHeight;
            targetLeft = WindowWidth;
        }

        SetCursorPosition(targetLeft, targetTop);
    }
}