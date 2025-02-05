using System.Reflection;
using BLL;
using DAL;
using DAL.DBHandlers;
using DAL.Persistence.ConfigClasses;
using DAL.Persistence.DataModels;

using static System.Console;
using static ConsoleUtilities;

static class ServerUI
{
    public static string Header { get; } = "Server Control Center";

    public static async Task<(bool success, bool exit)> FillDBInfo()
    {
        Clear();
        DrawHeader(Header);

        WriteLine(" -- Connecting to MySql database");
        WriteLine(" 'ESC' to return");
        WriteLine(" Leave blank for default values");
        DrawLine('-');

        WriteLine(" Current available database IP:");
        WriteLine(" - Radmin VPN: 26.244.97.115");
        WriteLine(" -- Radmin VPN network: consoleconquer (password: 000000)");
        WriteLine(" - Localhost: 127.0.0.1");
        WriteLine(@" -- MySql dump file for local db is located at: MySql Files\Dump\");
        DrawLine('-');

        string? dbIP, db, uid, password;
        
        Write(" Database IP (default localhost): ");
        if ((dbIP = ReadInput()) == null)
            return (false, true);
        if (string.IsNullOrWhiteSpace(dbIP))
            dbIP = "127.0.0.1";

        Write(" Database name (default 'consoleconquer'): ");
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
        if (string.IsNullOrWhiteSpace(password))
            password = "";

        DrawLine('-');

        var (success, error) = await Server.InitializeDB(dbIP, db, uid, password);
        if (success)
        {
            WriteLine(" Database connection successful!");
            WriteLine(" Press any key to continue...");
            ReadKey(true);
            return (true, false);
        }
        else
        {
            WriteLine(" Database connection failed.");
            WriteLine($" Error: {error}");
            ReadKey(true);
            return (false, false);
        }
    }
    
    public static void MainMenu(string? serverIP, int port, bool serverOnline)
    {
        Clear();
        DrawHeader(Header);
        WriteLine($" Server's IP: {serverIP ?? "Any"}");
        WriteLine($" Server's port: {port}");
        Write(" Server's status: ");
        ForegroundColor = serverOnline ? ConsoleColor.Green : ConsoleColor.Red;
        WriteLine(serverOnline ? "Online" : "Offline");
        ResetColor();
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
            WriteLine(" 2. Manage player accounts");
            WriteLine(" 3. Modify game data");
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
            WriteLine($" -- Connected clients (Page {curPage + 1}/{(clients.Count - 1) / 10 + 1})");
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

    public static void ManageAccounts()
    {
        Clear();
        DrawHeader(Header);
        WriteLine(" -- Managing player accounts");
        WriteLine(" 'ESC' to return");
        DrawLine('-');

        WriteLine(" 1. View all accounts");
        WriteLine(" 2. Search for account");
        DrawLine('-');

        Write(" Enter choice: ");
    }

    public static async Task ViewAccounts()
    {
        var (accounts, error) = await UserDB.GetAll();
        if (accounts == null)
        {
            WriteLine(" Error: " + error);
            ReadKey(true);
            return;
        }

        int curPage = 0;
        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine($" -- Player accounts (Page {curPage + 1}/{(accounts.Count - 1) / 10 + 1})");
            WriteLine(" 'Left/Right' to navigate pages");
            WriteLine(" 'ESC' to return");
            DrawLine('-');

            foreach (var account in accounts.GetRange(curPage * 10, Math.Min(accounts.Count - curPage * 10, 10)))
            {
                WriteLine($" - ID: {account.UserID}, Username: {account.Username}");
            }

            DrawLine('=');

            ConsoleKey key = ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    if (curPage > 0)
                        curPage--;
                    break;

                case ConsoleKey.RightArrow:
                    if (curPage < (accounts.Count - 1) / 10)
                        curPage++;
                    break;

                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    public static async Task SearchAccount()
    {
        User? account = null;

        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" -- Searching for account");
            WriteLine(" 'ESC' to return");
            DrawLine('-');

            Write(" Enter ID: ");
            if (account != null)
                WriteLine(account.UserID);
            else
            {
                string? input = ReadInput();
                if (input == null)
                    return;

                if (!int.TryParse(input, out int id))
                {
                    WriteLine(" Invalid input.");
                    ReadKey(true);
                    continue;
                }

                (account, string error) = await UserDB.Get(id);

                if (account == null)
                {
                    WriteLine(" Error: " + error);
                    ReadKey(true);
                    continue;
                }
            }

            WriteLine(" Account found:");
            WriteLine($" - ID: {account.UserID}");
            WriteLine($" - Username: {account.Username}");
            WriteLine($" - Nickname: {account.Nickname}");
            WriteLine($" - Email: {account.Email}");

            DrawLine('-');
            WriteLine(" 'DEL' to delete account");

            ConsoleKey key = ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.Delete:
                    WriteLine(" Deleting account...");
                    WriteLine(" Press 'ENTER' to confirm");

                    if (ReadKey(true).Key != ConsoleKey.Enter)
                        continue;
                    
                    var (delSuccess, delError) = await UserDB.Delete(account.UserID);

                    if (delSuccess)
                    {
                        WriteLine(" Account deleted.");

                        foreach (var client in Server.ClientList)
                        {
                            if (client.User?.UserID == account.UserID)
                            {
                                client.Close();
                            }
                        }
                    }
                    else
                        WriteLine($" Error: {delError}");

                    ReadKey(true);
                    account = null;
                    continue;

                case ConsoleKey.Escape:
                    return;

                default:
                    continue;
            }
        }
    }

    public static void GameDataMenu()
    {
        Clear();
        DrawHeader(Header);
        WriteLine(" -- Modifying game data");
        WriteLine(" 'ESC' to return");
        DrawLine('-');

        WriteLine(" 1. Modify game assets");
        WriteLine(" 2. View game config");
        WriteLine(" 3. Modify game config");
        DrawLine('-');

        Write(" Enter choice: ");
    }

    public static void ViewGameConfig()
    {
        Clear();
        DrawHeader(Header);
        WriteLine(" -- Viewing game config");
        DrawLine('-');

        GameConfig config = ConfigManager.Instance.GameConfig;

        if (config == null)
        {
            WriteLine(" Error: Game config not found.");
            ReadKey(true);
            return;
        }

        WriteLine(" Game config:");

        string[] properties = [.. typeof(GameConfig).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name)];
        foreach (var prop in properties)
        {
            PropertyInfo propInfo = typeof(GameConfig).GetProperty(prop)!;
            WriteLine($" - {prop}: {propInfo.GetValue(config)}");
        }

        DrawLine('-');
        WriteLine(" Press any key to continue...");
        ReadKey(true);
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

            WriteLine(" (Leave blank to skip)");
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
            WriteLine(" -- Viewing activity log");
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