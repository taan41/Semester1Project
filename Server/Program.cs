using System.Net.Sockets;

using static System.Console;

class Server
{
    const int defaultPort = 5000;

    static TcpListener? server;

    public static async Task Main()
    {
        string? serverIP = null;
        int port = defaultPort;

        try
        {

        }
        catch (Exception ex)
        {

        }

    }


    static bool CheckMySqlConn()
    {
        string? server, db, uid, password;

        while (true)
        {
            UIHandler.DrawLine('-');
            UIHandler.WriteCenter("Server Control Center");
            UIHandler.DrawLine('-');
            WriteLine(" MySql database info:");

            Write(" Server (default 'localhost'): ");
            if (string.IsNullOrWhiteSpace(server = ReadLine()))
                server = "localhost";

            Write(" Database (default 'consoleconquer'): ");
            if (string.IsNullOrWhiteSpace(db = ReadLine()))
                db = "consoleconquer";

            Write(" UID (default 'root'): ");
            if (string.IsNullOrWhiteSpace(uid = ReadLine()))
                uid = "root";

            Write(" Password: ");
            if ((password = ReadLine()) == null)
                return false;


            if (DBHandler.Initialize(server, db, uid, password, out string errorMessage))
                WriteLine(" Connect to MySql database successfully");
            else
            {
                WriteLine($" Error while connecting to MySql DB: {errorMessage}");
                ReadKey(true);
                return false;
            }

            LogHandler.Initialize();

            ReadKey(true);
            return true;
        }
    }
}