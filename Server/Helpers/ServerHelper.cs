using System.Net;
using System.Net.Sockets;

using static System.Console;

class ServerHelper
{
    public static void StartServerMenu(string? serverIP, int port)
    {
        Clear();
        UIHelper.DrawHeader("Server Control Center");
        WriteLine($" Server's IP: {serverIP ?? "Any"}");
        WriteLine($" Server's port: {port}");
        UIHelper.DrawLine('-');
        WriteLine(" 1. Start server");
        WriteLine(" 2. Change IP");
        WriteLine(" 3. Change port");
        WriteLine(" 4. View activity log");
        WriteLine(" 0. Shut down");
        UIHelper.DrawLine('-');
        Write(" Enter choice: ");
    }

    public static void OverviewMenu(string? serverIP, int port)
    {
        Clear();
        UIHelper.DrawHeader("Server Control Center");
        WriteLine(" Server is online");
        WriteLine($" Address: {serverIP ?? "Any"}");
        WriteLine($" Port: {port}");
        UIHelper.DrawLine('-');
        WriteLine(" 1. View connected clients");
        WriteLine(" 2. View activity log");
        WriteLine(" 0. Shut down server");
        UIHelper.DrawLine('-');
        Write(" Enter choice: ");
    }

    public static void ConnectedClients(List<ClientHandler> clients, int curPage)
    {
        Clear();
        UIHelper.DrawHeader("Server Control Center");
        WriteLine($" List of connected clients (Page {curPage + 1}/{(clients.Count - 1) / 10 + 1}):");
        UIHelper.DrawLine('-');

        foreach(var client in clients.GetRange(curPage * 10, Math.Min(clients.Count - curPage * 10, 10)))
        {
            WriteLine($" • {client.EndPoint} (logged in as: '{client.User?.ToString() ?? "None"}'");
        }

        UIHelper.DrawLine('-');
        WriteLine(" 8. Previous page");
        WriteLine(" 9. Next page");
        WriteLine(" 0. Return");
        UIHelper.DrawLine('-');
        Write(" Enter Choice: ");
    }

    // Return false when leaving viewer
    public static bool ViewLog()
    {
        Clear();
        UIHelper.DrawHeader("Server Control Center");
        WriteLine(" Viewing activity log");
        WriteLine(" 'DEL' to clear log");
        WriteLine(" 'ESC' to return");
        UIHelper.DrawLine('-');
        
        LogHandler.WriteAllLog();
        LogHandler.ToggleLogView(true);

        while(true)
        {
            ConsoleKey key = ReadKey(true).Key;

            switch(key)
            {
                case ConsoleKey.Escape:
                    LogHandler.ToggleLogView(false);
                    return false;

                case ConsoleKey.Delete:
                    LogHandler.ClearLog();
                    return true;
            }
        }
    }
    
    public static bool CheckIPv4(string? ipAddress)
    {
        if(!IPAddress.TryParse(ipAddress, out _))
            return false;

        string[] parts = ipAddress.Split('.');
        if(parts.Length != 4) return false;

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

}