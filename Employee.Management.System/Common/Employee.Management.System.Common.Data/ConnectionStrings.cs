using Microsoft.Extensions.Configuration;

namespace Employee.Management.System.Common.Data
{
    public class ConnectionStrings
    {
        private readonly Dictionary<string, string>? _connectionStringsDictionary;

        public ConnectionStrings(IConfiguration config)
        {
            var ob = config.GetSection("ConnectionStrings");
            if (ob != null)
            {
                _connectionStringsDictionary = new Dictionary<string, string>();
                var connectionStrings = ob.AsEnumerable();
                foreach (var connectionString in connectionStrings)
                {
                    if (string.IsNullOrEmpty(connectionString.Value)) continue;
                    var key = connectionString.Key;
                    key = key.Substring(key.IndexOf(":") + 1);
                    _connectionStringsDictionary.Add(key, connectionString.Value);
                }
            }
        }

        public async Task<string> GetConnectionStringByKey(string key)
        {
            if (_connectionStringsDictionary == null || !_connectionStringsDictionary.ContainsKey(key))
            {
                throw new Api.ApiException($"No connection string found for key {key}");
            }

            // Add secrets to the connection string and then return the connection string ...
            var connectionString = _connectionStringsDictionary[key];

            return await Task.FromResult(connectionString);
        }

        public async Task<string?> GetParameterValueFromConnectionString(string? connectionString, string? parameterName)
        {
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(parameterName))
            {
                return await Task.FromResult<string?>(null);
            }
            else
            {
                int startIndexOfParameter = connectionString.IndexOf(parameterName, StringComparison.CurrentCultureIgnoreCase);
                int startIndexOfEqualTo = connectionString.IndexOf("=", startIndexOfParameter, StringComparison.CurrentCultureIgnoreCase);
                int startIndexOfSemiColon = connectionString.IndexOf(";", startIndexOfEqualTo);

                string ParameterValue = connectionString.Substring((startIndexOfEqualTo + 1), (startIndexOfSemiColon - startIndexOfEqualTo - 1));
                return await Task.FromResult(ParameterValue);
            }
        }
    }
}
