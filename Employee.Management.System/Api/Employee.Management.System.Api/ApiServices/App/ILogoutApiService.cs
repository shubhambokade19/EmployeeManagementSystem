using Employee.Management.System.ApiModels.App;
using Employee.Management.System.Domain.Models.App;

namespace Employee.Management.System.Api.ApiServices.App
{
    public interface ILogoutApiService : IApiService
    {
        Task<bool> LogoutAsync(LogoutApiModel model);
    }
}
