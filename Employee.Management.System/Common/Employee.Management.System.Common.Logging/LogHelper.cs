using NLog;

namespace Employee.Management.System.Common.Logging
{
    public static class LogHelper
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public static void Trace(LogContext? logContext, string message)
        {
            logger.Trace(GetLogMessage(logContext, message, isTrace: true));
        }
        public static void Trace(string message)
        {
            logger.Trace(message);
        }
        public static void Information(LogContext? logContext, string message)
        {
            logger.Info(GetLogMessage(logContext, message));
        }
        public static void Info(string message)
        {
            logger.Info(message);
        }
        public static void Debug(LogContext? logContext, string message)
        {
            logger.Debug(GetLogMessage(logContext, message));
        }
        public static void Debug(string message)
        {
            logger.Debug(message);
        }
        public static void Warning(LogContext? logContext, string message)
        {
            logger.Warn(GetLogMessage(logContext, message));
        }
        public static void Error(LogContext logContext, string message)
        {
            logger.Error(GetLogMessage(logContext, message));
        }
        public static void Error(LogContext? logContext, Exception exception)
        {
            logger.Error(GetLogMessage(logContext, exception));
        }
        private static string GetLogMessage(LogContext? logContext, string message, bool isTrace = false)
        {
            return isTrace ? $"{logContext/*.GetContextMessageTrace()*/}|{message}" : $"{logContext/*.GetContextMessage()*/}|{message}";
        }
        private static string GetLogMessage(LogContext? logContext, Exception exception)
        {
            return $"{logContext/*.GetContextMessage()*/}|{exception?.Message} - {exception?.StackTrace}";
        }
        public static string GetLogMessage(string message)
        {
            return message;
        }
        private static string GetLogMessage(long storeId, string userName, Exception exception)
        {
            return $"StoreId:{storeId} | userName:{userName} | {exception?.Message} - {exception?.StackTrace}";
        }
    }
}
