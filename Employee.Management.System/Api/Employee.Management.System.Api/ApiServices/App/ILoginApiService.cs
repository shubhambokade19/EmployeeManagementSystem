using Employee.Management.System.ApiModels.App;
using Employee.Management.System.Domain.Models.App;

namespace Employee.Management.System.Api.ApiServices.App
{
    public interface ILoginApiService : IApiService
    {
        Task<Login> LoginAsync(LoginApiModel model);
    }
}
