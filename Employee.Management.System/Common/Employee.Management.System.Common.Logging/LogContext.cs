using Employee.Management.System.Common.Api;
using Microsoft.IdentityModel.Logging;
using System.Diagnostics;
using System.Text;

namespace Employee.Management.System.Common.Logging
{
    public class LogContext
    {
        private string? _methodName;
        public Session? Session { get; }

        private readonly Stopwatch stopWatch;
        private readonly Stopwatch traceWatch;
        private readonly Stopwatch debugWatch;
        private readonly Stopwatch infoWatch;

        private string? _clientName;
        private string? _entityName;

        public string? MethodName
        {
            get => _methodName;
            set
            {
                _methodName = value;
                UpdateContextString();
            }
        }

        public string? ClientName
        {
            get => _clientName;
            set
            {
                _clientName = value;
                UpdateContextString();
            }
        }

        public string? EntityName
        {
            get => _entityName;
            set
            {
                _entityName = value;
                UpdateContextString();
            }
        }

        public LogContext()
        {
            stopWatch = new Stopwatch();
            traceWatch = new Stopwatch();
            debugWatch = new Stopwatch();
            infoWatch = new Stopwatch();
            _contextString = string.Empty;
        }

        public LogContext(string? methodName) : this()
        {
            _methodName = methodName;
            UpdateContextString();
        }

        public LogContext(string? methodName, string? clientName) : this()
        {
            _methodName = methodName;
            _clientName = clientName;
            UpdateContextString();
        }

        public LogContext(string? methodName, string? clientName, string? entityName) : this()
        {
            _methodName = methodName;
            _clientName = clientName;
            _entityName = entityName;
            UpdateContextString();
        }

        public LogContext(Session? session, string? methodName) : this(methodName)
        {
            Session = session;
        }

        private string _contextString;

        public string ContextString => _contextString;

        public void Start()
        {
            stopWatch.Reset();
            stopWatch.Start();
        }

        public void Stop()
        {
            stopWatch.Stop();
        }

        public long TimeElapsed => stopWatch.ElapsedMilliseconds;

        private void UpdateContextString()
        {
            var contextStringBuilder = new StringBuilder($"{MethodName}");
            if (!string.IsNullOrEmpty(ClientName))
            {
                if (ClientName.Length > 12)
                    contextStringBuilder.Append($"|{ClientName.Replace(' ', '_').Substring(0, 12)}");
                else
                    contextStringBuilder.Append($"|{ClientName}");
            }
            if (!string.IsNullOrEmpty(EntityName)) contextStringBuilder.Append($"|{EntityName}");
            _contextString = contextStringBuilder.ToString();
        }

        public string GetContextMessage()
        {
            var message = string.IsNullOrEmpty(MethodName) ? $"MethodNameNotSpecified|{TimeElapsed}" : $"{MethodName}|{TimeElapsed}";

            if (Session != null)
                message = $@"{message}|{(string.IsNullOrEmpty(Session.UserLogin) ? " " : Session.UserLogin)}";

            return message;
        }

        public string GetContextMessage(long timeElapsed)
        {
            var message = string.IsNullOrEmpty(MethodName) ? $"MethodNameNotSpecified|{timeElapsed}" : $"{MethodName}|{timeElapsed}";

            if (Session != null)
                message = $@"{message}|{(string.IsNullOrEmpty(Session.UserLogin) ? " " : Session.UserLogin)}";

            return message;
        }

        public string GetContextMessageTrace()
        {
            var timeElapsed = traceWatch.ElapsedMilliseconds;
            var message = string.IsNullOrEmpty(MethodName) ? $"MethodNameNotSpecified|{timeElapsed}" : $"{MethodName}|{timeElapsed}";

            if (Session != null)
                message = $@"{message}|{(string.IsNullOrEmpty(Session.UserLogin) ? " " : Session.UserLogin)}";

            return message;
        }

        public string GetLogMessage(LogContext logContext, string message)
        {
            return $"{logContext.ContextString}|{logContext.TimeElapsed}|{message}";
        }

        public string GetLogMessage(LogContext logContext, Exception exception)
        {
            return $"{logContext.ContextString}|{logContext.TimeElapsed}|EXCEPTION|{exception?.Message} - {exception?.StackTrace}";
        }

        public void StartDebug(string message = "")
        {
            debugWatch.Reset();
            debugWatch.Start();
            if (!string.IsNullOrEmpty(message))
            {
                var fullMessage = $"{GetContextMessage(0)}|{message}";
                LogHelper.Debug(fullMessage);
            }
        }

        public void StopDebug(string message = "")
        {
            debugWatch.Stop();
            var fullMessage = $"{GetContextMessage(debugWatch.ElapsedMilliseconds)}|{message}";
            LogHelper.Debug(fullMessage);
        }

        public void StartInfo(string message = "")
        {
            infoWatch.Reset();
            infoWatch.Start();
            if (!string.IsNullOrEmpty(message))
            {
                var fullMessage = $"{GetContextMessage(0)}|{message}";
                LogHelper.Info(fullMessage);
            }
        }

        public void StopInfo(string message = "")
        {
            infoWatch.Stop();
            var fullMessage = $"{GetContextMessage(infoWatch.ElapsedMilliseconds)}|{message}";
            LogHelper.Info(fullMessage);
        }

        public void StartTrace(string message)
        {
            traceWatch.Reset();
            traceWatch.Start();
            if (!string.IsNullOrEmpty(message))
            {
                var fullMessage = $"{GetContextMessage(0)}|{message}";
                LogHelper.Trace(fullMessage);
            }
        }

        public void StopTrace(string message = "")
        {
            traceWatch.Stop();
            var fullMessage = $"{GetContextMessage(traceWatch.ElapsedMilliseconds)}|{message}";
            LogHelper.Trace(fullMessage);
        }
    }
}
