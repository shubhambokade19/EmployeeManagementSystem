using MySqlConnector;
using NLog;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace Employee.Management.System.Common.Api
{
    public class Session : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly Stopwatch stopWatch;

        public string? GetLabel()
        {
            return UserLogin;
        }
        public long GetValue()
        {
            return UserId;
        }

        public string? SessionId { get; set; }
        private string? _authToken;
        public string? AuthToken
        {
            get
            {
                if (string.IsNullOrEmpty(_authToken))
                    _authToken = Guid.NewGuid().ToString();

                return _authToken;
            }
            set
            {
                _authToken = value;
            }
        }
        public string? UserLogin { get; set; }
        public long UserId { get; set; }
        public string? RealName { get; set; }
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
        public long InsertUserId { get; set; }
        public long UpdateUserId { get; set; }
        public DateTime InsertTimestamp { get; set; }
        public DateTime? UpdateTimestamp { get; set; }

        public Session()
        {
            stopWatch = new Stopwatch();
        }

        #region Support for Database Operations ...
        private IDbConnection? _connection = null;
        public IDbTransaction? Transaction { get; private set; } = null;
        public async Task<IDbConnection> GetConnectionAsync()
        {
            // If connection is already open, return the open connection ...
            if (_connection != null && _connection.State == ConnectionState.Open) return _connection;

            // if connection is not open, create and return the connection ...
            _connection = await OpenConnection(ConnectionString);
            return _connection;
        }
        public async Task<IDbConnection> GetConnectionAsync(string connectionString)
        {
            // If connection is already open, return the open connection ...
            if (_connection != null && _connection.State == ConnectionState.Open) return _connection;

            // if connection is not open, create and return the connection ...
            _connection = await OpenConnection(connectionString);
            return _connection;
        }
        public async Task<bool> BeginTransaction()
        {
            // There is already a valid transaction, do nothing ...
            if (Transaction != null) return false;

            // Connection is null, open a new connection ...
            if (_connection == null)
            {
                _connection = await OpenConnection(ConnectionString);
            }

            // If existing connection is not open, discard the existing connection, open a new one ...
            if (_connection.State != ConnectionState.Open)
            {
                CloseAndDisposeConnection();
                _connection = await OpenConnection(ConnectionString);
            }

            // Begin a new transaction and return true ...
            Transaction = _connection?.BeginTransaction();

            InsertTimestamp = DateTime.Now;
            UpdateTimestamp = DateTime.Now;
            InsertUserId = UserId;
            UpdateUserId = UserId;
            return true;
        }
        public async Task<bool> BeginTransaction(string? connectionString)
        {
            // There is already a valid transaction, do nothing ...
            if (Transaction?.Connection != null) return false;

            CloseAndDisposeConnection();
            _connection = await OpenConnection(connectionString);

            // Begin a new transaction and return true ...
            Transaction = _connection?.BeginTransaction();

            InsertTimestamp = DateTime.Now;
            UpdateTimestamp = DateTime.Now;
            InsertUserId = UserId;
            UpdateUserId = UserId;
            return true;
        }
        public bool CommitTransaction(bool shouldCommit)
        {
            if (!shouldCommit) return true;

            Transaction?.Commit();
            Transaction = null;
            CloseAndDisposeConnection();
            return true;
        }
        public bool RollbackTransaction()
        {
            Transaction?.Rollback();
            Transaction = null;
            CloseAndDisposeConnection();
            return true;
        }
        public void CloseConnection()
        {
            // If the session is in transaction, do nothing ...
            if (Transaction != null)
            {
                return;
            }
            // If connection is already open, return the open connection ...
            CloseAndDisposeConnection();
        }
        private async Task<IDbConnection> OpenConnection(string? connectionString)
        {
            StartDebug("OpenConnection", "Started");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ApiException("Connection string is empty");
            }
            var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            StopDebug("OpenConnection", "Completed");
            return connection;
        }
        public async Task<bool> OpenConnection()
        {
            if (_connection == null)
            {
                _connection = await OpenConnection(ConnectionString);
            }

            // If existing connection is not open, discard the existing connection, open a new one ...
            if (_connection.State != ConnectionState.Open)
            {
                CloseAndDisposeConnection();
                _connection = await OpenConnection(ConnectionString);
            }
            return true;
        }

        public Session clone()
        {
            return new Session
            {
                SessionId = SessionId,
                AuthToken = AuthToken,
                UserLogin = UserLogin,
                UserId = UserId,
                RealName = RealName,
                ConnectionString = ConnectionString,
                DatabaseName = DatabaseName,
                InsertUserId = InsertUserId,
                UpdateUserId = UpdateUserId,
                InsertTimestamp = InsertTimestamp,
                UpdateTimestamp = UpdateTimestamp
            };
        }

        private void CloseAndDisposeConnection()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects) ...
                    if (Transaction != null)
                    {
                        Transaction.Rollback();
                        Transaction = null;
                    }
                    CloseAndDisposeConnection();
                }
                disposedValue = true;
            }
        }

        // Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Session()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private void StartDebug(string methodName, string message = "")
        {
            stopWatch.Reset();
            stopWatch.Start();
            if (!string.IsNullOrEmpty(message))
            {
                var fullMessage = $"{GetContextMessage(0, methodName)}|{message}";
                logger.Debug(fullMessage);
            }

        }
        private void StopDebug(string methodName, string message = "")
        {
            stopWatch.Stop();
            var fullMessage = $"{GetContextMessage(stopWatch.ElapsedMilliseconds, methodName)}|{message}";
            logger.Debug(fullMessage);
        }
        private string GetContextMessage(long timeElapsed, string methodName = "")
        {
            var message = string.IsNullOrEmpty(methodName) ? $"MethodNameNotSpecified|{timeElapsed}" : $"{methodName}|{timeElapsed}";
            message = $@"{message}|{(string.IsNullOrEmpty(this.UserLogin) ? " " : this.UserLogin)}";

            return message;
        }
    }
}
