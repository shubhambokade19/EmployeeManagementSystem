using Employee.Management.System.Common.Api;

namespace Employee.Management.System.Common.Core.Services
{
    public interface IService<TEntity>
    {
        Task<TEntity> SaveAsync(Session session, object? entity, bool returnSavedEntity = false);
        Task<bool> DeleteAsync(Session session, long id);
        Task<bool> ActivateAsync(Session session, string[] idList);
        Task<bool> InactivateAsync(Session session, string[] idList);
        Task<TEntity> GetByIdAsync(Session session, long id, bool includeEmbeddedObjects = false);
        Task<IEnumerable<TEntity>> GetBySearchAsync(Session session, SearchRequest searchRequest);
    }
}
