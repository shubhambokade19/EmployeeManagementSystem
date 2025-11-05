using MySqlConnector;
using System.Data;

namespace Employee.Management.System.Common.Data
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly ConnectionStrings _connectionStrings;
        public DbConnectionFactory(ConnectionStrings connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }
        public async Task<IDbConnection> GetDbConnectionAsync(string connectionString)
        {
            var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }
        public async Task<IDbConnection> GetDbConnectionAsync()
        {
            var connectionString = await _connectionStrings.GetConnectionStringByKey("Default");
            var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
