using System.Text;

using static System.Console;
using static UINumbers;

static class UINumbers
{
    public const int UIWidth = 70;

    public const int NameLen = 20;

    public const int
        PlayerBarLen = 30,
        MonsterBarLen = 10,
        EliteBarLen = 15,
        BossBarLen = 20;

}

static class UIHandler
{
    public static void DrawBorder(char borderChar)
        => WriteLine(new string(borderChar, UIWidth));

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

    public static T? PickFromArray<T>(int startCursorTop, List<T> components) where T : Component
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

    public static Monster? PickMonster(int initCurorTop, List<Monster> monsters)
    {
        int pointer = 0, oldPtr;
        ConsoleKeyInfo keyPressed;
        SetCursorPosition(0, initCurorTop);
        Write(" ►");
        monsters[pointer].Print();
        SetCursorPosition(0, CursorTop - 1);

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
                    if (pointer < monsters.Count - 1)
                    {
                        oldPtr = pointer++;
                        break;
                    }
                    continue;

                case ConsoleKey.Enter:
                    monsters[pointer].Print();
                    return monsters[pointer];

                case ConsoleKey.Escape:
                    monsters[pointer].Print();
                    return null;

                default: continue;
            }

            monsters[oldPtr].Print();
            SetCursorPosition(0, pointer + initCurorTop);
            Write(" ►");
            monsters[pointer].Print();
            SetCursorPosition(0, CursorTop - 1);
        }
    }

    public static int? PickAction(int initCurorTop, List<string> actions)
    {
        int pointer = 0, oldPtr;
        ConsoleKeyInfo keyPressed;

        SetCursorPosition(0, initCurorTop);
        WriteLine($" ► {actions[pointer]} ");
        SetCursorPosition(0, initCurorTop);

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
                    if (pointer < actions.Count - 1)
                    {
                        oldPtr = pointer++;
                        break;
                    }
                    continue;

                case ConsoleKey.Enter:
                    Write($" ► {actions[pointer]} ");
                    return pointer;

                case ConsoleKey.Escape:
                    Write($" {actions[pointer]}  ");
                    return null;

                default: continue;
            }

            Write($" {actions[oldPtr]}  ");
            SetCursorPosition(0, pointer + initCurorTop);
            Write($" ► {actions[pointer]}");
            CursorLeft = 0;
        }
    }

    public static Skill? PickSkill(int initCurorTop, List<Skill> skills)
    {
        int pointer = 0, oldPtr;
        ConsoleKeyInfo keyPressed;

        SetCursorPosition(0, initCurorTop);

        Write(" ►");
        skills[pointer].Print();
        for (int i = 1; i < skills.Count; i++)
            skills[i].Print();

        SetCursorPosition(0, initCurorTop);

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
                    if (pointer < skills.Count - 1)
                    {
                        oldPtr = pointer++;
                        break;
                    }
                    continue;

                case ConsoleKey.Enter:
                    skills[pointer].Print();
                    return skills[pointer];

                case ConsoleKey.Escape:
                    skills[pointer].Print();
                    return null;

                default: continue;
            }

            skills[oldPtr].Print();
            SetCursorPosition(0, pointer + initCurorTop);
            Write(" ►");
            skills[pointer].Print();
            SetCursorPosition(0, CursorTop - 1);
        }
    }
}