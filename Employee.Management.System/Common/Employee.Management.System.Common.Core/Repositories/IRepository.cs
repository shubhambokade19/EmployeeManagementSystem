using Employee.Management.System.Common.Api;

namespace Employee.Management.System.Common.Core.Repositories
{
    public interface IRepository<TEntity>
    {
        string TableName { get; }
        string PrimaryKeyFieldName { get; }

        #region insert, update, delete 
        Task<long> InsertAsync(Session session, TEntity entity);
        Task<long> InsertAsync(Session session, object? entity);
        Task<bool> UpdateAsync(Session session, TEntity entity);
        Task<bool> UpdateAsync(Session session, object? entity);
        Task<int> UpdateBySqlStatementAsync(Session session, string sqlStatement, object parameters);
        Task<int> InsertBySqlStatementAsync(Session session, string sqlStatement, object parameters);
        Task<bool> UpdateActiveAsync(Session session, string commaSeperatedIdList, short active);
        Task<bool> DeleteAsync(Session session, long id);
        Task<bool> BulkDeleteAsync(Session session, string? ids);
        Task<List<TEntity>> BulkInsertAsync(Session session, List<TEntity> entities);
        #endregion

        #region search
        Task<TEntity?> GetByIdAsync(Session session, long id);
        Task<List<TEntity>> GetListByIdAsync(Session session, string idList);
        Task<IEnumerable<TEntity>> GetBySqlStatementAsync(Session session, string sqlStatement);
        Task<IEnumerable<TType>> GetBySqlStatementAsync<TType>(Session session, string sqlStatement, object? parameters = null);
        Task<IEnumerable<TType>> GetBySqlStatementAsync<TType>(Session session, string sqlStatement);
        Task<TType?> GetFieldsByIdAsync<TType>(Session session, long id, string fieldList);
        Task<List<TEntity>> GetListByFieldIdAsync(Session session, string? tableName, string? fieldName, string? fieldId);
        #endregion
    }
}
