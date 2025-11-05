using System.Data;

namespace Employee.Management.System.Common.Data
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> GetDbConnectionAsync();
        Task<IDbConnection> GetDbConnectionAsync(string connectionString);
    }
}
