using BLL;
using DAL.Config;

using static System.Console;

namespace ConsolePL;

public class Program
{
    private static Server Server => Server.Instance;

    public static async Task Main()
    {
        while (true)
        {
            var (dbSuccess, dbExit) = await ServerControlUI.FillDBInfo();

            if (dbExit)
                return;

            if (dbSuccess)
                break;
        }

        int port = ServerConfig.DefaultPort;

        while (true)
        {
            try
            {
                var (success, newPort) = await InitServer(port);

                if (!success)
                    return;

                port = newPort;

                await ServerMenu(port);
            }
            catch (Exception e)
            {
                Server.Stop();
                WriteLine($"Error: {e.Message}");
                ReadKey(true);
            }
        }
    }

    static async Task<(bool success, int port)> InitServer(int port)
    {
        while (true)
        {
            ServerControlUI.MainMenu(port, Server.IsRunning);

            switch (ConsoleUtilities.ReadInput())
            {
                case "1":
                    Server.Start(port);
                    return (true, port);

                case "2":
                    port = await ServerControlUI.ManageServerAddresses(port);
                    continue;

                case "3":
                    ServerControlUI.ViewLog();
                    continue;

                case "0": case null:
                    Write(" Re-enter 'ESC' or '0' after 1s...");
                    await Task.Delay(1000);

                    while (KeyAvailable)
                        ReadKey(true);

                    CursorLeft = 0;
                    Write(" Re-enter 'ESC' or '0' to exit program: ");

                    var confirm = ConsoleUtilities.ReadInput();
                    if (confirm == "0" || confirm == null)
                    {
                        Server.Stop();
                        WriteLine(" Shutting down server...");
                        return (false, port);
                    }
                    else
                        continue;

                default: continue;
            }
        }
    }

    static async Task ServerMenu(int port)
    {
        while (true)
        {
            ServerControlUI.MainMenu(port, Server.IsRunning);

            switch (ConsoleUtilities.ReadInput())
            {
                case "1":
                    ServerControlUI.ViewConnectedClients(Server.Clients);
                    continue;

                case "2":
                    await ManageUsers();
                    continue;

                case "3":
                    await ManageGameData();
                    continue;

                case "4":
                    ServerControlUI.ViewLog();
                    continue;

                case "0": case null:
                    Server.Stop();
                    WriteLine(" Shutting down server...");
                    ReadKey(true);
                    return;

                default: continue;
            }
        }
    }

    static async Task ManageUsers()
        => await ServerControlUI.ManageUsers();

    static async Task ManageGameData()
    {
        while (true)
        {
            ServerControlUI.GameDataMenu();

            switch (ConsoleUtilities.ReadInput())
            {
                case "1":
                    await AssetManagerUI.Start();
                    continue;

                case "2":
                    ServerControlUI.ViewGameConfig();
                    continue;

                case "3":
                    await ServerControlUI.ModifyGameConfig();
                    continue;

                case "0": case null:
                    return;

                default: continue;
            }
        }
    }
}