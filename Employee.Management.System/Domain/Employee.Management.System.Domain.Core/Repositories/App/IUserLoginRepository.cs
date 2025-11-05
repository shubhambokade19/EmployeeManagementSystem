using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Core.Repositories;
using Employee.Management.System.Domain.Models.App;

namespace Employee.Management.System.Domain.Core.Repositories.App
{
    public interface IUserLoginRepository : IRepository<UserLogin>
    {
        Task<bool> IsUserLoggedInAsync(Session session, long userId);
        Task<bool> UpdateUserLoginAsync(Session session, long userId);
        Task<dynamic?> GetUserBySessionIdAndUserIdAsync(Session session, string? sessionId, long userId);
    }
}
