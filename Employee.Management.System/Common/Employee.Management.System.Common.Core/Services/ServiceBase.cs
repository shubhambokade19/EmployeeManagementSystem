using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Core.Models;
using Employee.Management.System.Common.Logging;

namespace Employee.Management.System.Common.Core.Services
{
    public abstract class ServiceBase<TEntity> : IService<TEntity>
    {
        protected string _entityName = "ServiceBase";

        public abstract Task<bool> DeleteAsync(Session session, long id);

        public async virtual Task<TEntity> SaveAsync(Session session, object? entity, bool returnSavedEntity = false)
        {
            var modelBase = (ModelBase?)entity;
            var methodName = "ServiceBase.SaveAsync";
            var logContext = new LogContext(session, $"{methodName}");
            var methodContext = $"Saving {_entityName} {modelBase?.Label}";
            logContext.StartDebug($"Started {methodContext}");
            try
            {
                bool startedTransaction = await session.BeginTransaction().ConfigureAwait(false);

                if (modelBase?.Value == 0 && entity != null)
                {
                    entity = await InsertAsync(session, (TEntity)entity, returnSavedEntity).ConfigureAwait(false);
                }
                else if (entity != null)
                {
                    entity = await UpdateAsync(session, (TEntity)entity, returnSavedEntity).ConfigureAwait(false);
                }

                // Commit the transaction ...
                session.CommitTransaction(startedTransaction);

                // Fetch saved entity ONLY AFTER the transaction has been committed ...
                if (returnSavedEntity) entity = await GetByIdAsync(session, modelBase.Value).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                session.RollbackTransaction();
                LogHelper.Error(logContext, ex);
                throw;
            }
            finally
            {
                logContext.StopDebug($"Completed {methodContext}");
            }
            return entity is TEntity result ? result : throw new InvalidCastException($"The entity is not of type {typeof(TEntity).Name}");
        }

        protected abstract Task<TEntity> InsertAsync(Session session, TEntity entity, bool returnSavedEntity = false);

        protected abstract Task<TEntity> UpdateAsync(Session session, TEntity entity, bool returnSavedEntity = false);

        public abstract Task<bool> ActivateAsync(Session session, string[] idList);

        public abstract Task<bool> InactivateAsync(Session session, string[] idList);

        public abstract Task<TEntity> GetByIdAsync(Session session, long id, bool includeEmbeddedObjects = false);

        public abstract Task<IEnumerable<TEntity>> GetBySearchAsync(Session session, SearchRequest searchRequest);
    }
}
