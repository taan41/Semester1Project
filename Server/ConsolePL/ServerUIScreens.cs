using System.Reflection;
using System.Threading.Tasks;
using BLL;
using DAL;
using DAL.DBHandlers;
using DAL.Persistence.ConfigClasses;
using static System.Console;
using static ServerUIHelper;

static class ServerUI
{
    public static string Header { get; } = "Server Control Center";

    public static async Task<(bool success, bool exit)> FillDBInfoScreen()
    {
        string? server, db, uid, password;

        Clear();
        DrawHeader(Header);
        WriteLine(" MySql database info:");
        WriteLine(" 'ESC' to return");
        WriteLine(" Leave empty for default values");
        DrawLine('-');

        Write(" Server (default 'localhost'): ");
        if ((server = ReadInput()) == null)
            return (false, true);
        if (string.IsNullOrWhiteSpace(server))
            server = "localhost";

        Write(" Database (default 'consoleconquer'): ");
        if ((db = ReadInput()) == null)
            return (false, true);
        if (string.IsNullOrWhiteSpace(db))
            db = "consoleconquer";

        Write(" UID (default 'root'): ");
        if ((uid = ReadInput()) == null)
            return (false, true);
        if (string.IsNullOrWhiteSpace(uid))
            uid = "root";

        Write(" Password (default empty): ");
        if ((password = ReadInput(true)) == null)
            return (false, true);

        DrawLine('-');

        if (await Server.InitializeDB(server, db, uid, password))
        {
            WriteLine(" Database connection successful!");
            ReadKey(true);
            return (true, false);
        }
        else
        {
            WriteLine(" Database connection failed.");
            ReadKey(true);
            return (false, false);
        }
    }
    
    public static void MainMenuScreen(string? serverIP, int port, bool serverOnline)
    {
        Clear();
        DrawHeader(Header);
        WriteLine($" Server's IP: {serverIP ?? "Any"}");
        WriteLine($" Server's port: {port}");
        WriteLine($" Server's status: {(serverOnline ? "Online" : "Offline")}");
        DrawLine('-');

        if (!serverOnline)
        {
            WriteLine(" 1. Start server");
            WriteLine(" 2. Change IP");
            WriteLine(" 3. Change port");
            WriteLine(" 4. View activity log");
            WriteLine(" 0. Exit program");
        }
        else
        {
            WriteLine(" 1. View connected clients");
            WriteLine(" 2. Manage assets");
            WriteLine(" 3. Modify game config");
            WriteLine(" 4. View activity log");
            WriteLine(" 0. Shut down server");
        }

        DrawLine('-');
        Write(" Enter choice: ");
    }

    public static void ViewConnectedClients(List<ClientHandler> clients)
    {
        int curPage = 0;

        while(true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine($" Connected clients (Page {curPage + 1}/{(clients.Count - 1) / 10 + 1})");
            WriteLine(" 'Left/Right' to navigate pages");
            WriteLine(" 'ESC' to return");
            DrawLine('-');

            foreach(var client in clients.GetRange(curPage * 10, Math.Min(clients.Count - curPage * 10, 10)))
            {
                WriteLine($" • {client.EndPoint} (logged in as: '{client.User?.ToString() ?? "None"}')");
            }

            DrawLine('=');

            ConsoleKey key = ReadKey(true).Key;
            switch(key)
            {
                case ConsoleKey.LeftArrow:
                    if(curPage > 0)
                        curPage--;
                    break;

                case ConsoleKey.RightArrow:
                    if(curPage < (clients.Count - 1) / 10)
                        curPage++;
                    break;

                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    public static async Task ModifyGameConfig()
    {
        GameConfig config = ConfigManager.Instance.GameConfig;

        Clear();
        DrawHeader(Header);
        WriteLine(" -- Modifying game config");
        WriteLine(" 'ESC' to return");
        DrawLine('-');

        if (config == null)
        {
            WriteLine(" Error: Game config not found.");
            ReadKey(true);
            return;
        }

        string[] properties = [.. typeof(GameConfig).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name)];
        string[] filteredProps = [];

        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" -- Modifying game config");
            WriteLine(" 'ESC' to return");
            DrawLine('-');

            Write(" Properties name filter (empty for all): ");
            string? filter = ReadInput();
            if (filter == null)
                return;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filteredProps = [.. properties.Where(p => p.Contains(filter, StringComparison.OrdinalIgnoreCase))];
                if (filteredProps.Length == 0)
                {
                    WriteLine(" No properties found.");
                    ReadKey(true);
                    continue;
                }
            }
            else
            {
                filteredProps = properties;
            }

            WriteLine(" (Leave empty to skip)");
            WriteLine(" (Config will only be saved if all properties have been filled)");
            DrawLine('-');
            
            bool allFilled = true;
            foreach (var prop in filteredProps)
            {
                PropertyInfo propInfo = typeof(GameConfig).GetProperty(prop)!;
                
                Write($" {prop} (old value: {propInfo.GetValue(config)}): ");

                string? input = ReadInput();
                if (input == null)
                {
                    allFilled = false;
                    break;
                }

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (propInfo.CanWrite)
                {
                    try
                    {
                        propInfo.SetValue(config, Convert.ChangeType(input, propInfo.PropertyType));
                    }
                    catch (Exception)
                    {
                        WriteLine(" Invalid input.");
                        allFilled = false;
                        break;
                    }
                }
            }

            if (allFilled)
            {
                await config.Save();
                await ConfigManager.Instance.LoadConfig();

                DrawLine('-');
                WriteLine(" Game config updated.");
                ReadKey(true);
            }
            else
            {
                DrawLine('-');
                WriteLine(" Operation cancelled.");
                ReadKey(true);
            }
        }
    }

    public static void ViewLog()
    {
        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" Viewing activity log");
            WriteLine(" 'DEL' to clear log");
            WriteLine(" 'ESC' to return");
            DrawLine('-');
            
            LogHandler.WriteAllLog();
            LogHandler.ToggleLogView(true);

            bool validKey = false;
            while (!validKey)
            {
                ConsoleKey key = ReadKey(true).Key;
                switch(key)
                {
                    case ConsoleKey.Escape:
                        LogHandler.ToggleLogView(false);
                        return;

                    case ConsoleKey.Delete:
                        LogHandler.ClearLog();
                        validKey = true;
                        break;
                }
            }
        }
    }
}