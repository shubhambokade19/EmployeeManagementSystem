using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Core.Repositories;
using Employee.Management.System.Common.Logging;
using Employee.Management.System.Domain.Core.Repositories.App;
using Employee.Management.System.Domain.Models;
using Employee.Management.System.Domain.Models.App;

namespace Employee.Management.System.Domain.Infrastructure.Repositories.App
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        private const string TABLENAME = "Users";
        private const string PRIMARYKEY_FIELDNAME = "UserId";

        public UserRepository()
        {
            tableName = TABLENAME;
            primaryKeyFieldName = PRIMARYKEY_FIELDNAME;
        }

        public async Task<User?> GetUserByUserLoginAndPasswordAsync(Session session, string? userLogin, string? password)
        {
            var methodName = "UserRepository.GetUserSubscriberBySubscriberCodeAsync";
            var logContext = new LogContext(session, $"{methodName} for UserLogin = {userLogin}");
            logContext.StartDebug($"Started {methodName}");

            var sql = $@"
                SELECT u.UserId
                , u.UserLogin
                , u.UserPassword
                , u.RealName
                FROM {TABLENAME} AS u
                WHERE u.UserLogin = @UserLogin
                AND u.UserPassword = @UserPassword
                AND u.Active = {YesNoType.YES};
            ";
            sql = sql.Replace("\t", "");

            var result = (await GetBySqlStatementAsync<User>(session, sql, new { UserLogin = userLogin, UserPassword = password }).ConfigureAwait(false)).FirstOrDefault();

            logContext.StopDebug($"Completed {methodName} for UserLogin = {userLogin}");
            return result;
        }

        public async Task<User?> GetUserByUserLoginAsync(Session session, string? userLogin, string? selfUserId)
        {
            var methodName = "UserRepository.GetUserByUserLoginAsync";
            var logContext = new LogContext(session, $"{methodName} for UserLogin = {userLogin}");
            logContext.StartDebug($"Started {methodName}");

            var sql = $@"
                SELECT u.UserId
                , u.UserLogin
                , u.UserPassword
                , u.RealName
                FROM {TABLENAME} AS u
                WHERE u.UserLogin = @UserLogin
                AND u.Active = {YesNoType.YES}
            ";
            if (!string.IsNullOrEmpty(selfUserId))
            {
                sql += " AND u.UserId <> @SelfUserId";
            }
            sql = sql.Replace("\t", "");

            var result = (await GetBySqlStatementAsync<User>(session, sql, new { UserLogin = userLogin, SelfUserId = selfUserId }).ConfigureAwait(false)).FirstOrDefault();

            logContext.StopDebug($"Completed {methodName} for UserLogin = {userLogin}");
            return result;
        }

        public async Task<bool> UpdateUserPasswordAsync(Session session, User user)
        {
            var methodName = "UserRepository.UpdateUserPasswordAsync";
            var logContext = new LogContext(session, $"{methodName}");
            logContext.StartDebug($"Started {methodName} for user = {user}");

            var sql = $@"UPDATE {TABLENAME}
                         SET UserPassword = @UserPassword
                         WHERE UserLogin = @UserLogin;";

            int rows = await UpdateBySqlStatementAsync(session, sql, new { user.UserLogin, user.UserPassword }).ConfigureAwait(false);
            logContext.StopDebug($"Completed {methodName} user = {user}");
            return rows > 0;
        }
    }
}
