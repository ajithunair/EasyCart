
namespace EasyCart.SharedLibrary.Logs
{
    public static class LogException
    {
        public static void LogExceptions(Exception ex)
        {
            LogToFIle(ex.Message);
            LogToConsole(ex.Message);
            LogToDebugger(ex.Message);
        }

        public static void LogToFIle(string message)
        {
            Serilog.Log.Information(message);
        }

        public static void LogToConsole(string message)
        {
            Serilog.Log.Warning(message);
        }

        public static void LogToDebugger(string message) => Serilog.Log.Information(message);
    }
}
