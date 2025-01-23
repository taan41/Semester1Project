﻿using System.Net;
using BLL;
using DAL;
using static System.Console;

namespace ConsolePL
{
    public class Program
    {
        private static ConfigManager Config => ConfigManager.Instance;

        public static async Task Main()
        {
            if (!await ServerUI.FillDBInfoScreen())
                return;

            string? serverIP = null;
            int port = Config.ServerConfig.Port;

            while (true)
            {
                if (!InitServer(ref serverIP, ref port))
                    return;

                await Server.StartServer(serverIP, port);

                await ServerOverview(serverIP, port);

                Server.StopServer();
            }
        }

        static bool InitServer(ref string? serverIP, ref int port)
        {
            while (true)
            {
                ServerUI.MainMenuScreen(serverIP, port, false);

                switch (ServerUIHelper.ReadInput())
                {
                    case "1":
                        return true;

                    case "2":
                        Write(" Enter IP: ");
                        serverIP = ReadLine();

                        if (serverIP == null || !CheckIPv4(serverIP))
                        {
                            serverIP = null;
                            WriteLine(" Invalid IP.");
                            ReadKey(true);
                        }
                        continue;

                    case "3":
                        Write(" Enter port: ");
                        int oldPort = port;
                        try
                        {
                            port = Convert.ToInt32(ReadLine());
                            
                            if (port < 0 || port > 65535)
                                throw new FormatException();
                        }
                        catch (FormatException)
                        {
                            port = oldPort;
                            WriteLine(" Invalid port");
                            ReadKey(true);
                        }
                        continue;

                    case "4":
                        ServerUI.ViewLog();
                        continue;

                    case "0": case null:
                        WriteLine(" Exiting program...");
                        return false;

                    default: continue;
                }
            }
        }

        static async Task ServerOverview(string? serverIP, int port)
        {
            while (true)
            {
                ServerUI.MainMenuScreen(serverIP, port, true);

                switch (ServerUIHelper.ReadInput())
                {
                    case "1":
                        ServerUI.ViewConnectedClients(Server.clientList);
                        continue;

                    case "2":
                        await AssetManagerPL.Intance.Start();
                        continue;

                    case "3":
                        await ServerUI.ModifyGameConfig();
                        break;

                    case "4":
                        ServerUI.ViewLog();
                        continue;

                    case "0": case null:
                        WriteLine(" Shutting down server...");
                        ReadKey(true);
                        return;

                    default: continue;
                }
            }
        }

        private static bool CheckIPv4(string? ipAddress)
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
    }
}