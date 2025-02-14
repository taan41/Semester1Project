using System.Reflection;
using BLL;
using DAL.DBHandlers;
using DAL.Config;
using DAL.DataModels;

using static System.Console;
using static ConsolePL.ConsoleUtilities;
using System.Net;
using BLL.Utilities;

namespace ConsolePL;

public static class ServerControlUI
{
    private static ConfigManager Config => ConfigManager.Instance;
    private static Server Server => Server.Instance;

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

        string? dbIP, uid, password;
        
        Write(" Database IP (default localhost): ");
        if ((dbIP = ReadInput()) == null)
            return (false, true);
        if (string.IsNullOrWhiteSpace(dbIP))
            dbIP = "127.0.0.1";

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

        var (success, error) = await Server.InitializeDB(dbIP, uid, password);
        if (success)
        {
            WriteLine(" Database connection successful!");
            WriteLine(" Press any key to continue...");
            ReadKey(true);
            return (true, false);
        }
        else
        {
            WriteLine(" (!) Database connection failed.");
            WriteLine($" Error details: {error}");
            ReadKey(true);
            return (false, false);
        }
    }
    
    public static void MainMenu(int port, bool serverOnline)
    {
        Clear();
        DrawHeader(Header);
        WriteLine($" Server's port: {port}");
        Write(" Server's status: ");
        ForegroundColor = serverOnline ? ConsoleColor.Green : ConsoleColor.Red;
        WriteLine(serverOnline ? "Online" : "Offline");
        ResetColor();
        DrawLine('-');

        if (!serverOnline)
        {
            WriteLine(" 1. Start server");
            WriteLine(" 2. Manage server's address");
            WriteLine(" 3. View activity log");
            WriteLine(" 0. Exit program");
        }
        else
        {
            WriteLine(" 1. View connected clients");
            WriteLine(" 2. Manage users");
            WriteLine(" 3. Modify game data");
            WriteLine(" 4. View activity log");
            WriteLine(" 0. Shut down server");
        }

        DrawLine('-');
        Write(" Enter choice: ");
    }

    public static async Task<int> ManageServerAddresses(int curPort)
    {
        bool viewingIPs = true;

        while (true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" -- Managing server's valid addresses");
            WriteLine(" 'ESC' or 0 to return");
            DrawLine('-');

            if (viewingIPs)
            {
                WriteLine(" Valid IPs:");
                for (int i = 0; i < Config.ServerConfig.ServerIPs.Count; i++)
                    WriteLine($" - {Config.ServerConfig.ServerIPs[i]}");
            }
            else
            {
                WriteLine(" Valid ports:");
                for (int i = 0; i < Config.ServerConfig.ServerPorts.Count; i++)
                    WriteLine($" - {Config.ServerConfig.ServerPorts[i]}");
            }

            DrawLine('-');
            WriteLine($" 1. Toggle to {(viewingIPs ? "port" : "IP")} view");
            WriteLine(" 2. Add");
            WriteLine(" 3. Remove");
            if (!viewingIPs)
                WriteLine($" 4. Change server's current port: {curPort}");
            DrawLine('-');

            Write(" Enter choice: ");

            switch (ReadInput())
            {
                case "1":
                    viewingIPs = !viewingIPs;
                    continue;

                case "2":
                    if (viewingIPs)
                    {
                        Write(" Enter new IP: ");
                        string? newIP = ReadInput();
                        if (newIP == null)
                            continue;

                        if (!Helper.CheckIPv4(newIP))
                        {
                            WriteLine(" (!) Invalid IP address");
                            ReadKey(true);
                            continue;
                        }

                        if (Config.ServerConfig.ServerIPs.Contains(newIP))
                        {
                            WriteLine(" (!) IP already exists");
                            ReadKey(true);
                            continue;
                        }

                        Config.ServerConfig.ServerIPs.Add(newIP);
                        await Config.ServerConfig.Save();
                        continue;
                    }
                    else
                    {
                        Write(" Enter new port: ");
                        string? newPort = ReadInput();
                        if (newPort == null)
                            continue;

                        if (!Helper.CheckPort(newPort))
                        {
                            WriteLine(" (!) Invalid port");
                            ReadKey(true);
                            continue;
                        }

                        if (Config.ServerConfig.ServerPorts.Contains(Convert.ToInt32(newPort)))
                        {
                            WriteLine(" (!) Port already exists");
                            ReadKey(true);
                            continue;
                        }

                        Config.ServerConfig.ServerPorts.Add(Convert.ToInt32(newPort));
                        await Config.ServerConfig.Save();
                        continue;
                    }

                case "3":
                    if (viewingIPs)
                    {
                        Write(" Enter IP to remove: ");
                        string? remIP = ReadInput();
                        if (remIP == null)
                            continue;

                        if (!Config.ServerConfig.ServerIPs.Remove(remIP))
                        {
                            WriteLine(" (!) IP not found");
                            ReadKey(true);
                            continue;
                        }

                        await Config.ServerConfig.Save();
                        continue;
                    }
                    else
                    {
                        Write(" Enter port to remove: ");
                        string? remPort = ReadInput();
                        if (remPort == null)
                            continue;

                        if (!Config.ServerConfig.ServerPorts.Remove(Convert.ToInt32(remPort)))
                        {
                            WriteLine(" (!) Port not found");
                            ReadKey(true);
                            continue;
                        }

                        await Config.ServerConfig.Save();
                        continue;
                    }

                case "4":
                    if (viewingIPs)
                        continue;
                    
                    Write(" Enter new port: ");
                    string? port = ReadInput();
                    if (port == null)
                        continue;

                    if (!Helper.CheckPort(port))
                    {
                        WriteLine(" (!) Invalid port");
                        ReadKey(true);
                        continue;
                    }

                    if (!Config.ServerConfig.ServerPorts.Contains(Convert.ToInt32(port)))
                    {
                        WriteLine(" (!) Port not in list of valid ports");
                        ReadKey(true);
                        continue;
                    }

                    curPort = Convert.ToInt32(port);
                    continue;

                case "0": case null:
                    return curPort;

                default: continue;
            }
        }
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
            WriteLine(" 'ESC' or 0 to return");
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
                case ConsoleKey.D0:
                case ConsoleKey.NumPad0:
                    return;
            }
        }
    }

    public static async Task ManageUsers()
    {
        ConsoleKey[] konamiCode = [ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow];

        var (allUsers, error) = await UserDB.GetAll();
        if (allUsers == null)
        {
            WriteLine(" Error: " + error);
            ReadKey(true);
            return;
        }

        List<User> displayedUsers = allUsers;
        User? filteredByID = null;
        string? filter = null;

        int curPage = 0;
        while (true)
        {
            Clear();
            DrawHeader(Header);

            if (filteredByID != null)
            {
                WriteLine(" -- Player account details");
                DrawLine('-');
                WriteLine($" + ID: {filteredByID.UserID}");
                WriteLine($" + Username: {filteredByID.Username}");
                WriteLine($" + Nickname: {filteredByID.Nickname ?? "None"}");
                WriteLine($" + Email: {filteredByID.Email ?? "None"}");
            }
            else
            {
                WriteLine($" -- Displaying {(filter == null ? "all" : $"filtered with {filter}")} (Page {curPage + 1}/{(displayedUsers.Count - 1) / 10 + 1})");
                DrawLine('-');
                foreach (var account in displayedUsers.GetRange(curPage * 10, Math.Min(displayedUsers.Count - curPage * 10, 10)))
                    WriteLine($" - ID: {account.UserID}, Username: {account.Username}");
            }

            DrawLine('-');
            if (filteredByID == null)
                WriteLine(" 'Left/Right' to change pages");
            WriteLine(" 'ESC' to return");
            WriteLine(" '0' to refresh");
            WriteLine(" '1' to filter by username");
            WriteLine(" '2' to filter by ID");
            WriteLine(" 'DEL' to delete filtered account(s)");

            ConsoleKey key = ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    int codeInd = 0;
                    ConsoleKey input;
                    while (true)
                    {
                        input = ReadKey(true).Key;

                        if (input != konamiCode[codeInd])
                            break;

                        if (++codeInd == konamiCode.Length)
                        {
                            await Helper.AccountFiller();
                            DrawLine('-');
                            WriteLine(" Accounts filled.");
                            ReadKey(true);

                            goto refreshList;
                        }
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (curPage > 0)
                        curPage--;
                    break;

                case ConsoleKey.RightArrow:
                    if (curPage < (displayedUsers.Count - 1) / 10)
                        curPage++;
                    break;

                case ConsoleKey.Escape:
                    return;

                case ConsoleKey.D0:
                case ConsoleKey.NumPad0:
                refreshList:
                    (allUsers, error) = await UserDB.GetAll();
                    if (allUsers == null)
                    {
                        WriteLine(" Error: " + error);
                        ReadKey(true);
                        return;
                    }

                    displayedUsers = allUsers;
                    filteredByID = null;
                    filter = null;
                    curPage = 0;
                    break;

                
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    DrawLine('-');
                    Write(" Enter username: ");
                    filter = ReadInput();
                    if (string.IsNullOrWhiteSpace(filter))
                    {
                        displayedUsers = allUsers;
                        filter = null;
                    }
                    else
                    {
                        displayedUsers = [.. allUsers!.Where(a => a.Username.Contains(filter, StringComparison.Ordinal))];

                        if (displayedUsers.Count == 0)
                        {
                            displayedUsers = allUsers;
                            filter = null;

                            WriteLine(" (!) No accounts found.");
                            ReadKey(true);
                        }
                        else
                        {
                            filteredByID = null;
                            curPage = 0;
                        }
                    }
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    DrawLine('-');
                    Write(" Enter ID: ");
                    string? id = ReadInput();

                    if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out int userID))
                    {
                        WriteLine(" (!) Invalid ID.");
                        ReadKey(true);
                        continue;
                    }

                    (filteredByID, error) = await UserDB.Get(userID);
                    if (filteredByID == null)
                    {
                        WriteLine($" (!) {error}");
                        ReadKey(true);
                        continue;
                    }
                    break;

                case ConsoleKey.Delete:
                    DrawLine('-');

                    if (filteredByID == null && filter == null)
                    {
                        WriteLine(" (!) No accounts to delete.");
                        ReadKey(true);
                        continue;
                    }

                    WriteLine(" (!) This action is irreversible.");
                    WriteLine(" (!) All data associated with the account(s) will be deleted.");
                    WriteLine();

                    Write(" 'ENTER' ");
                    var (left, top) = Helper.InputWaitTime(3);
                    SetCursorPosition(left, top);
                    Write("to confirm deletion: ");

                    if (ReadKey(true).Key == ConsoleKey.Enter)
                    {
                        WriteLine();

                        if (filteredByID != null)
                        {
                            (bool success, error) = await UserDB.Delete(filteredByID.UserID);

                            if (!success)
                            {
                                WriteLine($" (!) Deletion failed: {error}");
                                ReadKey(true);
                            }
                            else
                            {
                                WriteLine(" Account deleted.");
                                ReadKey(true);
                                filteredByID = null;
                            }
                        }
                        else if (filter != null)
                        {
                            (bool success, error) = await UserDB.DeleteAll(filter);

                            if (!success)
                            {
                                WriteLine($" (!) Deletion failed: {error}");
                                ReadKey(true);
                            }
                            else
                            {
                                WriteLine(" Account(s) deleted.");
                                ReadKey(true);
                                displayedUsers = allUsers;
                                filter = null;
                            }
                        }

                        goto refreshList;
                    }
                    break;
            }
        }
    }

    public static void GameDataMenu()
    {
        Clear();
        DrawHeader(Header);
        WriteLine(" -- Modifying game data");
        WriteLine(" 'ESC' or 0 to return");
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
                    WriteLine(" (!) No properties found.");
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
                WriteLine(" (!) Operation cancelled.");
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
            WriteLine(" 'ESC' or 0 to return");
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
                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
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
    
    private static class Helper
    {
        public static bool CheckIPv4(string? ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out _))
                return false;

            string[] parts = ipAddress.Split('.');
            if (parts.Length != 4) return false;

            foreach(string part in parts)
            {
                if (!int.TryParse(part, out int number))
                    return false;

                if (number < 0 || number > 255)
                    return false;

                if (part.Length > 1 && part[0] == '0')
                    return false;
            }

            return true;
        }

        public static bool CheckPort(string? port)
        {
            if (!int.TryParse(port, out int number))
                return false;

            return number >= 0 && number <= 65535;
        }

        public static (int left, int top) InputWaitTime(int waitTime)
        {
            var (left, top) = (CursorLeft, CursorTop);

            for (int i = waitTime; i > 0; i--)
            {
                CursorLeft = left;
                CursorTop = top;
                Write($"after {i}s...");

                Task.Delay(1000).Wait();
            }

            while (KeyAvailable)
                ReadKey(true);

            return (left, top);
        }

        public static async Task AccountFiller()
        {
            Random random = new();
            for (int i = 0; i < 20; i++)
            {
                string name = $"throwaway{DateTime.Now:yyyyMMddHHmmss}{random.Next(1000, 9999)}";
                await UserDB.Add(new()
                {
                    Username = name,
                    Nickname = name,
                    Email = name,
                    PwdSet = ServerUtilities.Security.HashPassword("password")
                });
            }
        }
    }
}