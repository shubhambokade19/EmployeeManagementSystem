using Dapper;
using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Core.Models;
using Employee.Management.System.Common.Logging;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static Dapper.SqlMapper;

namespace Employee.Management.System.Common.Core.Repositories
{
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity>
    {
        protected string tableName = string.Empty;
        public string TableName => tableName;
        protected string primaryKeyFieldName = string.Empty;
        public string PrimaryKeyFieldName => primaryKeyFieldName;

        protected string[]? AllowedIntentList;
        protected Dictionary<string, string>? AllowedSearchFields;

        public virtual async Task<long> InsertAsync(Session session, TEntity entity)
        {
            return await InsertAsync(session, (object?)entity);
        }

        public virtual async Task<long> InsertAsync(Session session, object? entity)
        {
            var methodName = MethodBase.GetCurrentMethod();
            var logContext = new LogContext(session, $"{methodName}");
            var methodContext = $"Insert";
            logContext.StartDebug($"Started {methodContext}");

            // Set InsertUserId and InsertTimestamp
            if (entity is ModelBase modelBase)
            {
                modelBase.SetDataContextFields(session);
            }

            // create insert statement
            var sqlStatement = GetInsertStatement(session) ?? string.Empty;

            LogHelper.Trace(logContext, $"{Environment.NewLine}Sql : {sqlStatement}");

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);

            var id = await connection.ExecuteScalarAsync<long>(sqlStatement, entity, session.Transaction);
            logContext.StopDebug($"Completed {methodContext}");
            return id;
        }

        protected virtual string? GetInsertStatement(Session session)
        {
            var sql = @$"
                SET SESSION group_concat_max_len = 1000000;
                SELECT CONCAT('INSERT INTO {tableName} (',
                    GROUP_CONCAT(DISTINCT COLUMN_NAME SEPARATOR ', '),
                    ') VALUES (',
                    GROUP_CONCAT(DISTINCT CONCAT('@', REPLACE(COLUMN_NAME, ' ', '')) SEPARATOR ', '),
                    '); SELECT LAST_INSERT_ID();'
                ) AS InsertStatement
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = '{tableName}'
                AND TABLE_SCHEMA = '{session.Transaction?.Connection?.Database}'
                AND COLUMN_NAME NOT IN ('UpdateUserId', 'UpdateTimestamp')
                AND EXTRA <> 'auto_increment';";
            sql = sql.Replace("\t", "");

            var connection = (session.GetConnectionAsync()).Result;
            var insertStatement = connection.Query<string>(sql, new { }, session.Transaction).FirstOrDefault();
            return insertStatement;
        }

        public virtual async Task<bool> UpdateAsync(Session session, TEntity entity)
        {
            return await UpdateAsync(session, (object?)entity);
        }

        public virtual async Task<bool> UpdateAsync(Session session, object? entity)
        {
            var methodName = MethodBase.GetCurrentMethod();
            var logContext = new LogContext(session, $"{methodName}");
            var methodContext = $"Update";
            logContext.StartDebug($"Started {methodContext}");

            // Set UpdateUserId and UpdateUserId
            if (entity is ModelBase modelBase)
            {
                modelBase.SetDataContextFields(session);
            }

            var sqlStatement = GetUpdateStatement(session) ?? string.Empty;
            LogHelper.Trace(logContext, $"{Environment.NewLine}Sql : {sqlStatement}");

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);
            int updatedRecordsCount = connection.Execute(sqlStatement, entity, session.Transaction);

            logContext.StopDebug($"Completed {methodContext}");
            return (updatedRecordsCount > 0);
        }

        protected virtual string? GetUpdateStatement(Session session)
        {
            var sql = @$"
                SET SESSION group_concat_max_len = 1000000;
                SELECT CONCAT('UPDATE {tableName} SET ',
                    GROUP_CONCAT(DISTINCT CONCAT(COLUMN_NAME, ' = @', REPLACE(COLUMN_NAME, ' ', '')) SEPARATOR ', '),
                    ' WHERE {primaryKeyFieldName} = @{primaryKeyFieldName}'
                ) AS UpdateStatement
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = '{tableName}' 
                AND TABLE_SCHEMA = '{session.Transaction?.Connection?.Database}'
                AND COLUMN_NAME NOT IN ('InsertUserId', 'InsertTimestamp')
                AND EXTRA <> 'auto_increment';";

            sql = sql.Replace("\t", "");

            var connection = (session.GetConnectionAsync()).Result;
            var updateStatement = connection.Query<string>(sql, new { }, session.Transaction).FirstOrDefault();

            return updateStatement;
        }

        public virtual async Task<int> UpdateBySqlStatementAsync(Session session, string sqlStatement, object parameters)
        {
            var logContext = new LogContext(session, "RepositoryBase.UpdateBySqlStatementAsync");
            var methodContext = $"Started{Environment.NewLine}{sqlStatement}";
            logContext.StartTrace(methodContext);

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);
            var updatedRecordsCount = await connection.ExecuteAsync(sqlStatement, parameters, session.Transaction).ConfigureAwait(false);

            logContext.StartTrace($"Completed");
            return updatedRecordsCount;
        }

        public virtual async Task<int> InsertBySqlStatementAsync(Session session, string sqlStatement, object parameters)
        {
            var logContext = new LogContext(session, "RepositoryBase.InsertBySqlStatementAsync");
            var methodContext = $"Started{Environment.NewLine}{sqlStatement}";
            logContext.StartTrace(methodContext);

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);
            var insertedRecordsCount = await connection.ExecuteAsync(sqlStatement, parameters, session.Transaction).ConfigureAwait(false);

            logContext.StartTrace($"Completed");
            return insertedRecordsCount;
        }

        public virtual async Task<List<TEntity>> BulkInsertAsync(Session session, List<TEntity> entities)
        {
            // Cast List<TEntity> to List<object>
            var entityList = entities.Cast<object>().ToList();

            // Call the generic BulkInsertAsync method
            var result = await BulkInsertAsync(session, entityList);

            // Cast the result back to List<TEntity>
            return result.Cast<TEntity>().ToList();
        }

        public virtual async Task<List<object>> BulkInsertAsync(Session session, List<object> entities)
        {
            var methodName = MethodBase.GetCurrentMethod();
            var logContext = new LogContext(session, $"{methodName}");
            var methodContext = $"Bulk Insert";
            logContext.StartDebug($"Started {methodContext}");

            // Set InsertUserId and InsertTimestamp for all entities
            foreach (var entity in entities.OfType<ModelBase>())
            {
                entity.SetDataContextFields(session);
            }

            // create insert statement
            var sqlStatement = GetBulkInsertStatement(session, entities);

            // Build dynamic parameters
            var dynamicParameters = new DynamicParameters();
            int index = 0;
            foreach (var entity in entities)
            {
                var entityType = entity.GetType();
                entityType.GetProperties().ToList().ForEach(property =>
                {
                    var parameterName = $"@{property.Name}_{index}";
                    dynamicParameters.Add(parameterName, property.GetValue(entity));
                });
                index++;
            }

            LogHelper.Trace(logContext, $"{Environment.NewLine}Sql : {sqlStatement}");

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);

            await connection.ExecuteScalarAsync<bool>(sqlStatement, dynamicParameters, session.Transaction);
            logContext.StopDebug($"Completed {methodContext}");
            return entities;
        }

        public virtual async Task<bool> DeleteAsync(Session session, long id)
        {
            var methodName = MethodBase.GetCurrentMethod();
            var logContext = new LogContext(session, $"{methodName}");
            logContext.StartDebug($"Started {methodName} for Id = {id}");

            var sqlStatement = $@"DELETE FROM {tableName} WHERE {primaryKeyFieldName} = @Id;";

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);
            int iRecordsAffected = connection.Execute(sqlStatement, new { Id = id }, session.Transaction);

            logContext.StopDebug($"Completed {methodName} for Id = {id}");
            return (iRecordsAffected > 0);
        }

        public virtual async Task<bool> BulkDeleteAsync(Session session, string? ids)
        {
            var methodName = MethodBase.GetCurrentMethod();
            var logContext = new LogContext(session, $"{methodName}");
            logContext.StartDebug($"Started {methodName} for Ids = {ids}");

            var Ids = string.Join(",", ids.Split(',').Select(x => $"'{x.Trim()}'"));
            var sqlStatement = $@"DELETE FROM {tableName} WHERE {primaryKeyFieldName} IN ({Ids});";

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);
            int iRecordsAffected = connection.Execute(sqlStatement, new { }, session.Transaction);

            logContext.StopDebug($"Completed {methodName} for Ids = {ids}");
            return (iRecordsAffected > 0);
        }

        protected virtual async Task<int> DeleteBySqlStatementAsync(Session session, string sqlStatement, object parameters)
        {
            var logContext = new LogContext(session, "RepositoryBase.DeleteBySqlStatementAsync");
            var methodContext = $"Started{Environment.NewLine}{sqlStatement}";
            logContext.StartTrace(methodContext);

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);
            var iRecordsAffected = await connection.ExecuteAsync(sqlStatement, parameters, session.Transaction).ConfigureAwait(false);

            logContext.StartTrace($"Completed");
            return iRecordsAffected;
        }

        public virtual async Task<TEntity?> GetByIdAsync(Session session, long id)
        {
            var methodName = "RepositoryBase.GetByIdAsync";
            var logContext = new LogContext(session, $"{methodName}");
            logContext.StartDebug($"Started {methodName} for Id = {id}");

            var sql = $@"
					SELECT 
					e.*
					FROM {tableName} e
					WHERE e.{primaryKeyFieldName} = @Id
				;";
            sql = sql.Replace("\t", "");

            var methodContext = $"Query = {sql}";
            logContext.StartTrace($"Started {methodContext}");

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);
            var entity = connection.Query<TEntity>(sql, new { id }, session.Transaction).FirstOrDefault();

            logContext.StopDebug($"Completed {methodName} for Id = {id}");
            logContext.StopTrace($"Completed {methodContext}");
            return entity;
        }

        public virtual async Task<List<TEntity>> GetListByIdAsync(Session session, string idList)
        {
            var methodName = "RepositoryBase.GetListByIdAsync";
            var logContext = new LogContext(session, $"{methodName}");
            logContext.StartDebug($"Started {methodName} for Id = {idList}");

            var sql = $@"
					SELECT 
					e.*
					FROM {tableName} e
					WHERE e.{primaryKeyFieldName} IN ({idList})
				;";
            sql = sql.Replace("\t", "");

            var methodContext = $"Query = {sql}";
            logContext.StartTrace($"Started {methodContext}");

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);
            var entity = connection.Query<TEntity>(sql, new { }, session.Transaction).ToList();

            logContext.StopDebug($"Completed {methodName} for Id = {idList}");
            logContext.StopTrace($"Completed {methodContext}");
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> GetBySqlStatementAsync(Session session, string sqlStatement)
        {
            var methodName = MethodBase.GetCurrentMethod();
            var methodContext = $"Query = {sqlStatement}";
            var logContext = new LogContext(session, $"{methodName}");

            logContext.StartDebug($"Started {methodName}");
            logContext.StartTrace($"Started {methodContext}");

            IDbConnection connection = await session.GetConnectionAsync().ConfigureAwait(false);

            var list = await connection.QueryAsync<TEntity>(sqlStatement, transaction: session.Transaction);

            logContext.StopDebug($"Completed {methodName}");
            logContext.StopTrace($"Completed {methodContext}");
            return list;
        }

        public virtual async Task<IEnumerable<T>> GetBySqlStatementAsync<T>(Session session, string sqlStatement, object? parameters = null)
        {
            var methodName = MethodBase.GetCurrentMethod();
            var methodContext = $"Query = {sqlStatement}";
            var logContext = new LogContext(session, $"{methodName}");

            logContext.StartDebug($"Started {methodName}");
            logContext.StartTrace($"Started {methodContext}");

            IDbConnection connection = await session.GetConnectionAsync().ConfigureAwait(false);

            IEnumerable<T> list;
            if (parameters == null)
            {
                list = await connection.QueryAsync<T>(sqlStatement, transaction: session.Transaction);
            }
            else
            {
                list = await connection.QueryAsync<T>(sqlStatement, parameters, session.Transaction);
            }

            logContext.StopDebug($"Completed {methodName}");
            logContext.StopTrace($"Completed {methodContext}");
            return list;
        }

        public virtual async Task<bool> UpdateActiveAsync(Session session, string commaSeperatedIdList, short active)
        {
            var logContext = new LogContext(session, "StoreRepository.UpdateActiveAsync");
            var methodContext = $"Ids = {commaSeperatedIdList}, Active = {active}";
            logContext.StartDebug($"Started {methodContext}");

            var sqlStatement = $@"UPDATE {tableName} 
			SET Active = @Active
			, UpdateUserId = @UpdateUserId
			, UpdateTimeStamp = @UpdateTimeStamp
			WHERE {primaryKeyFieldName} IN ( {commaSeperatedIdList} );";

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);
            int iRecordsAffected = connection.Execute(sqlStatement, new
            {
                Active = active,
                session.UpdateUserId,
                session.UpdateTimestamp
            }, session.Transaction);

            logContext.StartDebug($"Completed {methodContext}");
            return (iRecordsAffected > 0);
        }

        public virtual async Task<T?> GetFieldsByIdAsync<T>(Session session, long id, string fieldList)
        {
            var methodName = MethodBase.GetCurrentMethod();
            var methodContext = $"FieldList = {fieldList}";
            var logContext = new LogContext(session, $"{methodName}");

            var sqlStatement = $@"SELECT {fieldList} FROM {tableName} WHERE {PrimaryKeyFieldName} = {id}";

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);
            var data = (await connection.QueryAsync<T>(sqlStatement, transaction: session.Transaction)).FirstOrDefault();

            logContext.StopDebug($"Completed {methodName}");
            logContext.StopTrace($"Completed {methodContext}");
            return data;
        }

        public virtual async Task<IEnumerable<T>> GetBySqlStatementAsync<T>(Session session, string sqlStatement)
        {
            var methodName = MethodBase.GetCurrentMethod();
            var methodContext = $"Query = {sqlStatement}";
            var logContext = new LogContext(session, $"{methodName}");

            logContext.StartDebug($"Started {methodName}");
            logContext.StartTrace($"Started {methodContext}");

            IDbConnection connection = await session.GetConnectionAsync().ConfigureAwait(false);

            var list = (await connection.QueryAsync<T>(sqlStatement, transaction: session.Transaction)).ToList();

            // Check for LIMIT clause and calculate total count if present
            if (ContainsLimitClause(sqlStatement))
            {
                var countQuery = GenerateCountQuery(sqlStatement);
                var totalCount = (await connection.QueryAsync<int>(countQuery, transaction: session.Transaction)).FirstOrDefault();

                // Map TotalCount for each item in the list
                if (typeof(T).GetProperty("TotalCount") is PropertyInfo totalCountProperty)
                {
                    list.ForEach(item => totalCountProperty.SetValue(item, totalCount));
                }
            }

            logContext.StopDebug($"Completed {methodName}");
            logContext.StopTrace($"Completed {methodContext}");
            return list;
        }

        private bool ContainsLimitClause(string sqlStatement) =>
            Regex.IsMatch(sqlStatement, @"\sLIMIT\s", RegexOptions.IgnoreCase) || sqlStatement.TrimEnd().EndsWith("LIMIT", StringComparison.OrdinalIgnoreCase);

        private string GenerateCountQuery(string sqlStatement)
        {
            string pattern = @"\sLIMIT\s+.*$";

            string trimmedStatement = Regex.Replace(sqlStatement, pattern, "", RegexOptions.Multiline | RegexOptions.IgnoreCase).Trim();

            // Find the FROM clause
            int fromIndex = trimmedStatement.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);
            if (fromIndex < 0) return string.Empty;

            // Check if there is a GROUP BY clause and a WHERE clause does not follow it
            bool hasGroupBy = trimmedStatement.LastIndexOf("GROUP BY", StringComparison.OrdinalIgnoreCase) > -1 &&
                              !trimmedStatement.Substring(trimmedStatement.LastIndexOf("GROUP BY", StringComparison.OrdinalIgnoreCase))
                                  .Contains("WHERE", StringComparison.OrdinalIgnoreCase);

            string countQuery;

            if (hasGroupBy)
                countQuery = $"SELECT COUNT(1) FROM (SELECT COUNT(1) {trimmedStatement.Substring(fromIndex)}) AS o";
            else
                countQuery = $"SELECT COUNT(1) {trimmedStatement.Substring(fromIndex)}";

            return countQuery.Trim();
        }

        public string GetBulkInsertStatement(Session session, List<object> entityList)
        {
            StringBuilder insertStatement = new StringBuilder();

            var sql = @$"
                SET SESSION group_concat_max_len = 1000000;
                SELECT CONCAT(
                    'INSERT INTO {tableName} (',
                    GROUP_CONCAT(DISTINCT COLUMN_NAME ORDER BY ORDINAL_POSITION SEPARATOR ', '),
                    ') VALUES '
                ) AS InitialInsert
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = '{tableName}'
                    AND TABLE_SCHEMA = '{session.Transaction?.Connection?.Database}'
                    AND COLUMN_NAME NOT IN ('UpdateUserId', 'UpdateTimestamp')
                    AND EXTRA <> 'auto_increment'
                GROUP BY TABLE_NAME;
            ";
            sql = sql.Replace("\t", "");

            var connection = (session.GetConnectionAsync()).Result;
            var initialInsertStatement = connection.Query<string>(sql, new { }, session.Transaction).FirstOrDefault();

            // Extract column names
            var columnNames = initialInsertStatement?.Split('(', ')')[1].Split(',').Select(col => col.Trim()).ToList();

            // Check columns exist or not
            if (columnNames == null || !columnNames.Any())
                throw new InvalidOperationException("No column names found for the insert statement.");

            // Construct the values part of the INSERT statement and StringBuilder
            insertStatement.Append(initialInsertStatement);
            insertStatement.Append(string.Join(", ", entityList.Select((entity, index) => $"({string.Join(", ", columnNames.Select(col => $"@{col}_{index}"))})")));

            return insertStatement.ToString();
        }

        public virtual async Task<List<TEntity>> GetListByFieldIdAsync(Session session, string? tableName, string? fieldName, string? fieldId)
        {
            var methodName = "RepositoryBase.GetListByFieldIdAsync";
            var logContext = new LogContext(session, $"{methodName}");
            logContext.StartDebug($"Started {methodName} for Table = {tableName}, Field = {fieldName}, FieldId = {fieldId}");

            // Create the SQL query
            var sql = $@"
                SELECT 
                e.*
                FROM {tableName} e
                WHERE e.{fieldName} IN ({fieldId})
            ";

            sql = sql.Replace("\t", "");

            var methodContext = $"Query = {sql}";
            logContext.StartTrace($"Started {methodContext}");

            var connection = await session.GetConnectionAsync().ConfigureAwait(false);
            var result = (await connection.QueryAsync<TEntity>(sql, new { }, session.Transaction)).ToList();

            logContext.StopDebug($"Completed {methodName} for Table = {tableName}, Field = {fieldName}, FieldId = {fieldId}");
            logContext.StopTrace($"Completed {methodContext}");
            return result;
        }
    }
}
