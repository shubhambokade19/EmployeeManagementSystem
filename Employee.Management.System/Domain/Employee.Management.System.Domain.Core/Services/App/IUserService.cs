using Employee.Management.System.Common.Core.Services;
using Employee.Management.System.Domain.Models.App;

namespace Employee.Management.System.Domain.Core.Services.App
{
    public interface IUserService : IService<User>
    {
        Task<Login> LoginAsync(Login login);
        Task<bool> LogoutAsync(Logout logout);
    }
}
