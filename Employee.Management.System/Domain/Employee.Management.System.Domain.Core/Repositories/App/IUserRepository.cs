using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Core.Repositories;
using Employee.Management.System.Domain.Models.App;

namespace Employee.Management.System.Domain.Core.Repositories.App
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetUserByUserLoginAndPasswordAsync(Session session, string? userLogin, string? encryptedPassword);
        Task<User?> GetUserByUserLoginAsync(Session session, string? userLogin, string? selfUserId = null);
        Task<bool> UpdateUserPasswordAsync(Session session, User user);
    }
}
