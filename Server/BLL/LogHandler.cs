using System.Runtime.CompilerServices;
using DAL.DBHandlers;
using DAL.Persistence;

namespace BLL
{
    public static class LogHandler
    {
        private static List<Log> logList = [];
        private static bool initialized = false, inLogView = false;

        public static async void Initialize()
        {
            var (oldLog, error) = await LogDB.GetAll();

            if(oldLog == null)
            {
                Console.WriteLine($" LogHandler error: {error}");
                return;
            }
            
            logList = oldLog;
            initialized = true;
        }

        public static void AddLog(string logContent, object? sourceObj, [CallerMemberName] string sourceMethod = "unknown method")
            => AddLog(logContent, sourceObj?.ToString() ?? sourceMethod);

        public static async void AddLog(string logContent, [CallerMemberName] string? sourceMethod = null)
        {
            if(!initialized)
                return;

            var (success, error) = await LogDB.Add(sourceMethod ?? "null", logContent);

            if(success)
            {
                lock(logList)
                {
                    logList.Add(new(null, sourceMethod ?? "null", logContent));
                    
                    if(inLogView)
                        Console.WriteLine(logList.Last());
                }
            }
            else
                Console.WriteLine($" LogHandler error: {error}");
        }

        public static async void ClearLog()
        {
            if(!initialized)
                return;

            lock(logList)
                logList.Clear();

            var (success, error) = await LogDB.Clear();

            if(!success)
            {
                Console.WriteLine($" LogHandler error: {error}");
                AddLog(error);
            }
        }

        public static void WriteAllLog()
        {
            if(!initialized)
                return;
            
            lock(logList)
                foreach(Log log in logList)
                    Console.WriteLine(log);
        }

        public static void ToggleLogView(bool toggle)
            => inLogView = toggle;
    }
}