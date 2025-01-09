using static System.Console;
using static ServerUIHelper;

static class ServerUI
{
    public static string Header { get; } = "Server Control Center";

    public static void FillDBInfo()
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
            throw new("Cancelled initializing MySql connection");
        if (string.IsNullOrWhiteSpace(server))
            server = "localhost";

        Write(" Database (default 'consoleconquer'): ");
        if ((db = ReadInput()) == null)
            throw new("Cancelled initializing MySql connection");
        if (string.IsNullOrWhiteSpace(db))
            db = "consoleconquer";

        Write(" UID (default 'root'): ");
        if ((uid = ReadInput()) == null)
            throw new("Cancelled initializing MySql connection");
        if (string.IsNullOrWhiteSpace(uid))
            uid = "root";

        Write(" Password (default empty): ");
        if ((password = ReadInput(true)) == null)
            throw new("Cancelled initializing MySql connection");

        DrawLine('-');

        if (!DBManager.Initialize(server, db, uid, password, out string errorMessage))
            throw new($" Error while connecting to MySql DB: {errorMessage}");

        WriteLine(" Connect to MySql database successfully");
        
        LogHandler.Initialize();

        ReadKey(true);
        return;
    }
    
    public static void MainMenu(string? serverIP, int port, bool serverOnline)
    {
        Clear();
        DrawHeader(Header);
        WriteLine($" Server's IP: {serverIP ?? "Any"}");
        WriteLine($" Server's port: {port}");
        WriteLine($" Server's status: {(serverOnline ? "Online" : "Offline")}");
        DrawLine('-');

        if (serverOnline)
        {
            WriteLine(" 1. View connected clients");
            WriteLine(" 4. View activity log");
            WriteLine(" 0. Shut down server");
        }
        else
        {
            WriteLine(" 1. Start server");
            WriteLine(" 2. Change IP");
            WriteLine(" 3. Change port");
            WriteLine(" 4. View activity log");
            WriteLine(" 0. Exit program");
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

    public static void ViewLog()
    {
        while(true)
        {
            Clear();
            DrawHeader(Header);
            WriteLine(" Viewing activity log");
            WriteLine(" 'DEL' to clear log");
            WriteLine(" 'ESC' to return");
            DrawLine('-');
            
            LogHandler.WriteAllLog();
            LogHandler.ToggleLogView(true);

            while(true)
            {
                ConsoleKey key = ReadKey(true).Key;
                switch(key)
                {
                    case ConsoleKey.Escape:
                        LogHandler.ToggleLogView(false);
                        return;

                    case ConsoleKey.Delete:
                        LogHandler.ClearLog();
                        break;
                }
            }
        }
    }
}