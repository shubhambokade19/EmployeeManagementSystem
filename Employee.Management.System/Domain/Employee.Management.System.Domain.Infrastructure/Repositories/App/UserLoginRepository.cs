using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Core.Repositories;
using Employee.Management.System.Common.Logging;
using Employee.Management.System.Domain.Core.Repositories.App;
using Employee.Management.System.Domain.Models.App;

namespace Employee.Management.System.Domain.Infrastructure.Repositories.App
{
    public class UserLoginRepository : RepositoryBase<UserLogin>, IUserLoginRepository
    {
        private const string TABLENAME = "UserLogin";
        private const string PRIMARYKEY_FIELDNAME = "UserLoginId";

        public UserLoginRepository()
        {
            tableName = TABLENAME;
            primaryKeyFieldName = PRIMARYKEY_FIELDNAME;
        }

        public async Task<bool> IsUserLoggedInAsync(Session session, long userId)
        {
            var methodName = "UserLoginRepository.IsUserLoggedInAsync";
            var logContext = new LogContext(session, $"{methodName} for UserId = {userId}");
            logContext.StartDebug($"Started {methodName}");

            var sql = $@"
                SELECT ul.{PRIMARYKEY_FIELDNAME}
                FROM {TABLENAME} AS ul
                WHERE ul.UserId = @UserId
                AND ul.EndTimeStamp IS NULL;
            ";
            sql = sql.Replace("\t", "");

            var found = (await GetBySqlStatementAsync<object>(session, sql, new { UserId = userId }).ConfigureAwait(false)).FirstOrDefault();

            logContext.StopDebug($"Completed {methodName} for UserId = {userId}");
            return found != null;
        }

        public async Task<bool> UpdateUserLoginAsync(Session session, long userId)
        {
            var methodName = "UserLoginRepository.UpdateUserLoginAsync";
            var logContext = new LogContext(session, $"{methodName}");
            logContext.StartDebug($"Started {methodName} for UserId = {userId}");

            var sql = $@"
                UPDATE {TABLENAME}
                SET EndTimeStamp = @CurrentDateTime
                , UpdateUserId = @UpdateUserId
                , UpdateTimestamp = @UpdateTimestamp
                WHERE UserId = @UserId
                AND EndTimeStamp IS NULL;
            ";
            sql = sql.Replace("\t", "");

            var recordsAffected = await UpdateBySqlStatementAsync(session, sql, new { UserId = userId, CurrentDateTime = DateTime.Now, UpdateUserId = session.UserId, UpdateTimestamp = DateTime.Now }).ConfigureAwait(false);

            logContext.StopDebug($"Completed {methodName} for UserId = {userId}");
            return recordsAffected > 0;
        }

        public async Task<dynamic?> GetUserBySessionIdAndUserIdAsync(Session session, string? sessionId, long userId)
        {
            var methodName = "UserLoginRepository.GetUserBySessionIdAndUserIdAsync";
            var logContext = new LogContext(session, $"{methodName} for SessionId = {sessionId} and UserId = {userId}");
            logContext.StartDebug($"Started {methodName}");

            var sql = $@"
                SELECT ul.UserId
                , u.UserLogin
                , u.RealName
                FROM {TABLENAME} AS ul
                INNER JOIN Users AS u ON u.UserId = ul.UserId
                WHERE ul.SessionId = @SessionId
                AND ul.UserId = @UserId
                AND ul.EndTimeStamp IS NULL;
            ";
            sql = sql.Replace("\t", "");

            var result = (await GetBySqlStatementAsync<dynamic>(session, sql, new { SessionId = sessionId, UserId = userId }).ConfigureAwait(false)).FirstOrDefault();

            logContext.StopDebug($"Completed {methodName} for SessionId = {sessionId} and UserId = {userId}");
            return result;
        }
    }
}
